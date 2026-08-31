
using System.ComponentModel;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace HARFileInvestigator
{
    public partial class Form1 : Form
    {
        private const int MaxQueryHistory = 5;
        private readonly BindingList<HarTraceEntry> _filteredEntries = [];
        private readonly List<int> _matchRowIndices = [];
        private readonly Dictionary<string, DataGridViewColumn> _dynamicColumns = new(StringComparer.OrdinalIgnoreCase);
        private readonly List<(string Name, string Header)> _columnMenuEntries = [];
        private readonly Dictionary<string, DataGridViewColumn> _columnLookupCache = new(StringComparer.Ordinal);
        private List<TagDefinition> _tagDefinitions = [];
        private List<HarTraceEntry> _allEntries = [];
        private bool _isFilterEnabled;
        private string? _currentHarFilePath;
        private readonly string _settingsFilePath;
        private AppUiSettings _settings = new();
        private int _currentMatchIndex = -1;
        private TimelineDialog? _timelineDialog;
        private readonly ContextMenuStrip _columnMenu = new();
        private readonly ToolStripTextBox _columnMenuSearchBox = new();
        private bool _suppressColumnMenuSearchChange;
        private string _lastAppliedColumnMenuSearch = string.Empty;
        private readonly ContextMenuStrip _rowContextMenu = new();
        private readonly ToolStripMenuItem _rowTagMenuItem = new("Tag");
        private readonly ToolStripMenuItem _rowDeleteMenuItem = new("Delete");

        private RichTextBox? _activeRequestSearchBox;
        private int _requestSearchPosition;
        private RichTextBox? _activeResponseSearchBox;
        private int _responseSearchPosition;
        private TextBox? _requestPaneSearchTextBox;
        private TextBox? _responsePaneSearchTextBox;

        public Form1()
        {
            InitializeComponent();
            ConfigureFilterButtonVisual();
            ConfigureColumnMenu();
            ConfigureRowContextMenu();
            ConfigurePaneSearchUi();
            ConfigureQueryBoxShortcuts();
            InitializeGrid();

            entriesGrid.DataSource = _filteredEntries;

            _settingsFilePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "HARFileInvestigator",
                "ui-settings.json");

            LoadSettings();
            ApplyTheme(darkThemeCheckBox.Checked);
        }

        private void ConfigureQueryBoxShortcuts()
        {
            queryComboBox.KeyDown += QueryComboBox_KeyDown;
        }

        private void QueryComboBox_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
            {
                return;
            }

            if (e.Modifiers == Keys.None || e.Modifiers == Keys.Control)
            {
                applyQueryButton_Click(this, EventArgs.Empty);
                e.SuppressKeyPress = true;
            }
        }

        private void ConfigureFilterButtonVisual()
        {
            filterToggleButton.TextAlign = ContentAlignment.MiddleLeft;
            filterToggleButton.ImageAlign = ContentAlignment.MiddleLeft;
            filterToggleButton.TextImageRelation = TextImageRelation.ImageBeforeText;
            filterToggleButton.Padding = new Padding(3, 0, 3, 0);
            var textHeight = TextRenderer.MeasureText("Filter", filterToggleButton.Font).Height;
            var iconSize = Math.Max(12, textHeight);
            filterToggleButton.Image = CreateFunnelIcon(iconSize);
            UpdateFilterToggleButtonText();
        }

        private static Bitmap CreateFunnelIcon(int size)
        {
            var bitmap = new Bitmap(size, size);
            using var g = Graphics.FromImage(bitmap);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using var brush = new SolidBrush(Color.FromArgb(90, 90, 90));

            var w = size - 1f;
            var topY = Math.Max(1f, size * 0.12f);
            var midY = size * 0.5f;
            var stemTopX = size * 0.38f;
            var stemWidth = Math.Max(2f, size * 0.16f);
            var stemHeight = Math.Max(3f, size * 0.33f);

            var top = new[]
            {
                new PointF(1.5f, topY),
                new PointF(w - 1.5f, topY),
                new PointF(size * 0.62f, midY),
                new PointF(size * 0.38f, midY)
            };
            g.FillPolygon(brush, top);
            g.FillRectangle(brush, stemTopX, midY, stemWidth, stemHeight);

            return bitmap;
        }

        private static Bitmap CreateChevronIcon(bool next, int size)
        {
            var bitmap = new Bitmap(size, size);
            using var g = Graphics.FromImage(bitmap);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using var pen = new Pen(Color.FromArgb(70, 70, 70), Math.Max(2f, size / 5f));

            var inset = Math.Max(2f, size * 0.22f);
            var midY = size / 2f;
            if (next)
            {
                g.DrawLines(pen,
                [
                    new PointF(inset, inset),
                    new PointF(size - inset, midY),
                    new PointF(inset, size - inset)
                ]);
            }
            else
            {
                g.DrawLines(pen,
                [
                    new PointF(size - inset, inset),
                    new PointF(inset, midY),
                    new PointF(size - inset, size - inset)
                ]);
            }

            return bitmap;
        }

        private void InitializeGrid()
        {
            entriesGrid.AutoGenerateColumns = false;
            entriesGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            entriesGrid.ScrollBars = ScrollBars.Both;
            entriesGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            entriesGrid.Columns.Clear();
            _dynamicColumns.Clear();
            entriesGrid.CellFormatting += EntriesGrid_CellFormatting;
            entriesGrid.ColumnHeaderMouseClick += EntriesGrid_ColumnHeaderMouseClick;
            entriesGrid.KeyDown += EntriesGrid_KeyDown;

            entriesGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = nameof(HarTraceEntry.StartedDateTime),
                DataPropertyName = nameof(HarTraceEntry.StartedDateTime),
                HeaderText = "Started",
                Width = 210,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "yyyy-MM-dd HH:mm:ss.fff" }
            });

            entriesGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = nameof(HarTraceEntry.Method),
                DataPropertyName = nameof(HarTraceEntry.Method),
                HeaderText = "Method",
                Width = 90
            });

            entriesGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = nameof(HarTraceEntry.Host),
                DataPropertyName = nameof(HarTraceEntry.Host),
                HeaderText = "Host",
                Width = 260
            });

            entriesGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = nameof(HarTraceEntry.Url),
                DataPropertyName = nameof(HarTraceEntry.Url),
                HeaderText = "URL",
                Width = 900
            });

            entriesGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = nameof(HarTraceEntry.Status),
                DataPropertyName = nameof(HarTraceEntry.Status),
                HeaderText = "Status",
                Width = 90
            });

            entriesGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = nameof(HarTraceEntry.MimeType),
                DataPropertyName = nameof(HarTraceEntry.MimeType),
                HeaderText = "MIME",
                Width = 220
            });

            entriesGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = nameof(HarTraceEntry.DurationMs),
                DataPropertyName = nameof(HarTraceEntry.DurationMs),
                HeaderText = "Time (ms)",
                Width = 120,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "N1" }
            });

            entriesGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = nameof(HarTraceEntry.Tags),
                DataPropertyName = nameof(HarTraceEntry.Tags),
                HeaderText = "Tags",
                Width = 180
            });

            RebuildColumnMenuEntryTable();
        }

        private async void openButton_Click(object sender, EventArgs e)
        {
            using var dialog = new OpenFileDialog
            {
                Filter = "HAR files (*.har)|*.har|JSON files (*.json)|*.json|All files (*.*)|*.*",
                Title = "Open HAR Trace",
                InitialDirectory = Directory.Exists(_settings.LastOpenedDirectory) ? _settings.LastOpenedDirectory : string.Empty
            };

            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            await LoadHarFileAsync(dialog.FileName);
        }

        private async Task LoadHarFileAsync(string fileName)
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                requestTextBox.Clear();
                responseTextBox.Clear();
                requestJwtTextBox.Clear();
                responseJwtTextBox.Clear();

                _allEntries = await HarTraceLoader.LoadAsync(fileName);
                _currentHarFilePath = fileName;
                RebuildDynamicColumns();
                ApplyFilters();

                _settings.LastOpenedFile = fileName;
                _settings.LastOpenedDirectory = Path.GetDirectoryName(fileName) ?? string.Empty;
                SaveSettings();

                Text = $"HAR File Investigator - {Path.GetFileName(fileName)} ({_allEntries.Count} entries)";
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Failed to load HAR file:\n{ex.Message}", "Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void saveHarFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_currentHarFilePath) || !File.Exists(_currentHarFilePath))
            {
                MessageBox.Show(this, "No HAR file is loaded.", "Save HAR", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var dialog = new SaveFileDialog
            {
                Filter = "HAR files (*.har)|*.har|JSON files (*.json)|*.json|All files (*.*)|*.*",
                Title = "Save HAR File",
                FileName = Path.GetFileName(_currentHarFilePath),
                InitialDirectory = Path.GetDirectoryName(_currentHarFilePath) ?? string.Empty
            };

            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            try
            {
                var json = File.ReadAllText(_currentHarFilePath);
                var root = JsonNode.Parse(json)?.AsObject();
                var entries = root?["log"]?["entries"]?.AsArray();
                if (entries is null)
                {
                    throw new InvalidDataException("HAR log.entries array not found.");
                }

                var map = _allEntries.ToDictionary(x => x.EntryIndex, x => x.Tags);
                for (var i = 0; i < entries.Count; i++)
                {
                    if (entries[i] is not JsonObject entryObj)
                    {
                        continue;
                    }

                    if (!map.TryGetValue(i, out var tags) || string.IsNullOrWhiteSpace(tags))
                    {
                        entryObj.Remove("tag");
                    }
                    else
                    {
                        entryObj["tag"] = tags;
                    }
                }

                var output = root!.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(dialog.FileName, output, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Save failed:\n{ex.Message}", "Save HAR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void exportButton_Click(object sender, EventArgs e)
        {
            if (_filteredEntries.Count == 0)
            {
                MessageBox.Show(this, "No entries to export.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var dialog = new SaveFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                Title = "Export Filtered Rows to CSV",
                FileName = "har-filtered.csv",
                InitialDirectory = Directory.Exists(_settings.LastOpenedDirectory) ? _settings.LastOpenedDirectory : string.Empty
            };

            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            try
            {
                var csv = BuildCsv(_filteredEntries);
                File.WriteAllText(dialog.FileName, csv, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
                MessageBox.Show(this, $"Exported {_filteredEntries.Count} row(s).", "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Export failed:\n{ex.Message}", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static string BuildCsv(IEnumerable<HarTraceEntry> entries)
        {
            var sb = new StringBuilder();
            sb.AppendLine("StartedDateTime,Method,Host,Url,Status,StatusText,MimeType,DurationMs,IpAddress");

            foreach (var entry in entries)
            {
                sb.Append(Csv(entry.StartedDateTime.ToString("O", CultureInfo.InvariantCulture))).Append(',');
                sb.Append(Csv(entry.Method)).Append(',');
                sb.Append(Csv(entry.Host)).Append(',');
                sb.Append(Csv(entry.Url)).Append(',');
                sb.Append(Csv(entry.Status.ToString(CultureInfo.InvariantCulture))).Append(',');
                sb.Append(Csv(entry.StatusText)).Append(',');
                sb.Append(Csv(entry.MimeType)).Append(',');
                sb.Append(Csv(entry.DurationMs.ToString("N1", CultureInfo.InvariantCulture))).Append(',');
                sb.Append(Csv(entry.IpAddress)).AppendLine();
            }

            return sb.ToString();
        }

        private static string Csv(string value)
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        private void applyQueryButton_Click(object sender, EventArgs e)
        {
            ApplyFilters();
            AddQueryToHistory(GetQueryText());
            SaveSettingsFromUi();
        }

        private void filterToggleButton_Click(object sender, EventArgs e)
        {
            _isFilterEnabled = !_isFilterEnabled;
            UpdateFilterToggleButtonText();
            ApplyFilters();
            SaveSettingsFromUi();
        }

        private void tagButton_Click(object sender, EventArgs e)
        {
            if (_matchRowIndices.Count == 0)
            {
                MessageBox.Show(this, "No highlighted items to tag.", "Tag", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var inputForm = new Form
            {
                Text = "Add Tag",
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                ClientSize = new Size(420, 110)
            };

            var textBox = new TextBox { Left = 12, Top = 12, Width = 396 };
            var okButton = new Button { Text = "OK", Left = 252, Width = 75, Top = 50, DialogResult = DialogResult.OK };
            var cancelButton = new Button { Text = "Cancel", Left = 333, Width = 75, Top = 50, DialogResult = DialogResult.Cancel };
            inputForm.Controls.Add(textBox);
            inputForm.Controls.Add(okButton);
            inputForm.Controls.Add(cancelButton);
            inputForm.AcceptButton = okButton;
            inputForm.CancelButton = cancelButton;

            if (inputForm.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            var tag = textBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(tag))
            {
                return;
            }

            var entries = _matchRowIndices
                .Where(i => i >= 0 && i < entriesGrid.Rows.Count)
                .Select(i => entriesGrid.Rows[i].DataBoundItem as HarTraceEntry)
                .Where(x => x is not null)
                .Cast<HarTraceEntry>()
                .ToList();

            ApplyTagToEntries(entries, tag);
            SaveSettingsFromUi();
        }

        private void tagMenuButton_Click(object sender, EventArgs e)
        {
            OpenTagManager();
        }

        private void clearSessionsButton_Click(object sender, EventArgs e)
        {
            if (_allEntries.Count == 0)
            {
                return;
            }

            var result = MessageBox.Show(
                this,
                "Clear all loaded sessions from the current view?",
                "Clear Sessions",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
            {
                return;
            }

            _allEntries.Clear();
            _currentHarFilePath = null;
            RebuildDynamicColumns();
            ApplyFilters();

            requestTextBox.Clear();
            responseTextBox.Clear();
            requestJwtTextBox.Clear();
            responseJwtTextBox.Clear();

            Text = "HAR File Investigator";
            SaveSettingsFromUi();
        }

        private void OpenTagManager()
        {
            using var dialog = new TagManagerDialog(_tagDefinitions);
            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            _tagDefinitions = dialog.Tags
                .Where(x => !string.IsNullOrWhiteSpace(x.Name))
                .GroupBy(x => x.Name.Trim(), StringComparer.OrdinalIgnoreCase)
                .Select(x => x.First())
                .ToList();

            entriesGrid.Refresh();
            SaveSettingsFromUi();
        }

        private void ApplyTagToEntries(IEnumerable<HarTraceEntry> entries, string tag)
        {
            EnsureTagDefinition(tag);

            foreach (var entry in entries)
            {
                var tags = entry.Tags
                    .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                    .ToList();

                if (tags.All(x => !string.Equals(x, tag, StringComparison.OrdinalIgnoreCase)))
                {
                    tags.Add(tag);
                    entry.Tags = string.Join(", ", tags);
                }
            }

            entriesGrid.Refresh();
        }

        private void clearQueryButton_Click(object sender, EventArgs e)
        {
            queryComboBox.Text = string.Empty;
            ApplyFilters();
            SaveSettingsFromUi();
        }

        private void previousMatchButton_Click(object sender, EventArgs e)
        {
            MoveToMatch(-1);
        }

        private void nextMatchButton_Click(object sender, EventArgs e)
        {
            MoveToMatch(1);
        }

        private void queryHistoryComboBox_SelectionChangeCommitted(object sender, EventArgs e)
        {
            if (queryComboBox.SelectedItem is string selectedQuery)
            {
                queryComboBox.Text = selectedQuery;
            }
        }

        private void timelineButton_Click(object sender, EventArgs e)
        {
            if (_timelineDialog is null || _timelineDialog.IsDisposed)
            {
                _timelineDialog = new TimelineDialog();
                _timelineDialog.ApplyTheme(darkThemeCheckBox.Checked);
                _timelineDialog.DotClicked += TimelineDialog_DotClicked;
                _timelineDialog.FormClosed += (_, _) => _timelineDialog = null;
            }

            if (!_timelineDialog.Visible)
            {
                _timelineDialog.Show(this);
            }
            else
            {
                _timelineDialog.BringToFront();
            }

            UpdateTimelineFromHighlights();
        }

        private void TimelineDialog_DotClicked(object? sender, int rowIndex)
        {
            if (rowIndex < 0 || rowIndex >= entriesGrid.Rows.Count)
            {
                return;
            }

            entriesGrid.ClearSelection();
            entriesGrid.CurrentCell = entriesGrid.Rows[rowIndex].Cells[0];
            entriesGrid.Rows[rowIndex].Selected = true;

            var firstDisplayed = entriesGrid.FirstDisplayedScrollingRowIndex;
            if (firstDisplayed < 0)
            {
                return;
            }

            var displayedCount = entriesGrid.DisplayedRowCount(false);
            var lastDisplayed = firstDisplayed + displayedCount - 1;

            if (rowIndex < firstDisplayed || rowIndex > lastDisplayed)
            {
                entriesGrid.FirstDisplayedScrollingRowIndex = rowIndex;
            }
        }

        private void darkThemeCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            ApplyTheme(darkThemeCheckBox.Checked);
            SaveSettingsFromUi();
        }

        private void ConfigureColumnMenu()
        {
            _columnMenu.ShowImageMargin = false;
            _columnMenu.MaximumSize = new Size(460, 560);
            _columnMenu.ItemClicked += ColumnMenu_ItemClicked;
            _columnMenuSearchBox.AutoSize = false;
            _columnMenuSearchBox.Width = 240;
            _columnMenuSearchBox.ToolTipText = "Search columns";
            _columnMenuSearchBox.TextChanged += (_, _) =>
            {
                if (_suppressColumnMenuSearchChange)
                {
                    return;
                }

                var text = _columnMenuSearchBox.Text?.Trim() ?? string.Empty;
                if (text.Length is > 0 and < 3)
                {
                    if (_lastAppliedColumnMenuSearch.Length >= 3)
                    {
                        _lastAppliedColumnMenuSearch = string.Empty;
                        BuildColumnMenuItems(string.Empty);
                    }
                    return;
                }

                if (string.Equals(_lastAppliedColumnMenuSearch, text, StringComparison.Ordinal))
                {
                    return;
                }

                _lastAppliedColumnMenuSearch = text;
                BuildColumnMenuItems(text);
            };
        }

        private void ConfigureRowContextMenu()
        {
            _rowContextMenu.Opening += RowContextMenu_Opening;
            entriesGrid.ContextMenuStrip = _rowContextMenu;
        }

        private void ConfigurePaneSearchUi()
        {
            AttachPaneSearchBar(requestGroupBox, requestTabControl, true);
            AttachPaneSearchBar(responseGroupBox, responseTabControl, false);
        }

        private void AttachPaneSearchBar(GroupBox host, TabControl tabControl, bool isRequest)
        {
            if (host.Tag is TableLayoutPanel)
            {
                return;
            }

            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                Height = 36,
                Padding = new Padding(2, 2, 2, 2)
            };

            var searchBox = new TextBox
            {
                Left = 4,
                Top = 2,
                Width = Math.Max(140, host.Width - 130),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            if (isRequest)
            {
                _requestPaneSearchTextBox = searchBox;
            }
            else
            {
                _responsePaneSearchTextBox = searchBox;
            }

            var prevButton = new Button
            {
                Width = searchBox.PreferredHeight,
                Height = searchBox.PreferredHeight,
                Top = 2,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            prevButton.Image = CreateChevronIcon(false, Math.Max(12, searchBox.PreferredHeight - 8));
            prevButton.Text = string.Empty;

            var nextButton = new Button
            {
                Width = searchBox.PreferredHeight,
                Height = searchBox.PreferredHeight,
                Top = 2,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            nextButton.Image = CreateChevronIcon(true, Math.Max(12, searchBox.PreferredHeight - 8));
            nextButton.Text = string.Empty;

            var copyButton = new Button
            {
                Text = "Copy",
                Width = 52,
                Height = 24,
                Top = 2,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            prevButton.Left = Math.Max(0, host.Width - (copyButton.Width + nextButton.Width + prevButton.Width + 10));
            nextButton.Left = prevButton.Right + 2;
            copyButton.Left = Math.Max(0, host.Width - 56);

            host.Resize += (_, _) =>
            {
                searchBox.Width = Math.Max(140, host.Width - 130);
                prevButton.Left = Math.Max(0, host.Width - (copyButton.Width + nextButton.Width + prevButton.Width + 10));
                nextButton.Left = prevButton.Right + 2;
                copyButton.Left = Math.Max(0, host.Width - 56);
            };

            prevButton.Click += (_, _) => SearchPaneText(isRequest, searchBox.Text, forward: false);
            nextButton.Click += (_, _) => SearchPaneText(isRequest, searchBox.Text, forward: true);
            searchBox.KeyDown += (_, e) =>
            {
                if (e.KeyCode != Keys.Enter)
                {
                    return;
                }

                SearchPaneText(isRequest, searchBox.Text, forward: true);
                e.SuppressKeyPress = true;
            };
            copyButton.Click += (_, _) => CopyPaneText(isRequest);

            tabControl.Dock = DockStyle.Fill;

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2
            };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            tabControl.Parent?.Controls.Remove(tabControl);
            layout.Controls.Add(panel, 0, 0);
            layout.Controls.Add(tabControl, 0, 1);
            host.Controls.Add(layout);
            host.Tag = layout;

            panel.Controls.Add(searchBox);
            panel.Controls.Add(prevButton);
            panel.Controls.Add(nextButton);
            panel.Controls.Add(copyButton);
        }

        private void CopyPaneText(bool isRequest)
        {
            var box = isRequest ? GetActiveRequestSearchBox() : GetActiveResponseSearchBox();
            if (box is null || string.IsNullOrEmpty(box.Text))
            {
                return;
            }

            Clipboard.SetText(box.Text);
        }

        private void SearchPaneText(bool isRequest, string term, bool forward)
        {
            if (string.IsNullOrWhiteSpace(term))
            {
                return;
            }

            var box = isRequest ? GetActiveRequestSearchBox() : GetActiveResponseSearchBox();
            if (box is null || string.IsNullOrEmpty(box.Text))
            {
                return;
            }

            var currentPosition = isRequest ? _requestSearchPosition : _responseSearchPosition;
            var comparison = StringComparison.OrdinalIgnoreCase;
            var index = forward
                ? box.Text.IndexOf(term, Math.Max(0, currentPosition), comparison)
                : box.Text.LastIndexOf(term, Math.Max(0, Math.Min(currentPosition, box.Text.Length - 1)), comparison);

            if (index < 0)
            {
                currentPosition = forward ? 0 : box.Text.Length - 1;
                index = forward
                    ? box.Text.IndexOf(term, currentPosition, comparison)
                    : box.Text.LastIndexOf(term, currentPosition, comparison);
            }

            if (index < 0)
            {
                return;
            }

            box.Focus();
            box.SelectionStart = index;
            box.SelectionLength = term.Length;
            box.ScrollToCaret();

            if (isRequest)
            {
                _requestSearchPosition = forward ? index + term.Length : Math.Max(0, index - 1);
                _activeRequestSearchBox = box;
            }
            else
            {
                _responseSearchPosition = forward ? index + term.Length : Math.Max(0, index - 1);
                _activeResponseSearchBox = box;
            }
        }

        private RichTextBox? GetActiveRequestSearchBox()
        {
            return requestTabControl.SelectedTab == requestJwtTabPage ? requestJwtTextBox : requestTextBox;
        }

        private RichTextBox? GetActiveResponseSearchBox()
        {
            return responseTabControl.SelectedTab == responseJwtTabPage ? responseJwtTextBox : responseTextBox;
        }

        private void RowContextMenu_Opening(object? sender, CancelEventArgs e)
        {
            var clientPoint = entriesGrid.PointToClient(Cursor.Position);
            var hit = entriesGrid.HitTest(clientPoint.X, clientPoint.Y);
            if (hit.Type is not DataGridViewHitTestType.Cell and not DataGridViewHitTestType.RowHeader)
            {
                e.Cancel = true;
                return;
            }

            if (hit.RowIndex >= 0 && hit.RowIndex < entriesGrid.Rows.Count && !entriesGrid.Rows[hit.RowIndex].Selected)
            {
                entriesGrid.ClearSelection();
                entriesGrid.Rows[hit.RowIndex].Selected = true;
                if (entriesGrid.Rows[hit.RowIndex].Cells.Count > 0)
                {
                    entriesGrid.CurrentCell = entriesGrid.Rows[hit.RowIndex].Cells[0];
                }
            }

            _rowContextMenu.Items.Clear();

            _rowTagMenuItem.DropDownItems.Clear();
            foreach (var tag in _tagDefinitions.OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase))
            {
                var tagItem = new ToolStripMenuItem(tag.Name);
                tagItem.Click += (_, _) => ApplyTagToSelectedRows(tag.Name);
                _rowTagMenuItem.DropDownItems.Add(tagItem);
            }

            if (_rowTagMenuItem.DropDownItems.Count > 0)
            {
                _rowTagMenuItem.DropDownItems.Add(new ToolStripSeparator());
            }

            var manageItem = new ToolStripMenuItem("Tag manager...");
            manageItem.Click += (_, _) => OpenTagManager();
            _rowTagMenuItem.DropDownItems.Add(manageItem);

            var compareItem = new ToolStripMenuItem("Compare");
            compareItem.Click += (_, _) => CompareSelectedRows();

            var propertiesItem = new ToolStripMenuItem("Properties");
            propertiesItem.Click += (_, _) => ShowSelectedRowProperties();

            _rowDeleteMenuItem.Enabled = entriesGrid.SelectedRows.Count > 0;
            _rowDeleteMenuItem.Click -= RowDeleteMenuItem_Click;
            _rowDeleteMenuItem.Click += RowDeleteMenuItem_Click;

            _rowContextMenu.Items.Add(_rowTagMenuItem);
            _rowContextMenu.Items.Add(compareItem);
            _rowContextMenu.Items.Add(propertiesItem);
            _rowContextMenu.Items.Add(new ToolStripSeparator());
            _rowContextMenu.Items.Add(_rowDeleteMenuItem);
        }

        private void RowDeleteMenuItem_Click(object? sender, EventArgs e)
        {
            DeleteSelectedRowsFromView();
        }

        private void EntriesGrid_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Delete)
            {
                return;
            }

            DeleteSelectedRowsFromView();
            e.Handled = true;
            e.SuppressKeyPress = true;
        }

        private void DeleteSelectedRowsFromView()
        {
            var selectedEntries = entriesGrid.SelectedRows
                .Cast<DataGridViewRow>()
                .Select(x => x.DataBoundItem as HarTraceEntry)
                .Where(x => x is not null)
                .Cast<HarTraceEntry>()
                .Distinct()
                .ToList();

            if (selectedEntries.Count == 0)
            {
                return;
            }

            foreach (var entry in selectedEntries)
            {
                _allEntries.Remove(entry);
            }

            ApplyFilters();

            if (string.IsNullOrWhiteSpace(_currentHarFilePath))
            {
                Text = $"HAR File Investigator ({_allEntries.Count} entries)";
            }
            else
            {
                Text = $"HAR File Investigator - {Path.GetFileName(_currentHarFilePath)} ({_allEntries.Count} entries)";
            }
        }

        private void ApplyTagToSelectedRows(string tag)
        {
            var selectedEntries = entriesGrid.SelectedRows
                .Cast<DataGridViewRow>()
                .Select(x => x.DataBoundItem as HarTraceEntry)
                .Where(x => x is not null)
                .Cast<HarTraceEntry>()
                .ToList();

            ApplyTagToEntries(selectedEntries, tag);
            SaveSettingsFromUi();
        }

        private void CompareSelectedRows()
        {
            var entries = entriesGrid.SelectedRows
                .Cast<DataGridViewRow>()
                .Select(x => x.DataBoundItem as HarTraceEntry)
                .Where(x => x is not null)
                .Cast<HarTraceEntry>()
                .Take(2)
                .ToList();

            if (entries.Count < 2)
            {
                MessageBox.Show(this, "Select at least two rows to compare.", "Compare", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var left = entries[0];
            var right = entries[1];
            var sb = new StringBuilder();
            sb.AppendLine($"Method: {left.Method} | {right.Method}");
            sb.AppendLine($"Status: {left.Status} | {right.Status}");
            sb.AppendLine($"Host  : {left.Host} | {right.Host}");
            sb.AppendLine($"URL   : {left.Url} | {right.Url}");
            sb.AppendLine($"Time  : {left.DurationMs:N1} | {right.DurationMs:N1}");
            sb.AppendLine($"Tags  : {left.Tags} | {right.Tags}");

            MessageBox.Show(this, sb.ToString(), "Compare", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ShowSelectedRowProperties()
        {
            if (entriesGrid.CurrentRow?.DataBoundItem is not HarTraceEntry entry)
            {
                return;
            }

            var sb = new StringBuilder();
            sb.AppendLine($"Started: {entry.StartedDateTime:O}");
            sb.AppendLine($"Method: {entry.Method}");
            sb.AppendLine($"URL: {entry.Url}");
            sb.AppendLine($"Host: {entry.Host}");
            sb.AppendLine($"Status: {entry.Status} {entry.StatusText}");
            sb.AppendLine($"MIME: {entry.MimeType}");
            sb.AppendLine($"Duration: {entry.DurationMs:N1}");
            sb.AppendLine($"IP: {entry.IpAddress}");
            sb.AppendLine($"Tags: {entry.Tags}");

            foreach (var item in entry.AdditionalFields.OrderBy(x => x.Key, StringComparer.OrdinalIgnoreCase))
            {
                sb.AppendLine($"{item.Key}: {item.Value}");
            }

            MessageBox.Show(this, sb.ToString(), "Properties", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void EntriesGrid_ColumnHeaderMouseClick(object? sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
            {
                return;
            }

            _suppressColumnMenuSearchChange = true;
            _columnMenuSearchBox.Text = string.Empty;
            _suppressColumnMenuSearchChange = false;
            _lastAppliedColumnMenuSearch = string.Empty;
            BuildColumnMenuItems(string.Empty);
            _columnMenu.Show(Cursor.Position);
            BeginInvoke(() => _columnMenuSearchBox.Focus());
        }

        private void BuildColumnMenuItems(string? searchText)
        {
            _columnMenu.SuspendLayout();
            _columnMenu.Items.Clear();

            _columnMenu.Items.Add(_columnMenuSearchBox);
            _columnMenu.Items.Add(new ToolStripSeparator());

            var filter = searchText?.Trim() ?? string.Empty;
            var applyFilter = filter.Length >= 3;

            var filteredEntries = !applyFilter
                ? _columnMenuEntries
                : _columnMenuEntries
                    .Where(x => x.Header.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                                x.Name.Contains(filter, StringComparison.OrdinalIgnoreCase))
                    .ToList();

            foreach (var entry in filteredEntries)
            {
                if (!_columnLookupCache.TryGetValue(entry.Name, out var column))
                {
                    continue;
                }

                var item = new ToolStripMenuItem(entry.Header)
                {
                    Name = entry.Name,
                    Checked = column.Visible,
                    CheckOnClick = false
                };
                _columnMenu.Items.Add(item);
            }
            _columnMenu.ResumeLayout();
        }

        private void ColumnMenu_ItemClicked(object? sender, ToolStripItemClickedEventArgs e)
        {
            if (e.ClickedItem is not ToolStripMenuItem menuItem)
            {
                return;
            }

            var column = entriesGrid.Columns
                .Cast<DataGridViewColumn>()
                .FirstOrDefault(x => x.Name == menuItem.Name);
            if (column is null)
            {
                return;
            }

            var visibleCount = entriesGrid.Columns.Cast<DataGridViewColumn>().Count(x => x.Visible);
            if (column.Visible && visibleCount <= 1)
            {
                return;
            }

            column.Visible = !column.Visible;
            BuildColumnMenuItems(_columnMenuSearchBox.Text);
        }

        private void RebuildDynamicColumns()
        {
            foreach (var dynamicColumn in _dynamicColumns.Values)
            {
                if (entriesGrid.Columns.Contains(dynamicColumn))
                {
                    entriesGrid.Columns.Remove(dynamicColumn);
                }
            }

            _dynamicColumns.Clear();

            var fieldKeys = _allEntries
                .SelectMany(x => x.AdditionalFields.Keys)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
                .ToList();

            foreach (var key in fieldKeys)
            {
                var column = new DataGridViewTextBoxColumn
                {
                    Name = $"Dyn::{key}",
                    HeaderText = key,
                    Width = 240,
                    Visible = false,
                    Tag = key
                };

                _dynamicColumns[key] = column;
                entriesGrid.Columns.Add(column);
            }

            RebuildColumnMenuEntryTable();
        }

        private void RebuildColumnMenuEntryTable()
        {
            _columnMenuEntries.Clear();
            _columnLookupCache.Clear();
            foreach (DataGridViewColumn column in entriesGrid.Columns)
            {
                _columnMenuEntries.Add((column.Name, column.HeaderText));
                _columnLookupCache[column.Name] = column;
            }
        }

        private void EntriesGrid_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
            {
                return;
            }

            var column = entriesGrid.Columns[e.ColumnIndex];
            if (entriesGrid.Rows[e.RowIndex].DataBoundItem is not HarTraceEntry entry)
            {
                return;
            }

            if (string.Equals(column.Name, nameof(HarTraceEntry.Tags), StringComparison.Ordinal))
            {
                ApplyTagCellColor(entry, e);
                return;
            }

            if (column.Tag is not string dynamicKey)
            {
                return;
            }

            e.Value = entry.GetAdditionalField(dynamicKey);
            e.FormattingApplied = true;
        }

        private void ApplyTagCellColor(HarTraceEntry entry, DataGridViewCellFormattingEventArgs e)
        {
            var firstTag = entry.Tags
                .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                .FirstOrDefault();

            if (string.IsNullOrWhiteSpace(firstTag))
            {
                return;
            }

            var tag = _tagDefinitions.FirstOrDefault(x => string.Equals(x.Name, firstTag, StringComparison.OrdinalIgnoreCase));
            if (tag is null)
            {
                return;
            }

            var color = tag.Color;
            e.CellStyle.BackColor = color;
            e.CellStyle.ForeColor = GetContrastTextColor(color);
            e.CellStyle.SelectionBackColor = ControlPaint.Dark(color);
            e.CellStyle.SelectionForeColor = GetContrastTextColor(e.CellStyle.SelectionBackColor);
        }

        private void EnsureTagDefinition(string tag)
        {
            if (_tagDefinitions.Any(x => string.Equals(x.Name, tag, StringComparison.OrdinalIgnoreCase)))
            {
                return;
            }

            _tagDefinitions.Add(new TagDefinition
            {
                Name = tag,
                Color = GetDefaultTagColor(tag)
            });
        }

        private static Color GetDefaultTagColor(string key)
        {
            var hash = key.ToLowerInvariant().GetHashCode();
            var hue = Math.Abs(hash % 360);
            return ColorFromHsv(hue, 0.40, 0.95);
        }

        private static Color ColorFromHsv(double hue, double saturation, double value)
        {
            var hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            var f = hue / 60 - Math.Floor(hue / 60);

            value *= 255;
            var v = Convert.ToInt32(value);
            var p = Convert.ToInt32(value * (1 - saturation));
            var q = Convert.ToInt32(value * (1 - f * saturation));
            var t = Convert.ToInt32(value * (1 - (1 - f) * saturation));

            return hi switch
            {
                0 => Color.FromArgb(255, v, t, p),
                1 => Color.FromArgb(255, q, v, p),
                2 => Color.FromArgb(255, p, v, t),
                3 => Color.FromArgb(255, p, q, v),
                4 => Color.FromArgb(255, t, p, v),
                _ => Color.FromArgb(255, v, p, q)
            };
        }

        private static Color GetContrastTextColor(Color color)
        {
            var luminance = (0.299 * color.R) + (0.587 * color.G) + (0.114 * color.B);
            return luminance > 140 ? Color.Black : Color.White;
        }

        private void ApplyFilters()
        {
            var queryText = GetQueryText();

            IEnumerable<HarTraceEntry> gridEntries = _allEntries;
            if (_isFilterEnabled && !string.IsNullOrWhiteSpace(queryText))
            {
                gridEntries = _allEntries.Where(x => MatchesQuery(x, queryText));
            }

            _filteredEntries.Clear();
            foreach (var entry in gridEntries)
            {
                _filteredEntries.Add(entry);
            }

            var matchedEntries = string.IsNullOrWhiteSpace(queryText)
                ? []
                : _filteredEntries.Where(x => MatchesQuery(x, queryText)).ToHashSet();

            UpdateMatchHighlights(matchedEntries);
            ApplyQueryHighlightsToPanes(queryText);
            UpdateStatusBarCounts();
        }

        private void ApplyQueryHighlightsToPanes(string queryText)
        {
            HighlightInPane(requestTextBox, queryText);
            HighlightInPane(responseTextBox, queryText);
            HighlightInPane(requestJwtTextBox, queryText);
            HighlightInPane(responseJwtTextBox, queryText);
        }

        private void HighlightInPane(RichTextBox box, string query)
        {
            var originalStart = box.SelectionStart;
            var originalLength = box.SelectionLength;

            box.SelectAll();
            box.SelectionBackColor = box.BackColor;
            box.SelectionColor = box.ForeColor;

            if (!string.IsNullOrWhiteSpace(query))
            {
                var index = 0;
                while ((index = box.Text.IndexOf(query, index, StringComparison.OrdinalIgnoreCase)) >= 0)
                {
                    box.Select(index, query.Length);
                    box.SelectionBackColor = Color.Yellow;
                    box.SelectionColor = Color.Black;
                    index += query.Length;
                }
            }

            box.Select(originalStart, originalLength);
        }

        private void UpdateFilterToggleButtonText()
        {
            var status = _isFilterEnabled ? "On" : "Off";
            filterToggleButton.Text = $"Filter ({status})";
            filterToggleButton.FlatStyle = _isFilterEnabled ? FlatStyle.Popup : FlatStyle.Standard;
            filterToggleButton.Font = new Font(filterToggleButton.Font, _isFilterEnabled ? FontStyle.Bold : FontStyle.Regular);
        }

        private void UpdateMatchHighlights(ISet<HarTraceEntry> matchedEntries)
        {
            _matchRowIndices.Clear();
            var matchBackColor = GetMatchHighlightColor();

            for (var i = 0; i < entriesGrid.Rows.Count; i++)
            {
                var row = entriesGrid.Rows[i];
                if (row.DataBoundItem is HarTraceEntry entry && matchedEntries.Contains(entry))
                {
                    row.DefaultCellStyle.BackColor = matchBackColor;
                    _matchRowIndices.Add(i);
                }
                else
                {
                    row.DefaultCellStyle.BackColor = Color.Empty;
                }
            }

            _currentMatchIndex = _matchRowIndices.Count > 0 ? 0 : -1;
            UpdateMatchNavigationButtons();

            if (_currentMatchIndex >= 0)
            {
                SelectCurrentMatch();
            }

            UpdateTimelineFromHighlights();
            ApplyQueryHighlightsToPanes(GetQueryText());
            AutoMarkQueryInPanesForHighlightedRow();
            UpdateStatusBarCounts();
        }

        private void AutoMarkQueryInPanesForHighlightedRow()
        {
            var query = GetQueryText();
            if (string.IsNullOrWhiteSpace(query))
            {
                return;
            }

            var currentRowIndex = entriesGrid.CurrentRow?.Index ?? -1;
            if (currentRowIndex < 0 || !_matchRowIndices.Contains(currentRowIndex))
            {
                return;
            }

            if (_requestPaneSearchTextBox is { Text.Length: 0 })
            {
                MarkFirstQueryOccurrence(GetActiveRequestSearchBox(), query);
            }

            if (_responsePaneSearchTextBox is { Text.Length: 0 })
            {
                MarkFirstQueryOccurrence(GetActiveResponseSearchBox(), query);
            }
        }

        private static void MarkFirstQueryOccurrence(RichTextBox? box, string query)
        {
            if (box is null || string.IsNullOrEmpty(box.Text))
            {
                return;
            }

            var index = box.Text.IndexOf(query, StringComparison.OrdinalIgnoreCase);
            if (index < 0)
            {
                return;
            }

            box.SelectionStart = index;
            box.SelectionLength = query.Length;
            box.ScrollToCaret();
        }

        private Color GetMatchHighlightColor()
        {
            return darkThemeCheckBox.Checked
                ? Color.FromArgb(85, 85, 35)
                : Color.FromArgb(255, 247, 186);
        }

        private void MoveToMatch(int delta)
        {
            if (_matchRowIndices.Count == 0)
            {
                return;
            }

            _currentMatchIndex = (_currentMatchIndex + delta + _matchRowIndices.Count) % _matchRowIndices.Count;
            SelectCurrentMatch();
        }

        private void SelectCurrentMatch()
        {
            if (_currentMatchIndex < 0 || _currentMatchIndex >= _matchRowIndices.Count)
            {
                return;
            }

            var rowIndex = _matchRowIndices[_currentMatchIndex];
            if (rowIndex < 0 || rowIndex >= entriesGrid.Rows.Count)
            {
                return;
            }

            entriesGrid.ClearSelection();
            entriesGrid.CurrentCell = entriesGrid.Rows[rowIndex].Cells[0];
            entriesGrid.Rows[rowIndex].Selected = true;

            var firstDisplayed = entriesGrid.FirstDisplayedScrollingRowIndex;
            if (firstDisplayed < 0)
            {
                return;
            }

            var displayedCount = entriesGrid.DisplayedRowCount(false);
            var lastDisplayed = firstDisplayed + displayedCount - 1;

            if (rowIndex < firstDisplayed || rowIndex > lastDisplayed)
            {
                entriesGrid.FirstDisplayedScrollingRowIndex = rowIndex;
            }
        }

        private void UpdateMatchNavigationButtons()
        {
            var hasMatches = _matchRowIndices.Count > 0;
            previousMatchButton.Enabled = hasMatches;
            nextMatchButton.Enabled = hasMatches;
        }

        private static bool MatchesQuery(HarTraceEntry entry, string queryText)
        {
            var tokens = TokenizeQuery(queryText);
            if (tokens.Count == 0)
            {
                return true;
            }

            var groupSatisfied = true;
            var anyGroupMatched = false;

            foreach (var token in tokens)
            {
                if (string.Equals(token, "or", StringComparison.OrdinalIgnoreCase))
                {
                    anyGroupMatched |= groupSatisfied;
                    groupSatisfied = true;
                    continue;
                }

                if (string.Equals(token, "and", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                groupSatisfied &= EvaluateTerm(entry, token);
            }

            anyGroupMatched |= groupSatisfied;
            return anyGroupMatched;
        }

        private static List<string> TokenizeQuery(string query)
        {
            var matches = Regex.Matches(query, "\"[^\"]*\"|\\S+");
            return matches.Select(m => m.Value).ToList();
        }

        private static bool EvaluateTerm(HarTraceEntry entry, string token)
        {
            var isNegated = token.StartsWith('!');
            var rawToken = isNegated ? token[1..] : token;

            var result = EvaluateRawTerm(entry, rawToken);
            return isNegated ? !result : result;
        }

        private static bool EvaluateRawTerm(HarTraceEntry entry, string token)
        {
            var match = Regex.Match(token, "^(?<field>[a-zA-Z_][a-zA-Z0-9_.-]*)(?<op><=|>=|!=|=|:|<|>)(?<value>.+)$");
            if (!match.Success)
            {
                return entry.SearchText.Contains(Unquote(token), StringComparison.OrdinalIgnoreCase);
            }

            var field = match.Groups["field"].Value.ToLowerInvariant();
            var op = match.Groups["op"].Value;
            var value = Unquote(match.Groups["value"].Value);

            return field switch
            {
                "method" => CompareString(entry.Method, op, value),
                "host" => CompareString(entry.Host, op, value),
                "url" => CompareString(entry.Url, op, value),
                "mime" or "mimetype" => CompareString(entry.MimeType, op, value),
                "ip" => CompareString(entry.IpAddress, op, value),
                "tag" or "tags" => CompareString(entry.Tags, op, value),
                "status" => CompareInt(entry.Status, op, value),
                "time" or "duration" => CompareDouble(entry.DurationMs, op, value),
                "request" => CompareString(entry.RequestText, op, value),
                "response" => CompareString(entry.ResponseText, op, value),
                "text" => CompareString(entry.SearchText, op, value),
                _ => entry.AdditionalFields.TryGetValue(field, out var additionalValue)
                    ? CompareString(additionalValue, op, value)
                    : entry.SearchText.Contains(token, StringComparison.OrdinalIgnoreCase)
            };
        }

        private static string Unquote(string value)
        {
            if (value.Length >= 2 && value.StartsWith('"') && value.EndsWith('"'))
            {
                return value[1..^1];
            }

            return value;
        }

        private static bool CompareString(string source, string op, string value)
        {
            return op switch
            {
                "=" => string.Equals(source, value, StringComparison.OrdinalIgnoreCase),
                "!=" => !string.Equals(source, value, StringComparison.OrdinalIgnoreCase),
                ":" => source.Contains(value, StringComparison.OrdinalIgnoreCase),
                _ => source.Contains(value, StringComparison.OrdinalIgnoreCase)
            };
        }

        private static bool CompareInt(int source, string op, string value)
        {
            if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed))
            {
                return false;
            }

            return op switch
            {
                "=" => source == parsed,
                "!=" => source != parsed,
                ">" => source > parsed,
                ">=" => source >= parsed,
                "<" => source < parsed,
                "<=" => source <= parsed,
                _ => false
            };
        }

        private static bool CompareDouble(double source, string op, string value)
        {
            if (!double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed))
            {
                return false;
            }

            return op switch
            {
                "=" => Math.Abs(source - parsed) < 0.0001,
                "!=" => Math.Abs(source - parsed) >= 0.0001,
                ">" => source > parsed,
                ">=" => source >= parsed,
                "<" => source < parsed,
                "<=" => source <= parsed,
                _ => false
            };
        }

        private void entriesGrid_SelectionChanged(object sender, EventArgs e)
        {
            if (entriesGrid.CurrentRow?.DataBoundItem is HarTraceEntry selected)
            {
                requestTextBox.Text = selected.RequestText;
                requestTextBox.SelectionStart = 0;
                requestTextBox.SelectionLength = 0;

                responseTextBox.Text = selected.ResponseText;
                responseTextBox.SelectionStart = 0;
                responseTextBox.SelectionLength = 0;

                requestJwtTextBox.Text = BuildRequestJwtView(selected);
                requestJwtTextBox.SelectionStart = 0;
                requestJwtTextBox.SelectionLength = 0;

                responseJwtTextBox.Text = BuildResponseJwtView(selected);
                responseJwtTextBox.SelectionStart = 0;
                responseJwtTextBox.SelectionLength = 0;
            }
            else
            {
                requestTextBox.Clear();
                responseTextBox.Clear();
                requestJwtTextBox.Clear();
                responseJwtTextBox.Clear();
            }

            var queryText = GetQueryText();
            ApplyQueryHighlightsToPanes(queryText);
            AutoMarkQueryInPanesForHighlightedRow();

            UpdateTimelineFromHighlights();
            UpdateStatusBarCounts();
        }

        private void UpdateStatusBarCounts()
        {
            if (rowCountStatusLabel is null)
            {
                return;
            }

            var total = _filteredEntries.Count;
            var selected = entriesGrid?.SelectedRows?.Count ?? 0;
            var highlighted = _matchRowIndices.Count;

            rowCountStatusLabel.Text = $"Rows: {total} | Selected: {selected} | Highlighted: {highlighted}";
        }

        private void UpdateTimelineFromHighlights()
        {
            if (_timelineDialog is null || _timelineDialog.IsDisposed)
            {
                return;
            }

            var points = _matchRowIndices
                .Where(rowIndex => rowIndex >= 0 && rowIndex < entriesGrid.Rows.Count)
                .Select(rowIndex => new { rowIndex, row = entriesGrid.Rows[rowIndex] })
                .Select(x => new { x.rowIndex, entry = x.row.DataBoundItem as HarTraceEntry })
                .Where(x => x.entry is not null)
                .Select(x => new TimelineDialog.TimelinePoint(x.entry!.StartedDateTime, x.rowIndex, x.entry.Url))
                .ToList();

            var selectedRowIndices = entriesGrid.SelectedRows
                .Cast<DataGridViewRow>()
                .Select(x => x.Index)
                .ToList();

            _timelineDialog.UpdatePoints(points, GetMatchHighlightColor(), entriesGrid.SelectedRows.Count, selectedRowIndices);
        }

        private void ApplyTheme(bool dark)
        {
            if (_timelineDialog is not null && !_timelineDialog.IsDisposed)
            {
                _timelineDialog.ApplyTheme(dark);
            }

            if (!dark)
            {
                BackColor = SystemColors.Control;
                ForeColor = SystemColors.ControlText;
                ApplyControlTheme(this, SystemColors.Control, SystemColors.ControlText, SystemColors.Window, SystemColors.WindowText);
                entriesGrid.EnableHeadersVisualStyles = true;
                UpdateMatchHighlights(
                    string.IsNullOrWhiteSpace(GetQueryText())
                        ? []
                        : _allEntries.Where(x => MatchesQuery(x, GetQueryText())).ToHashSet());
                return;
            }

            var back = Color.FromArgb(30, 30, 30);
            var fore = Color.FromArgb(220, 220, 220);
            var surface = Color.FromArgb(45, 45, 45);

            BackColor = back;
            ForeColor = fore;
            ApplyControlTheme(this, back, fore, surface, fore);

            entriesGrid.BackgroundColor = surface;
            entriesGrid.GridColor = Color.FromArgb(70, 70, 70);
            entriesGrid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(55, 55, 55);
            entriesGrid.ColumnHeadersDefaultCellStyle.ForeColor = fore;
            entriesGrid.EnableHeadersVisualStyles = false;
            entriesGrid.DefaultCellStyle.BackColor = surface;
            entriesGrid.DefaultCellStyle.ForeColor = fore;
            entriesGrid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 122, 204);
            entriesGrid.DefaultCellStyle.SelectionForeColor = Color.White;

            UpdateMatchHighlights(
                string.IsNullOrWhiteSpace(GetQueryText())
                    ? []
                    : _allEntries.Where(x => MatchesQuery(x, GetQueryText())).ToHashSet());
        }

        private static void ApplyControlTheme(Control root, Color back, Color fore, Color inputBack, Color inputFore)
        {
            foreach (Control control in root.Controls)
            {
                switch (control)
                {
                    case TextBox:
                    case RichTextBox:
                    case ComboBox:
                        control.BackColor = inputBack;
                        control.ForeColor = inputFore;
                        break;
                    case DataGridView:
                        break;
                    default:
                        control.BackColor = back;
                        control.ForeColor = fore;
                        break;
                }

                ApplyControlTheme(control, back, fore, inputBack, inputFore);
            }
        }

        private static string BuildRequestJwtView(HarTraceEntry entry)
        {
            if (!TryExtractBearerToken(entry.RequestText, out var token))
            {
                return "No bearer token found in request Authorization header.";
            }

            return TryDecodeJwt(token, out var decoded)
                ? decoded
                : "Bearer token found, but it is not a valid JWT.";
        }

        private static string BuildResponseJwtView(HarTraceEntry entry)
        {
            if (!TryExtractAccessToken(entry.ResponseText, out var token))
            {
                return "No access token found in response body.";
            }

            return TryDecodeJwt(token, out var decoded)
                ? decoded
                : "Access token found, but it is not a valid JWT.";
        }

        private static bool TryExtractBearerToken(string requestText, out string token)
        {
            var match = Regex.Match(
                requestText,
                @"Authorization\s*:\s*Bearer\s+([A-Za-z0-9\-_]+\.[A-Za-z0-9\-_]+\.[A-Za-z0-9\-_]+)",
                RegexOptions.IgnoreCase);

            token = match.Success ? match.Groups[1].Value : string.Empty;
            return match.Success;
        }

        private static bool TryExtractAccessToken(string responseText, out string token)
        {
            var bodyMatch = Regex.Match(responseText, @"Response Body:\s*(?<body>[\s\S]*)$", RegexOptions.IgnoreCase);
            var body = bodyMatch.Success ? bodyMatch.Groups["body"].Value : responseText;

            var jsonMatch = Regex.Match(
                body,
                "\"(?:access_token|accessToken|accesstoken)\"\\s*:\\s*\"([^\"\\r\\n]+)\"",
                RegexOptions.IgnoreCase);

            if (jsonMatch.Success)
            {
                token = jsonMatch.Groups[1].Value;
                return true;
            }

            var formMatch = Regex.Match(body, @"(?:^|[\?&\s])access_token=([^&\s]+)", RegexOptions.IgnoreCase);
            token = formMatch.Success ? Uri.UnescapeDataString(formMatch.Groups[1].Value) : string.Empty;
            return formMatch.Success;
        }

        private static bool TryDecodeJwt(string token, out string decoded)
        {
            decoded = string.Empty;

            var parts = token.Split('.');
            if (parts.Length < 2)
            {
                return false;
            }

            if (!TryDecodeBase64Url(parts[0], out var headerJson) || !TryDecodeBase64Url(parts[1], out var payloadJson))
            {
                return false;
            }

            var formattedHeader = TryFormatJson(headerJson);
            var formattedPayload = TryFormatJson(payloadJson);

            var sb = new StringBuilder();
            sb.AppendLine("JWT");
            sb.AppendLine();
            sb.AppendLine("Header:");
            sb.AppendLine(formattedHeader);
            sb.AppendLine();
            sb.AppendLine("Payload:");
            sb.AppendLine(formattedPayload);

            decoded = sb.ToString();
            return true;
        }

        private static bool TryDecodeBase64Url(string input, out string decoded)
        {
            decoded = string.Empty;

            try
            {
                var base64 = input.Replace('-', '+').Replace('_', '/');
                var padding = 4 - (base64.Length % 4);
                if (padding is > 0 and < 4)
                {
                    base64 = base64.PadRight(base64.Length + padding, '=');
                }

                var bytes = Convert.FromBase64String(base64);
                decoded = Encoding.UTF8.GetString(bytes);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static string TryFormatJson(string raw)
        {
            try
            {
                using var doc = JsonDocument.Parse(raw);
                return JsonSerializer.Serialize(doc.RootElement, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
            }
            catch
            {
                return raw;
            }
        }

        private void LoadSettings()
        {
            _settings = AppUiSettings.Load(_settingsFilePath);

            queryComboBox.Text = _settings.SearchFilter;
            RefreshQueryHistoryDropdown();
            darkThemeCheckBox.Checked = _settings.DarkTheme;
            _isFilterEnabled = _settings.FilterEnabled;
            _tagDefinitions = (_settings.TagDefinitions ?? [])
                .Where(x => !string.IsNullOrWhiteSpace(x.Name))
                .GroupBy(x => x.Name.Trim(), StringComparer.OrdinalIgnoreCase)
                .Select(x => x.First())
                .ToList();
            UpdateFilterToggleButtonText();

            if (_settings.Width > 0 && _settings.Height > 0)
            {
                StartPosition = FormStartPosition.Manual;
                Location = new Point(Math.Max(0, _settings.Left), Math.Max(0, _settings.Top));
                Size = new Size(_settings.Width, _settings.Height);
            }

            if (_settings.SplitterDistance > 0)
            {
                splitContainer.SplitterDistance = Math.Min(_settings.SplitterDistance, splitContainer.Width - 150);
            }

            if (_settings.DetailsSplitterDistance > 0)
            {
                rightSplitContainer.SplitterDistance = Math.Min(_settings.DetailsSplitterDistance, rightSplitContainer.Height - 120);
            }

            if (!string.IsNullOrWhiteSpace(_settings.LastOpenedFile) && File.Exists(_settings.LastOpenedFile))
            {
                _ = LoadHarFileAsync(_settings.LastOpenedFile);
            }
        }

        private void AddQueryToHistory(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return;
            }

            _settings.QueryHistory ??= [];
            _settings.QueryHistory.RemoveAll(x => string.Equals(x, query, StringComparison.OrdinalIgnoreCase));
            _settings.QueryHistory.Insert(0, query);

            if (_settings.QueryHistory.Count > MaxQueryHistory)
            {
                _settings.QueryHistory = _settings.QueryHistory.Take(MaxQueryHistory).ToList();
            }

            RefreshQueryHistoryDropdown();
        }

        private void RefreshQueryHistoryDropdown()
        {
            queryComboBox.Items.Clear();

            var currentText = queryComboBox.Text;

            if (_settings.QueryHistory is null)
            {
                queryComboBox.Text = currentText;
                return;
            }

            foreach (var query in _settings.QueryHistory)
            {
                queryComboBox.Items.Add(query);
            }

            queryComboBox.Text = currentText;
        }

        private void SaveSettingsFromUi()
        {
            _settings.SearchFilter = GetQueryText();
            _settings.DarkTheme = darkThemeCheckBox.Checked;
            _settings.FilterEnabled = _isFilterEnabled;
            _settings.TagDefinitions = _tagDefinitions
                .Select(x => new TagDefinition
                {
                    Name = x.Name,
                    ColorArgb = x.ColorArgb
                })
                .ToList();
            SaveSettings();
        }

        private string GetQueryText()
        {
            return queryComboBox.Text.Trim();
        }

        private void SaveSettings()
        {
            _settings.Left = Left;
            _settings.Top = Top;
            _settings.Width = Width;
            _settings.Height = Height;
            _settings.SplitterDistance = splitContainer.SplitterDistance;
            _settings.DetailsSplitterDistance = rightSplitContainer.SplitterDistance;
            AppUiSettings.Save(_settingsFilePath, _settings);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_timelineDialog is not null && !_timelineDialog.IsDisposed)
            {
                _timelineDialog.Close();
            }

            SaveSettingsFromUi();
        }
    }
}
