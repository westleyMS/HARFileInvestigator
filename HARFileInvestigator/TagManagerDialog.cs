using System.ComponentModel;

namespace HARFileInvestigator;

internal sealed class TagManagerDialog : Form
{
    private readonly BindingList<TagDefinition> _tags;
    private readonly DataGridView _grid;
    private readonly TextBox _newTagTextBox;

    public List<TagDefinition> Tags { get; }

    public TagManagerDialog(IReadOnlyCollection<TagDefinition> tags)
    {
        Text = "Tag Manager";
        StartPosition = FormStartPosition.CenterParent;
        MinimumSize = new Size(520, 360);
        Size = new Size(640, 420);

        _tags = new BindingList<TagDefinition>(tags
            .Select(x => new TagDefinition { Name = x.Name, ColorArgb = x.ColorArgb })
            .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .ToList());

        _grid = new DataGridView
        {
            Dock = DockStyle.Fill,
            AutoGenerateColumns = false,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            RowHeadersVisible = false
        };
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(TagDefinition.Name),
            HeaderText = "Tag",
            Width = 260
        });
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "ColorPreview",
            HeaderText = "Color",
            Width = 120,
            ReadOnly = true
        });
        _grid.Columns.Add(new DataGridViewButtonColumn
        {
            Name = "PickColor",
            HeaderText = string.Empty,
            Text = "Choose...",
            UseColumnTextForButtonValue = true,
            Width = 90
        });
        _grid.CellContentClick += Grid_CellContentClick;
        _grid.CellFormatting += Grid_CellFormatting;
        _grid.DataSource = _tags;

        _newTagTextBox = new TextBox { Anchor = AnchorStyles.Left | AnchorStyles.Right, Width = 260 };
        var addButton = new Button { Text = "Add", Width = 80 };
        addButton.Click += (_, _) => AddTag();

        var removeButton = new Button { Text = "Remove", Width = 90 };
        removeButton.Click += (_, _) => RemoveSelectedTag();

        var okButton = new Button { Text = "OK", Width = 90, DialogResult = DialogResult.OK };
        okButton.Click += (_, _) => SaveResult();
        var cancelButton = new Button { Text = "Cancel", Width = 90, DialogResult = DialogResult.Cancel };

        var topPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            Height = 36,
            FlowDirection = FlowDirection.LeftToRight,
            Padding = new Padding(6, 6, 6, 0)
        };
        topPanel.Controls.Add(_newTagTextBox);
        topPanel.Controls.Add(addButton);
        topPanel.Controls.Add(removeButton);

        var bottomPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            Height = 44,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(6, 6, 6, 6)
        };
        bottomPanel.Controls.Add(okButton);
        bottomPanel.Controls.Add(cancelButton);

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40f));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50f));

        layout.Controls.Add(topPanel, 0, 0);
        layout.Controls.Add(_grid, 0, 1);
        layout.Controls.Add(bottomPanel, 0, 2);

        Controls.Add(layout);

        Tags = [];
    }

    private void AddTag()
    {
        var name = _newTagTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            return;
        }

        var existing = _tags.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
        if (existing is null)
        {
            _tags.Add(new TagDefinition
            {
                Name = name,
                Color = GetDefaultColor(name)
            });
        }

        _newTagTextBox.Clear();
    }

    private void RemoveSelectedTag()
    {
        if (_grid.CurrentRow?.DataBoundItem is not TagDefinition selected)
        {
            return;
        }

        _tags.Remove(selected);
    }

    private void Grid_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        if (e.RowIndex < 0)
        {
            return;
        }

        if (_grid.Columns[e.ColumnIndex].Name != "ColorPreview")
        {
            return;
        }

        if (_grid.Rows[e.RowIndex].DataBoundItem is not TagDefinition tag)
        {
            return;
        }

        e.Value = $"#{tag.Color.R:X2}{tag.Color.G:X2}{tag.Color.B:X2}";
        e.CellStyle.BackColor = tag.Color;
        e.CellStyle.ForeColor = GetContrastTextColor(tag.Color);
        e.FormattingApplied = true;
    }

    private void Grid_CellContentClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0 || _grid.Columns[e.ColumnIndex].Name != "PickColor")
        {
            return;
        }

        if (_grid.Rows[e.RowIndex].DataBoundItem is not TagDefinition tag)
        {
            return;
        }

        using var dialog = new ColorDialog
        {
            AllowFullOpen = true,
            FullOpen = true,
            Color = tag.Color
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        tag.Color = dialog.Color;
        _grid.Refresh();
    }

    private void SaveResult()
    {
        Tags.Clear();
        foreach (var tag in _tags.Where(x => !string.IsNullOrWhiteSpace(x.Name)))
        {
            Tags.Add(new TagDefinition
            {
                Name = tag.Name.Trim(),
                ColorArgb = tag.ColorArgb
            });
        }
    }

    private static Color GetDefaultColor(string key)
    {
        var hash = key.ToLowerInvariant().GetHashCode();
        var hue = Math.Abs(hash % 360);
        return ColorFromHsv(hue, 0.40, 0.95);
    }

    private static Color ColorFromHsv(double hue, double saturation, double value)
    {
        var hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
        var f = hue / 60 - Math.Floor(hue / 60);

        value = value * 255;
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
}
