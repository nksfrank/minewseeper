namespace Minefield.Test.Unit;

[TestClass]
public class MinefieldTest
{
    private static IEnumerable<object[]> TestCases
    {
        get
        {
            return new[] {
                new object[] {
                  (3,3),
                  new (int, int)[]{},
                  new []{Minefield.Reveal(0, 0)},
                  @"You Won
                  __012
                  2 ...
                  1 ...
                  0 ...",
                  "No bombs should win the game"
                },
                new object[] {
                  (3,3),
                  new []{(0,0), (0,1), (0,2), (1,0), (1,2), (2,0), (2,1), (2,2)},
                  new []{Minefield.Reveal(1, 1)},
                  @"You Won
                  __012
                  2 ???
                  1 ?8?
                  0 ???",
                  "Picking all the bombs should win the game"
                },
                new object[] {
                  (3,3),
                  new []{(0,0), (0,1), (0,2), (1,0), (1,1), (1,2), (2,0), (2,1), (2,2)},
                  new []{Minefield.Reveal(1, 1)},
                  @"You Lost
                  __012
                  2 QQQ
                  1 QXQ
                  0 QQQ",
                  "Picking a bomb explodes and reveals all other bombs left, loosing the game"
                },
                new object[] {
                  (3,4),
                  new []{(0,2), (1, 2), (2,2)},
                  new []{Minefield.Reveal(0, 0)},
                  @"
                  __012
                  3 ???
                  2 ???
                  1 232
                  0 ...",
                  "a move should not open up past bombs"
                },
                new object[] {
                  (3,4),
                  new []{(0,3), (1, 3), (2,3)},
                  new []{Minefield.Reveal(0, 0)},
                  @"You Won
                  __012
                  3 ???
                  2 232
                  1 ...
                  0 ...",
                  "selecting an empty square should open up the board"
                },
                new object[] {
                  (3,3),
                  new []{(0,2), (1, 2), (2,2)},
                  new []{Minefield.Reveal(0, 0)},
                  @"You Won
                  __012
                  2 ???
                  1 232
                  0 ...",
                  "revealed squares count adjacent bombs"
                },
                new object[] {
                  (1,1),
                  new []{(0,0)},
                  new []{Minefield.Flag(0,0)},
                  @"You Won
                  __0
                  0 F",
                  "flagging all bombs should win the game"
                },
                new object[] {
                  (2,2),
                  new []{(0,0), (2,2)},
                  new []{Minefield.Flag(0,0)},
                  @"
                  __01
                  1 ??
                  0 F?",
                  "flagging should not end the game"
                },
                new object[] {
                  (2,2),
                  new []{(0,0), (2,2)},
                  new []{Minefield.Flag(0,0), Minefield.Flag(0,0)},
                  @"
                  __01
                  1 ??
                  0 ??",
                  "flagging again should unflag the cell"
                },
                new object[] {
                  (4,4),
                  new [] {(2,3),(1,2),(3,3)},
                  new [] {Minefield.Reveal(0,0)},
                  @"
                  __0123
                  3 ????
                  2 ??32
                  1 111.
                  0 ....",
                  "a move doesnt open up past non empty squares"
                },
                new object[] {
                  (4,4),
                  new [] {(2,3),(1,2),(3,3)},
                  new [] {Minefield.Reveal(0,0), Minefield.Flag(1, 2), Minefield.Reveal(3,3)},
                  @"You Lost
                  __0123
                  3 ??QX
                  2 ?B32
                  1 111.
                  0 ....",
                  "all parts combined should work as a whole"
                },
            };
        }
    }

    [TestMethod, DynamicData(nameof(TestCases))]
    public void Minefield_Gameloop_Integration((int, int) board, (int, int)[] bombs, IMove[] moves, string expected, string message)
    {
        var mf = new Minefield(board.Item1, board.Item2, bombs);
        foreach (var m in moves)
        {
            mf.MakeMove(m);
        }
        var actual = WithStringWriter(mf.Render);
        Assert.AreEqual(NormalizeMultilineStrings(expected), NormalizeMultilineStrings(actual), message);
    }

    [TestMethod]
    public void CreatingNewMinefield()
    {
        var bombs = Enumerable.Empty<(int, int)>();
        Assert.ThrowsException<MinefieldInvalidSizeException>(() => new Minefield(0, -1, bombs));
        Assert.ThrowsException<MinefieldInvalidSizeException>(() => new Minefield(-1, 0, bombs));

        var expected = @"
          __01234567
          9 ????????
          8 ????????
          7 ????????
          6 ????????
          5 ????????
          4 ????????
          3 ????????
          2 ????????
          1 ????????
          0 ????????";

        var mf = new Minefield(8, 10, bombs);
        var actual = WithStringWriter(mf.Render);
        Assert.AreEqual(NormalizeMultilineStrings(expected), NormalizeMultilineStrings(actual));
    }

    public static string NormalizeMultilineStrings(string input) => String.Join(
        Environment.NewLine,
        input
          .Replace("\r\n", "\n")
          .Replace("\r", "\n")
          .Split("\n")
          .Select(s => s.Trim()));
    private string WithStringWriter(Action<TextWriter> fn)
    {
        using (var sw = new StringWriter())
        {
            fn(sw);
            return sw.ToString();
        }
    }
}
