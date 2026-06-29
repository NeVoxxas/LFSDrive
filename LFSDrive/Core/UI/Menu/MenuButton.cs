public sealed class MenuButton
{
    public byte ClickId { get; init; }

    public string Text { get; init; } = string.Empty;

    public byte Width { get; init; } = 35;

    public byte Height { get; init; } = 6;

    public byte Style { get; init; } = 0x20 | 0x08;

    public bool Enabled { get; init; } = true;
}