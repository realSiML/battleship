namespace battleship.Models;

public sealed class Ship
{
    public Vector2 Position { get; }
    public int Decks { get; }
    public ShipOrientation Orientation { get; }
    public int Hits { get; set; } = 0;

    public Ship(Vector2 position, int decks, ShipOrientation orientation)
    {
        Position = position;
        Decks = decks;
        Orientation = orientation;
    }

}

public enum ShipOrientation
{
    Horizontal,
    Vertical
}