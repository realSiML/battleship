using BenMakesGames.PlayPlayMini.Attributes.DI;
using battleship.Models;

namespace battleship.Services;

[AutoRegister(Lifetime.Singleton)]
public sealed class CellFactory
{
    public Cell CreateCell(Vector2 position, CellStatus status)
    {
        return new Cell()
        {
            Position = new Vector2(position.X, position.Y),
            Status = status
        };
    }
}