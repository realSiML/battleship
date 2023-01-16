namespace battleship.Models;

public sealed class Cell
{
    public static int SIZE => 64;

    public Vector2 Position { get; set; }
    public CellStatus Status { get; set; }
    public Ship? Ship { get; set; }

    public int PixelX => (int)Position.X;
    public int PixelY => (int)Position.Y;
    public Rectangle Rectangle => new(PixelX, PixelY, SIZE, SIZE);
}

public enum CellStatus
{
    Default,
    Hit,
    Miss,
    Mark,
    Hidden
}