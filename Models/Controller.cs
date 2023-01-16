namespace battleship.Models;

public sealed class Controller
{
    List<(int, int)> RandomCoords { get; } = new(100);
    List<(int, int)> FixedCoords { get; } = new(48);
    List<(int, int)> AroundCoords { get; } = new(4);
    (int, int) FirstHit { get; set; } = (-1, -1);
    ShipOrientation? Orientation { get; set; }
    private readonly List<((int, int), Direction)> StartPoints = new()
    {
        ((6,0), Direction.Down),
        ((2,0), Direction.Down),
        ((0,2), Direction.Down),
        ((0,6), Direction.Down),
        ((3,0), Direction.Up),
        ((7,0), Direction.Up),
        ((9,2), Direction.Up),
        ((9,6), Direction.Up),
    };

    public Controller()
    {
        var rand = new Random();
        // RandomCoords
        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                RandomCoords.Add((i, j));
            }
        }
        RandomCoords = RandomCoords.OrderBy(_ => rand.Next()).ToList();

        // Перемешиваем "схемы" стрельбы
        StartPoints = StartPoints.OrderBy(_ => rand.Next()).ToList();
        // FixedCoords ↘️ ↗️
        foreach (var startPoint in StartPoints)
        {
            var (y, x) = startPoint.Item1;
            var direction = startPoint.Item2;

            if (direction is Direction.Down)
            {
                while (x <= 9 && y <= 9)
                {
                    FixedCoords.Add((y, x));
                    x = (x <= 9) ? x : 9;
                    y = (y <= 9) ? y : 9;
                    x++; y++;
                }
            }
            else
            {
                while (y >= 0 && y <= 9 && x <= 9)
                {
                    FixedCoords.Add((y, x));
                    y = (y >= 0 && y <= 9) ? y : (y < 0) ? 0 : 9;
                    x = (x <= 9) ? x : 9;
                    y--; x++;
                };
            }
        }
    }

    public (int, int) GetNextCoords()
    {
        (int, int) coords;
        if (AroundCoords.Count > 0)
        {
            coords = AroundCoords[0];
        }
        else if (FixedCoords.Count > 0)
        {
            coords = FixedCoords[0];
        }
        else
        {
            coords = RandomCoords[0];
        }

        RemoveCoords(coords);

        return coords;
    }

    public void RemoveCoords((int, int) coords)
    {
        RandomCoords.Remove(coords);
        FixedCoords.Remove(coords);
        AroundCoords.Remove(coords);
    }

    public void SetCoordsAround((int, int) point, (int, int)[] coords)
    {
        if (FirstHit != (-1, -1))
        {
            FirstHit = point;
        }
        else if (Orientation is null)
        {
            if (Math.Abs(FirstHit.Item1 - point.Item1) == 1)
            {
                Orientation = ShipOrientation.Vertical;
            }
            else
            {
                Orientation = ShipOrientation.Horizontal;
            }
        }

        foreach (var coord in coords)
        {
            AroundCoords.Add(coord);
        }
    }

    public void ResetHit()
    {
        FirstHit = (-1, -1);
        Orientation = null;
    }
}

public enum Direction
{
    Up,
    Down
}
