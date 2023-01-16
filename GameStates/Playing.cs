using battleship.Models;
using battleship.Services;
using BenMakesGames.PlayPlayMini;
using BenMakesGames.PlayPlayMini.Services;
using Microsoft.Xna.Framework.Input;

namespace battleship.GameStates;

// sealed classes execute faster than non-sealed, so always seal your game states!
public sealed class Playing : BenMakesGames.PlayPlayMini.GameState
{
    private GraphicsManager Graphics { get; }
    private KeyboardManager Keyboard { get; }
    private MouseManager Mouse { get; }
    private GameStateManager GSM { get; }
    private CellFactory CellFactory { get; }

    private Field HumanField { get; set; }
    private Field CompField { get; set; }
    private Controller Controller { get; set; }

    private static readonly int TOP_MARGIN = 64;
    private static readonly int HF_MARGIN = 30;
    private static readonly int CF_MARGIN = HF_MARGIN + Field.SIZE + 25;

    private BoardState Board_State { get; set; }
    private List<Rectangle> RedLines { get; set; } = new(20);
    private bool IsCompTurn { get; set; }

    public Playing(GraphicsManager graphics, GameStateManager gsm, KeyboardManager keyboard, MouseManager mouse, CellFactory cellFactory)
    {
        Graphics = graphics;
        GSM = gsm;
        Keyboard = keyboard;
        Mouse = mouse;
        Mouse.UseSystemCursor();

        CellFactory = cellFactory;

        HumanField = new(Vector2.Zero, Array.Empty<Cell[]>());
        CompField = new(Vector2.Zero, Array.Empty<Cell[]>());

        Controller = new();
    }

    public override void ActiveInput(GameTime gameTime)
    {
        if (Keyboard.PressedKey(Keys.Escape))
        {
            GSM.Exit();
        }
        else if (Keyboard.PressedKey(Keys.R))
        {
            Restart();
        }

        if (Keyboard.KeyDown(Keys.RightAlt) && Keyboard.PressedKey(Keys.Enter))
        {
            Graphics.SetFullscreen(Graphics.FullScreen == false);
        }

        if (Mouse.LeftClicked || Mouse.RightClicked)
        {
            var mouseXY = new Point(Mouse.X, Mouse.Y);
            MouseInput(Board_State, mouseXY);
        }

    }

    public override void ActiveUpdate(GameTime gameTime)
    {
        if (CompField.Ships.Count == 0)
        {
            Board_State = BoardState.EndGame;
            GSM.ChangeState<EndGameMenu, EndGameMenuConfig>(new(this, "You Win :)"));
        }

        CompLogic();

        if (HumanField.Ships.Count == 0)
        {
            Board_State = BoardState.EndGame;

            for (int i = 0; i < Field.CELLS_COUNT; i++)
            {
                for (int j = 0; j < Field.CELLS_COUNT; j++)
                {
                    if (CompField[i, j].Status is CellStatus.Hidden) CompField[i, j].Status = CellStatus.Default;
                }
            }

            GSM.ChangeState<EndGameMenu, EndGameMenuConfig>(new(this, "You lose :("));
        }
    }

    public override void AlwaysUpdate(GameTime gameTime)
    {
    }

    public override void ActiveDraw(GameTime gameTime)
    {
        Mouse.ActiveDraw(gameTime);
    }

    public override void AlwaysDraw(GameTime gameTime)
    {
        Graphics.Clear(Color.LightGray);

        for (int i = 0; i < Field.CELLS_COUNT; i++)
        {
            for (int j = 0; j < Field.CELLS_COUNT; j++)
            {
                DrawCell(HumanField[i, j]);
                DrawCell(CompField[i, j]);
            }
        }

        foreach (var rect in RedLines)
        {
            Graphics.DrawFilledRectangle(rect.X, rect.Y, rect.Width, rect.Height, Color.Crimson);
        }
    }

    public override void Enter()
    {
        Restart();
    }

    public override void Leave()
    {
    }

    // *** Методы ***

    public void Restart()
    {
        Controller = new();
        RedLines = new(20);
        Board_State = BoardState.Playing;
        var random = new Random();
        IsCompTurn = random.Next(2) < 1;

        var N = Field.CELLS_COUNT;

        Cell[][] cells = new Cell[N][];
        for (int i = 0; i < N; i++)
        {
            cells[i] = new Cell[N];
            for (int j = 0; j < N; j++)
            {
                cells[i][j] = CellFactory.CreateCell(new Vector2(HF_MARGIN + Cell.SIZE * j, TOP_MARGIN + Cell.SIZE * i), CellStatus.Default);
            }
        }
        HumanField = new(new Vector2(HF_MARGIN, TOP_MARGIN), cells);
        HumanField.SetShipsRandomly();

        cells = new Cell[N][];
        for (int i = 0; i < N; i++)
        {
            cells[i] = new Cell[N];
            for (int j = 0; j < N; j++)
            {
                cells[i][j] = CellFactory.CreateCell(new Vector2(CF_MARGIN + Cell.SIZE * j, TOP_MARGIN + Cell.SIZE * i), CellStatus.Hidden);
            }
        }
        CompField = new(new Vector2(CF_MARGIN, TOP_MARGIN), cells);
        CompField.SetShipsRandomly();
    }

    public void DrawCell(Cell cell)
    {
        var sprite = cell.Status switch
        {
            CellStatus.Hit => "Hit",
            CellStatus.Miss => "Miss",
            CellStatus.Mark => "Mark",
            _ => String.Empty,
        };

        var color = cell.Ship is not null && cell.Status is not CellStatus.Hidden ? Color.RoyalBlue : Color.White;
        if (cell.Status is CellStatus.Mark) color = Color.White;

        Graphics.DrawSprite("Cell", cell.PixelX, cell.PixelY, 0, color);
        if (sprite != String.Empty)
        {
            Graphics.DrawSprite(sprite, cell.PixelX, cell.PixelY, 0, Color.White);
        }
    }

    public void MouseInput(BoardState boardstate, Point mouseXY)
    {
        if (boardstate is BoardState.EndGame || IsCompTurn) return;

        if (boardstate is BoardState.Planing)
        {
            // if (mouseXY.X < CompField.Left || mouseXY.X > CompField.Right || mouseXY.Y < CompField.Top || mouseXY.Y > CompField.Bottom) return;
            Board_State = BoardState.Playing;
        }

        if (boardstate is BoardState.Playing)
        {
            if (mouseXY.X < CompField.Left || mouseXY.X > CompField.Right || mouseXY.Y < CompField.Top || mouseXY.Y > CompField.Bottom) return;

            var mouseRectangle = new Rectangle(mouseXY.X, mouseXY.Y, 1, 1);

            var cell = GetIntersectedCell(mouseRectangle);

            if (cell.Ship is not null && cell.Status is CellStatus.Hidden && Mouse.LeftClicked)
            {
                cell.Status = CellStatus.Hit;
                if (++cell.Ship.Hits == cell.Ship.Decks)
                {
                    Sink(cell.Ship, CompField);
                }
                return;
            }
            else if (cell.Status is CellStatus.Hidden)
            {
                if (Mouse.RightClicked)
                {
                    cell.Status = CellStatus.Mark;
                    return;
                }

                if (Mouse.LeftClicked)
                {
                    cell.Status = CellStatus.Miss;
                    IsCompTurn = true;
                }
            }
            else if (cell.Status is CellStatus.Mark && Mouse.RightClicked)
            {
                cell.Status = CellStatus.Hidden;
                return;
            }
        }
    }

    public Cell GetIntersectedCell(Rectangle mouseRectangle)
    {
        for (int i = 0; i < Field.CELLS_COUNT; i++)
        {
            for (int j = 0; j < Field.CELLS_COUNT; j++)
            {
                var cell = CompField[i, j];

                if (!mouseRectangle.Intersects(cell.Rectangle)) continue;

                return cell;
            }
        }

        return new Cell();
    }

    public void CompLogic()
    {
        if (!IsCompTurn || Board_State is BoardState.Planing) return;

        var (y, x) = Controller.GetNextCoords();
        var cell = HumanField[y, x];

        if (cell.Ship is null)
        {
            cell.Status = CellStatus.Miss;
            IsCompTurn = false;
        }
        else
        {
            cell.Status = CellStatus.Hit;
            MarkCellsAroundHit((y, x));
            if (++cell.Ship.Hits == cell.Ship.Decks)
            {
                // Очистить контроллер
                Controller.ResetHit();
                MarkCellsAroundShip(cell.Ship);
                Sink(cell.Ship, HumanField);
                return;
            }
            else
            {
                List<(int, int)> temp = new(4);
                for (int k = -1; k <= 1; k++)
                {
                    if (k == 0) continue;

                    if (x + k >= 0 && x + k < Field.CELLS_COUNT && HumanField[y, x + k].Status is CellStatus.Default) temp.Add((y, x + k));
                    if (y + k >= 0 && y + k < Field.CELLS_COUNT && HumanField[y + k, x].Status is CellStatus.Default) temp.Add((y + k, x));
                }

                var coords = temp.ToArray();
                var rand = new Random();
                coords = coords.OrderBy(_ => rand.Next()).ToArray();
                Controller.SetCoordsAround((y, x), coords);
            }
        }
    }

    public void MarkCell((int, int) coords)
    {
        if (coords.Item1 < 0 || coords.Item1 >= Field.CELLS_COUNT || coords.Item2 < 0 || coords.Item2 >= Field.CELLS_COUNT) return;

        HumanField[coords.Item1, coords.Item2].Status = CellStatus.Mark;
        Controller.RemoveCoords(coords);
    }

    public void MarkCellsAroundHit((int, int) coords)
    {
        var (y, x) = coords;

        var markers = new (int, int)[4]
        {
            (y - 1, x - 1),
            (y - 1, x + 1),
            (y + 1, x - 1),
            (y + 1, x + 1)
        };

        foreach (var mark in markers)
        {
            MarkCell(mark);
        }
    }

    public void MarkCellsAroundShip(Ship ship)
    {
        var (y, x) = HumanField.GetIndexByPosition(ship.Position);
        // System.Console.WriteLine($"Позиция: {ship.Position}\nX: {x}, Y: {y}");
        var decks = ship.Decks;
        var orientation = ship.Orientation;


        List<(int, int)> cells = new(4);

        if (decks == 1)
        {
            cells.Add((y - 1, x));
            cells.Add((y + 1, x));
            cells.Add((y, x - 1));
            cells.Add((y, x + 1));
        }
        else if (orientation is ShipOrientation.Horizontal)
        {
            cells.Add((y, x - 1));
            cells.Add((y, x + decks));
        }
        else
        {
            cells.Add((y - 1, x));
            cells.Add((y + decks, x));
        }

        foreach (var cell in cells)
        {
            MarkCell(cell);
        }
    }

    public void Sink(Ship ship, Field field)
    {
        field.Ships.Remove(ship);

        (int x, int y) = ((int)ship.Position.X, (int)ship.Position.Y);
        var orientation = ship.Orientation;
        var lineThickness = 4;
        var width = lineThickness;
        var height = lineThickness;

        if (orientation is ShipOrientation.Vertical)
        {
            x += Cell.SIZE / 2 - lineThickness / 2;
            y += lineThickness;
            height = ship.Decks * Cell.SIZE - lineThickness * 2;
        }
        else
        {
            x += lineThickness;
            y += Cell.SIZE / 2 - lineThickness / 2;
            width = ship.Decks * Cell.SIZE - lineThickness * 2;
        }

        var redLine = new Rectangle(x, y, width, height);
        RedLines.Add(redLine);
    }
}