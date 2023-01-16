namespace battleship.Models;

public sealed class Field
{
    public static readonly int CELLS_COUNT = 10;
    public static int SIZE => Cell.SIZE * CELLS_COUNT;
    private static readonly int[][] SHIP_DATA = {
        new int[2]{1, 4},
        new int[2]{2, 3},
        new int[2]{3, 2},
        new int[2]{4, 1}
    };

    private readonly Vector2 _location;
    private readonly Cell[][] _cells;
    public List<Ship> Ships { get; } = new();

    public int Top => (int)_location.Y;
    public int Left => (int)_location.X;
    public int Bottom => (int)_location.Y + SIZE;
    public int Right => (int)_location.X + SIZE;

    public Field(Vector2 location, Cell[][] cells)
    {
        _location = location;
        _cells = cells;
    }

    public Cell this[int i, int j]
    {
        get => _cells[i][j];
    }

    public Cell? GetCellByPosition(Vector2 Position)
    {
        (int x, int y) = ((int)Position.X, (int)Position.Y);

        if (x < Left || x > Right || y < Top || y > Bottom)
            return null;

        return _cells[y / Cell.SIZE][x / Cell.SIZE];
    }

    public (int i, int j) GetIndexByPosition(Vector2 Position)
    {
        (int x, int y) = ((int)Position.X, (int)Position.Y);

        if (x < Left || x > Right || y < Top || y > Bottom)
            return (-1, -1);

        return ((y - Top) / Cell.SIZE, (x - Left) / Cell.SIZE);
    }

    public void SetShipsRandomly()
    {
        foreach (var ship_data in SHIP_DATA)
        {
            var count = ship_data[0];
            var decks = ship_data[1];

            for (int i = 0; i < count; i++)
            {
                GetDecksCoords(decks);
            }
        }
    }

    private void GetDecksCoords(int decks)
    {
        int x;
        int y;
        ShipOrientation orientation;
        var random = new Random();
        List<Cell> cells;

        do
        {
            cells = new(4);
            orientation = (ShipOrientation)random.Next(2);

            if (orientation is ShipOrientation.Horizontal)
            {
                x = random.Next(11 - decks);
                y = random.Next(10);

                for (int i = 0; i < decks; i++)
                {
                    cells.Add(_cells[y][x + i]);
                }
            }
            else
            {
                x = random.Next(10);
                y = random.Next(11 - decks);

                for (int i = 0; i < decks; i++)
                {
                    cells.Add(_cells[y + i][x]);
                }
            }

        } while (!CheckShipLocation(x, y, decks, orientation));

        var ship = new Ship(new Vector2(_cells[y][x].Position.X, _cells[y][x].Position.Y), decks, orientation);
        cells.ForEach(cell => cell.Ship = ship);
        Ships.Add(ship);
    }

    private bool CheckShipLocation(int x, int y, int decks, ShipOrientation orientation)
    {
        int fromRow = y - 1;
        int fromCol = x - 1;
        int toRow;
        int toCol;

        if (orientation is ShipOrientation.Horizontal)
        {
            toCol = x + decks;
            toRow = y + 1;
        }
        else
        {
            toCol = x + 1;
            toRow = y + decks;
        }

        for (int i = fromRow; i < toRow + 1; i++)
        {
            if (i < 0 || i >= CELLS_COUNT) continue;

            for (int j = fromCol; j < toCol + 1; j++)
            {
                if (j < 0 || j >= CELLS_COUNT) continue;

                if (_cells[i][j].Ship is not null) return false;
            }
        }

        return true;
    }
}

// 1 - 4 палуб
// 2 - 3 палуб
// 3 - 2 палуб
// 4 - 1 палуб
