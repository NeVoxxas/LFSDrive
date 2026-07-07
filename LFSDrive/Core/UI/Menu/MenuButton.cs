public sealed class MenuButton
{
    public byte ClickId { get; init; }

    public string Text { get; init; } = string.Empty;

    public byte Width { get; init; } = 60;

    public byte Height { get; init; } = 6;

    public byte Style { get; init; } = 0x20 | 0x08;

    public bool Enabled { get; init; } = true;

    public byte TypeIn { get; init; } = 0;

    public string? Category { get; init; }

    public MenuColumn Column { get; init; } = MenuColumn.Left;
    public byte? Left { get; init; }

    public byte? Top { get; init; }
}

public enum MenuColumn
{
    Left,
    Right
}