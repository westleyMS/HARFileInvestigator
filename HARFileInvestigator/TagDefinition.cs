namespace HARFileInvestigator;

internal sealed class TagDefinition
{
    public string Name { get; set; } = string.Empty;
    public int ColorArgb { get; set; } = Color.FromArgb(255, 224, 178).ToArgb();

    public Color Color
    {
        get => Color.FromArgb(ColorArgb);
        set => ColorArgb = value.ToArgb();
    }
}
