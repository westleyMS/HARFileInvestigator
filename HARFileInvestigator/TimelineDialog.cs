namespace HARFileInvestigator;

internal sealed class TimelineDialog : Form
{
    internal readonly record struct TimelinePoint(DateTimeOffset Timestamp, int RowIndex, string Url);

    private const double MinZoom = 1.0;
    private const double MaxZoom = 20.0;
    private const double ZoomStep = 1.25;

    private const int LeftMargin = 24;
    private const int RightMargin = 24;

    private readonly Panel _headerPanel;
    private readonly Panel _timelinePanel;
    private readonly Label _summaryLabel;
    private readonly Button _zoomOutButton;
    private readonly Button _zoomInButton;
    private readonly Label _zoomLabel;
    private readonly ToolTip _toolTip;
    private readonly System.Windows.Forms.Timer _hoverTimer;
    private readonly System.Windows.Forms.Timer _scrollStopTimer;
    private List<TimelinePoint> _points = [];
    private readonly List<(TimelinePoint Point, Rectangle Bounds)> _dotBounds = [];
    private readonly HashSet<int> _selectedRowIndices = [];
    private Color _dotColor = Color.FromArgb(255, 247, 186);
    private Color _axisColor = SystemColors.ControlDark;
    private Color _textColor = SystemColors.ControlText;
    private Color _hintColor = SystemColors.GrayText;
    private Color _selectedDotOutlineColor = Color.FromArgb(0, 122, 204);
    private int _selectedCount;
    private double _zoom = 1.0;
    private TimelinePoint? _hoverPoint;
    private Point _hoverLocation;

    public event EventHandler<int>? DotClicked;

    public TimelineDialog()
    {
        Text = "Highlight Timeline";
        StartPosition = FormStartPosition.CenterParent;
        MinimumSize = new Size(520, 240);
        Size = new Size(920, 360);

        _summaryLabel = new Label
        {
            Dock = DockStyle.Fill,
            Height = 28,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(8, 0, 0, 0),
            Text = "No highlighted rows"
        };

        _zoomOutButton = new Button
        {
            Dock = DockStyle.Right,
            Width = 30,
            Text = "-"
        };
        _zoomOutButton.Click += (_, _) => SetZoom(_zoom / ZoomStep);

        _zoomInButton = new Button
        {
            Dock = DockStyle.Right,
            Width = 30,
            Text = "+"
        };
        _zoomInButton.Click += (_, _) => SetZoom(_zoom * ZoomStep);

        _zoomLabel = new Label
        {
            Dock = DockStyle.Right,
            Width = 66,
            TextAlign = ContentAlignment.MiddleCenter,
            Text = "100%"
        };

        _headerPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 52
        };
        _headerPanel.Controls.Add(_summaryLabel);
        _headerPanel.Controls.Add(_zoomOutButton);
        _headerPanel.Controls.Add(_zoomInButton);
        _headerPanel.Controls.Add(_zoomLabel);

        _timelinePanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = SystemColors.Window,
            AutoScroll = true
        };

        _toolTip = new ToolTip
        {
            InitialDelay = 0,
            ReshowDelay = 0,
            AutoPopDelay = 8000
        };

        _hoverTimer = new System.Windows.Forms.Timer { Interval = 1000 };
        _hoverTimer.Tick += HoverTimer_Tick;

        _scrollStopTimer = new System.Windows.Forms.Timer { Interval = 180 };
        _scrollStopTimer.Tick += ScrollStopTimer_Tick;

        _timelinePanel.Paint += TimelinePanel_Paint;
        _timelinePanel.MouseDown += TimelinePanel_MouseDown;
        _timelinePanel.MouseMove += TimelinePanel_MouseMove;
        _timelinePanel.MouseLeave += TimelinePanel_MouseLeave;
        _timelinePanel.MouseWheel += TimelinePanel_MouseWheel;
        _timelinePanel.Scroll += TimelinePanel_Scroll;
        _timelinePanel.Resize += (_, _) =>
        {
            UpdateAutoScrollMinSize();
            _timelinePanel.Invalidate();
        };

        Controls.Add(_timelinePanel);
        Controls.Add(_headerPanel);

        ApplyTheme(false);
        UpdateAutoScrollMinSize();
    }

    public void ApplyTheme(bool dark)
    {
        if (!dark)
        {
            BackColor = SystemColors.Control;
            ForeColor = SystemColors.ControlText;
            _headerPanel.BackColor = SystemColors.Control;
            _summaryLabel.ForeColor = SystemColors.ControlText;
            _summaryLabel.BackColor = SystemColors.Control;
            _zoomLabel.ForeColor = SystemColors.ControlText;
            _zoomLabel.BackColor = SystemColors.Control;
            _zoomInButton.BackColor = SystemColors.Control;
            _zoomInButton.ForeColor = SystemColors.ControlText;
            _zoomOutButton.BackColor = SystemColors.Control;
            _zoomOutButton.ForeColor = SystemColors.ControlText;
            _timelinePanel.BackColor = SystemColors.Window;
            _axisColor = SystemColors.ControlDark;
            _textColor = SystemColors.ControlText;
            _hintColor = SystemColors.GrayText;
            _selectedDotOutlineColor = Color.FromArgb(0, 122, 204);
        }
        else
        {
            var back = Color.FromArgb(30, 30, 30);
            var fore = Color.FromArgb(220, 220, 220);
            var surface = Color.FromArgb(45, 45, 45);
            var header = Color.FromArgb(55, 55, 55);

            BackColor = back;
            ForeColor = fore;
            _headerPanel.BackColor = header;
            _summaryLabel.ForeColor = fore;
            _summaryLabel.BackColor = header;
            _zoomLabel.ForeColor = fore;
            _zoomLabel.BackColor = header;
            _zoomInButton.BackColor = header;
            _zoomInButton.ForeColor = fore;
            _zoomOutButton.BackColor = header;
            _zoomOutButton.ForeColor = fore;
            _timelinePanel.BackColor = surface;
            _axisColor = Color.FromArgb(120, 120, 120);
            _textColor = fore;
            _hintColor = Color.FromArgb(170, 170, 170);
            _selectedDotOutlineColor = Color.FromArgb(0, 122, 204);
        }

        _timelinePanel.Invalidate();
    }

    public void UpdatePoints(IReadOnlyCollection<TimelinePoint> points, Color dotColor, int selectedCount, IReadOnlyCollection<int> selectedRowIndices)
    {
        _points = points
            .OrderBy(x => x.Timestamp)
            .ToList();
        _dotColor = dotColor;
        _selectedCount = selectedCount;
        _selectedRowIndices.Clear();
        foreach (var rowIndex in selectedRowIndices)
        {
            _selectedRowIndices.Add(rowIndex);
        }

        UpdateSummary();
        UpdateAutoScrollMinSize();
        _timelinePanel.Invalidate();
    }

    private void UpdateSummary()
    {
        _summaryLabel.Text = _points.Count == 0
            ? $"Selected rows: {_selectedCount} | No highlighted rows"
            : $"Selected rows: {_selectedCount} | Highlighted rows: {_points.Count} | {_points.First().Timestamp:HH:mm:ss.fff} - {_points.Last().Timestamp:HH:mm:ss.fff}";
    }

    private void SetZoom(double zoom)
    {
        _zoom = Math.Clamp(zoom, MinZoom, MaxZoom);
        _zoomLabel.Text = $"{_zoom * 100:0}%";
        UpdateAutoScrollMinSize();
        _timelinePanel.Invalidate();
    }

    private void TimelinePanel_MouseWheel(object? sender, MouseEventArgs e)
    {
        if ((ModifierKeys & Keys.Control) != Keys.Control)
        {
            QueueScrollRefresh();
            return;
        }

        var oldZoom = _zoom;
        var nextZoom = e.Delta > 0 ? _zoom * ZoomStep : _zoom / ZoomStep;
        nextZoom = Math.Clamp(nextZoom, MinZoom, MaxZoom);
        if (Math.Abs(nextZoom - oldZoom) < 0.0001)
        {
            return;
        }

        var scrollXBefore = -_timelinePanel.AutoScrollPosition.X;
        var worldXBefore = scrollXBefore + e.X;
        var relativeBefore = worldXBefore - LeftMargin;

        _zoom = nextZoom;
        _zoomLabel.Text = $"{_zoom * 100:0}%";
        UpdateAutoScrollMinSize();

        var worldXAfter = LeftMargin + (relativeBefore * (_zoom / oldZoom));
        var targetScrollX = (int)Math.Round(worldXAfter - e.X);
        var maxScroll = Math.Max(0, _timelinePanel.AutoScrollMinSize.Width - _timelinePanel.ClientSize.Width);
        targetScrollX = Math.Clamp(targetScrollX, 0, maxScroll);
        _timelinePanel.AutoScrollPosition = new Point(targetScrollX, 0);

        _timelinePanel.Invalidate();
        QueueScrollRefresh();
    }

    private void TimelinePanel_Scroll(object? sender, ScrollEventArgs e)
    {
        QueueScrollRefresh();
    }

    private void QueueScrollRefresh()
    {
        _scrollStopTimer.Stop();
        _scrollStopTimer.Start();
    }

    private void ScrollStopTimer_Tick(object? sender, EventArgs e)
    {
        _scrollStopTimer.Stop();
        _timelinePanel.Invalidate();
    }

    private void TimelinePanel_MouseDown(object? sender, MouseEventArgs e)
    {
        var worldPoint = GetWorldPoint(e.Location);

        foreach (var dot in _dotBounds)
        {
            if (dot.Bounds.Contains(worldPoint))
            {
                DotClicked?.Invoke(this, dot.Point.RowIndex);
                break;
            }
        }
    }

    private void TimelinePanel_MouseMove(object? sender, MouseEventArgs e)
    {
        var worldPoint = GetWorldPoint(e.Location);

        foreach (var dot in _dotBounds)
        {
            if (!dot.Bounds.Contains(worldPoint))
            {
                continue;
            }

            if (_hoverPoint is TimelinePoint current && current.RowIndex == dot.Point.RowIndex)
            {
                return;
            }

            _hoverPoint = dot.Point;
            _hoverLocation = e.Location;
            _toolTip.Show($"Time: {dot.Point.Timestamp:HH:mm:ss.fff}", _timelinePanel, e.Location.X + 10, e.Location.Y + 12, 3000);
            _hoverTimer.Stop();
            _hoverTimer.Start();
            return;
        }

        HideHover();
    }

    private void TimelinePanel_MouseLeave(object? sender, EventArgs e)
    {
        HideHover();
    }

    private void HoverTimer_Tick(object? sender, EventArgs e)
    {
        _hoverTimer.Stop();
        if (_hoverPoint is not TimelinePoint point)
        {
            return;
        }

        var url = string.IsNullOrWhiteSpace(point.Url) ? "<no url>" : point.Url;
        var text = $"Time: {point.Timestamp:HH:mm:ss.fff}{Environment.NewLine}URL: {url}";
        _toolTip.Show(text, _timelinePanel, _hoverLocation.X + 10, _hoverLocation.Y + 12, 6000);
    }

    private void HideHover()
    {
        _hoverTimer.Stop();
        _hoverPoint = null;
        _toolTip.Hide(_timelinePanel);
    }

    private Point GetWorldPoint(Point viewPoint)
    {
        return new Point(viewPoint.X - _timelinePanel.AutoScrollPosition.X, viewPoint.Y);
    }

    private void UpdateAutoScrollMinSize()
    {
        var viewportWidth = Math.Max(200, _timelinePanel.ClientSize.Width - LeftMargin - RightMargin);
        var axisLength = Math.Max(1, (int)Math.Round(viewportWidth * _zoom));
        _timelinePanel.AutoScrollMinSize = new Size(axisLength + LeftMargin + RightMargin, 0);
    }

    private void TimelinePanel_Paint(object? sender, PaintEventArgs e)
    {
        var graphics = e.Graphics;
        graphics.Clear(_timelinePanel.BackColor);

        var bounds = _timelinePanel.ClientRectangle;
        if (bounds.Width <= 0 || bounds.Height <= 0)
        {
            return;
        }

        const int topMargin = 22;
        var axisY = topMargin + 44;
        var axisLeft = LeftMargin;
        var axisRight = Math.Max(axisLeft + 1, _timelinePanel.AutoScrollMinSize.Width - RightMargin);

        graphics.TranslateTransform(_timelinePanel.AutoScrollPosition.X, 0);

        using var axisPen = new Pen(_axisColor, 1f);
        graphics.DrawLine(axisPen, axisLeft, axisY, axisRight, axisY);

        _dotBounds.Clear();

        if (_points.Count == 0)
        {
            TextRenderer.DrawText(
                graphics,
                "Apply a query to highlight rows and plot timeline points.",
                Font,
                new Point(LeftMargin, axisY + 36),
                _hintColor);
            return;
        }

        var min = _points.First().Timestamp;
        var max = _points.Last().Timestamp;
        var totalMs = (max - min).TotalMilliseconds;

        using var dotBrush = new SolidBrush(_dotColor);
        using var selectedDotPen = new Pen(_selectedDotOutlineColor, 3f);
        const int dotRadius = 17;
        const int dotDiameter = dotRadius * 2;

        foreach (var point in _points)
        {
            var ratio = totalMs <= 0 ? 0.5 : (point.Timestamp - min).TotalMilliseconds / totalMs;
            var x = axisLeft + (int)Math.Round(ratio * (axisRight - axisLeft));
            var boundsRect = new Rectangle(x - dotRadius, axisY - dotRadius, dotDiameter, dotDiameter);
            graphics.FillEllipse(dotBrush, boundsRect);
            if (_selectedRowIndices.Contains(point.RowIndex))
            {
                graphics.DrawEllipse(selectedDotPen, boundsRect);
            }
            _dotBounds.Add((point, Rectangle.Inflate(boundsRect, 4, 4)));
        }

        var labelY = axisY + 52;
        var labelCount = GetLabelCount(graphics, axisRight - axisLeft);
        var labelFormat = GetLabelFormat(min, max);

        for (var i = 0; i < labelCount; i++)
        {
            var ratio = labelCount == 1 ? 0d : i / (double)(labelCount - 1);
            var x = axisLeft + (int)Math.Round(ratio * (axisRight - axisLeft));

            using var tickPen = new Pen(_axisColor, 1f);
            graphics.DrawLine(tickPen, x, axisY - 6, x, axisY + 6);

            var stamp = totalMs <= 0
                ? min
                : min.AddMilliseconds(totalMs * ratio);

            var labelText = stamp.ToString(labelFormat);
            var labelSize = TextRenderer.MeasureText(graphics, labelText, Font);
            TextRenderer.DrawText(
                graphics,
                labelText,
                Font,
                new Point(x - (labelSize.Width / 2), labelY),
                _textColor);
        }
    }

    private int GetLabelCount(Graphics graphics, int axisLength)
    {
        var sampleText = _points.Count > 0
            ? _points[0].Timestamp.ToString(GetLabelFormat(_points[0].Timestamp, _points[^1].Timestamp))
            : DateTimeOffset.Now.ToString("HH:mm:ss.fff");
        var sampleWidth = TextRenderer.MeasureText(graphics, sampleText, Font).Width;
        var minSpacing = Math.Max(80, sampleWidth + 16);

        var count = (axisLength / Math.Max(1, minSpacing)) + 1;
        return Math.Clamp(count, 2, 18);
    }

    private static string GetLabelFormat(DateTimeOffset min, DateTimeOffset max)
    {
        var span = max - min;
        if (span.TotalDays >= 1)
        {
            return "MM-dd HH:mm";
        }

        if (span.TotalMinutes >= 5)
        {
            return "HH:mm:ss";
        }

        if (span.TotalSeconds >= 15)
        {
            return "HH:mm:ss.ff";
        }

        return "HH:mm:ss.fff";
    }
}
