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
            ({ Revealed: false, IsBomb: true }, GameState.Lost) => "Q",
            ({ Revealed: false, IsFlagged: true }, _) => "F",
            ({ Revealed: false }, _) => "?",
            ({ IsBomb: true }, _) => "X",
            ({ Neighbours: 0 }, _) => ".",
            _ => Neighbours
        });
    }
}

public enum MoveType
{
    Flag,
    Deflag,
    Reveal
}
public class Move
{
    public MoveType Type { get; init; }
    public int X { get; init; }
    public int Y { get; init; }

    private Move(MoveType type, int x, int y)
    {
        Type = type;
        X = x;
        Y = y;
    }

    public static Move Reveal(int x, int y) => new Move(MoveType.Reveal, x, y);
    public static Move Flag(int x, int y) => new Move(MoveType.Flag, x, y);
    public static Move Deflag(int x, int y) => new Move(MoveType.Deflag, x, y);
}

public enum GameState
{
    Lost = -1,
    Runnig,
    Won
}

public class Minefield
{
    public int Cols => board[0].Length;
    public int Rows => board.Length;
    public GameState State { get; private set; }

    private int mines;
    private Cell[][] board;
    public Cell[][] Board => board;//TODO: Return a copy and not actually the internal reference

    public Minefield(int x, int y, int mines)
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
        this.mines = mines;
    }

    public bool SetBomb(int x, int y)
    {
        if (!ValidPosition(x, y))
        {
            return false;
        }
        this.board[y][x] = new Cell(false, true);
        return true;
    }

    private bool ValidPosition((int, int) c) => ValidPosition(c.Item1, c.Item2);
    private bool ValidPosition(int x, int y)
    {
        return 0 <= x && x < Cols && 0 <= y && y < Rows;
    }

    public void MakeMove(Move move)
    {
        if (!ValidPosition(move.X, move.Y))
        {
            return;
        }
        Update(move);
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

        for (var row = Rows - 1; 0 <= row; row--)
        {
            w.Write($"{row} ");
            for (var col = 0; col < Cols; col++)
            {
                board[row][col].Render(w, this);
            }
            if (row != 0)
            {
                w.Write(Environment.NewLine);
            }
        }
    }

    private readonly static (int x, int y)[] matrix = {
      (-1, -1), (0, -1), (1, -1),
      (-1, 0), (1, 0),
      (-1, 1), (0, 1), (1, 1)
    };

    private Cell GetCell(Move move) => GetCell(move.X, move.Y);
    private Cell GetCell((int x, int y) c) => GetCell(c.x, c.y);
    private Cell GetCell(int x, int y) => board[y][x];

    private void Update(Move move)
    {
        var cellsSeen = new HashSet<(int, int)>();
        var moves = new Queue<Move>();
        moves.Enqueue(move);
        while (moves.Any())
        {
            var mv = moves.Dequeue();
            ProcessCell(mv, moves, cellsSeen);
        }

        var flat = board.SelectMany(r => r.Select(c => c));
        var aBombIsRevealed = flat.Any(c => c.IsBomb && c.Revealed);
        var allCellsAreRevealed = flat.Where(c => !c.IsBomb).All(c => c.Revealed);
        var allBombsAreFlagged = flat.All(c => (c.IsBomb && c.IsFlagged));
        State = (allCellsAreRevealed, allBombsAreFlagged, aBombIsRevealed) switch
        {
            (true, _, false) => GameState.Won,
            (_, _, true) => GameState.Lost,
            _ => State
        };

    }

    private void ProcessCell(Move mv, Queue<Move> moves, HashSet<(int, int)> cellsSeen)
    {
        var c = GetCell(mv);

        switch (mv.Type)
        {
            case MoveType.Reveal:
                RevealCell(mv, moves, cellsSeen, c);
                break;
            case MoveType.Flag:
                c.IsFlagged = true;
                break;
            case MoveType.Deflag:
                c.IsFlagged = false;
                break;
        }
    }

    private void RevealCell(Move mv, Queue<Move> moves, HashSet<(int, int)> cellsSeen, Cell c)
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

    private IEnumerable<(int, int)> NeighboringCells(Move mv) => matrix.Select(m => (mv.X + m.x, mv.Y + m.y)).Where(ValidPosition);
    private int CountProximityBombs(IEnumerable<(int, int)> neighbours) => neighbours.Select(GetCell).Count(c => c.IsBomb);

    private void EnqueueUnseenNeighbours(IEnumerable<(int, int)> neighbours, Queue<Move> moves, HashSet<(int, int)> cellsSeen)
    {
        foreach (var (dx, dy) in neighbours)
        {
            if (cellsSeen.Contains((dx, dy)))
            {
                continue;
            }
            cellsSeen.Add((dx, dy));
            moves.Enqueue(Move.Reveal(dx, dy));
        }
    }
}
