using Coords = (int x, int y);

namespace Minefield;


public class MinefieldException(string msg) : Exception(msg);
public class MinefieldInvalidSizeException(string msg) : MinefieldException(msg);

public class Cell
{
    public int Neighbours { get; set; }
    public bool IsBomb { get; private set; }
    public bool IsFlagged { get; set; }
    public bool Revealed { get; set; }

    public Cell(bool revealed, bool isBomb)
    {
        IsBomb = isBomb;
        Revealed = revealed;
    }

    public void Render(TextWriter w, Minefield g)
    {
        w.Write((this, g.State) switch
        {
            ({ Revealed: false, IsBomb: true, IsFlagged: false }, GameState.Lost) => "Q",
            ({ Revealed: false, IsBomb: true, IsFlagged: true }, GameState.Lost) => "B",
            ({ Revealed: false, IsFlagged: true }, _) => "F",
            ({ Revealed: false }, _) => "?",
            ({ IsBomb: true }, _) => "X",
            ({ Neighbours: 0 }, _) => ".",
            _ => Neighbours
        });
    }
}

public interface IMove
{
    Coords Coords { get; }
}
public record Reveal(Coords Coords) : IMove;
public record Flag(Coords Coords) : IMove;

public enum GameState
{
    Lost = -1,
    Runnig,
    Won
}

public class Minefield
{
    public static IMove Reveal(int x, int y) => new Reveal((x, y));
    public static IMove Flag(int x, int y) => new Flag((x, y));
    public int Cols => board[0].Length;
    public int Rows => board.Length;
    public GameState State { get; private set; }

    private Cell[][] board;

    public Minefield(int x, int y, IEnumerable<Coords> mines)
    {
        if (x < 0 || y < 0)
        {
            throw new MinefieldInvalidSizeException("the board must contain rows and columns");
        }
        this.board = new Cell[y][];
        var newRow = Enumerable.Range(0, x).Select(x => new Cell(false, false));
        for (var i = 0; i < y; i++)
        {
            board[i] = newRow.ToArray();
        }

        foreach (var m in mines)
        {
            SetBomb(m);
        }
    }

    public bool SetBomb(Coords m)
    {
        if (!ValidPosition(m))
        {
            return false;
        }
        var (x, y) = m;
        this.board[y][x] = new Cell(false, true);
        return true;
    }

    private bool ValidPosition(Coords c)
    {
        var (x, y) = c;
        return 0 <= x && x < Cols && 0 <= y && y < Rows;
    }

    public void Render(TextWriter w)
    {
        w.WriteLine(State switch
        {
            GameState.Lost => "You Lost",
            GameState.Won => "You Won",
            _ => "",
        });
        w.Write("__");
        for (var col = 0; col < Cols; col++)
        {
            w.Write(col);
        }
        w.Write(Environment.NewLine);

        for (var y = Rows - 1; 0 <= y; y--)
        {
            w.Write($"{y} ");
            for (var x = 0; x < Cols; x++)
            {
                board[y][x].Render(w, this);
            }
            if (y != 0)
            {
                w.Write(Environment.NewLine);
            }
        }
    }

    private static readonly Coords[] neighborMatrix = {
      (-1, -1), (0, -1), (1, -1),
      (-1, 0), (1, 0),
      (-1, 1), (0, 1), (1, 1)
    };

    private Cell GetCell(Coords move)
    {
        var (x, y) = move;
        return board[y][x];
    }


    public void MakeMove(IMove move)
    {
        if (!ValidPosition(move.Coords))
        {
            return;
        }

        var cellsSeen = new HashSet<(int, int)>();
        var moves = new Queue<IMove>();
        moves.Enqueue(move);
        while (moves.Any())
        {
            var mv = moves.Dequeue();
            ProcessCell(mv, moves, cellsSeen);
        }

        CheckGameState();
    }

    private void ProcessCell(IMove mv, Queue<IMove> moves, HashSet<Coords> cellsSeen)
    {
        var c = GetCell(mv.Coords);

        switch (mv)
        {
            case Reveal r:
                RevealCell(r, moves, cellsSeen, c);
                break;
            case Flag _:
                c.IsFlagged = !c.IsFlagged;
                break;
        }
    }

    private void RevealCell(IMove mv, Queue<IMove> moves, HashSet<Coords> cellsSeen, Cell c)
    {
        c.Revealed = true;
        if (c.IsBomb)
        {
            moves.Clear();
            return;
        }

        var neighbors = NeighboringCells(mv).ToArray();
        c.Neighbours = CountProximityBombs(neighbors);
        if (0 < c.Neighbours)
        {
            return;
        }

        EnqueueUnseenNeighbours(neighbors, moves, cellsSeen);
    }

    private IEnumerable<Coords> NeighboringCells(IMove mv) => neighborMatrix.Select(m => (mv.Coords.x + m.x, mv.Coords.y + m.y)).Where(ValidPosition);
    private int CountProximityBombs(IEnumerable<(int, int)> neighbours) => neighbours.Select(GetCell).Count(c => c.IsBomb);

    private void EnqueueUnseenNeighbours(IEnumerable<Coords> neighbours, Queue<IMove> moves, HashSet<Coords> cellsSeen)
    {
        foreach (var (dx, dy) in neighbours)
        {
            if (cellsSeen.Contains((dx, dy)))
            {
                continue;
            }
            cellsSeen.Add((dx, dy));
            moves.Enqueue(new Reveal((dx, dy)));
        }
    }

    private void CheckGameState()
    {
        var cells = board.SelectMany(r => r.Select(c => c)).ToList();

        var aBombIsRevealed = cells.Any(c => c.IsBomb && c.Revealed);
        var allCellsAreRevealed = cells.Where(c => !c.IsBomb).All(c => c.Revealed);
        var allBombsAreFlagged = cells.All(c => (c.IsBomb && c.IsFlagged));

        State = (allCellsAreRevealed, allBombsAreFlagged, aBombIsRevealed) switch
        {
            (_, _, true) => GameState.Lost,
            (true, _, _) => GameState.Won,
            (_, true, _) => GameState.Won,
            _ => State
        };
    }
}
