namespace Minefield.Test.Unit;

[TestClass]
public class MinefieldTest
{

    private static IEnumerable<object[]> ATestData
    {
        get
        {
            return new[] {
               new object[] {
                 (3,3, 0),
                 new (int, int)[]{},
                 new []{Move.Reveal(0, 0)},
                 @"You Won
                   __012
                   2 ...
                   1 ...
                   0 ...",
               },
                new object[] {
                  (3,3,8),
                  new []{(0,0), (0,1), (0,2), (1,0), (1,2), (2,0), (2,1), (2,2)},
                  new []{Move.Reveal(1, 1)},
                  @"You Won
                    __012
                    2 ###
                    1 #8#
                    0 ###",
                },
                new object[] {
                  (3,3,8),
                  new []{(0,0), (0,1), (0,2), (1,0), (1,1), (1,2), (2,0), (2,1), (2,2)},
                  new []{Move.Reveal(1, 1)},
                  @"You Lost
                    __012
                    2 QQQ
                    1 QXQ
                    0 QQQ",
                },
                new object[] {
                   (3,4,3),
                   new []{(0,2), (1, 2), (2,2)},
                   new []{Move.Reveal(0, 0)},
                   @"
                     __012
                     3 ###
                     2 ###
                     1 232
                     0 ...",
                 },
                new object[] {
                   (3,4,3),
                   new []{(0,3), (1, 3), (2,3)},
                   new []{Move.Reveal(0, 0)},
                   @"You Won
                     __012
                     3 ###
                     2 232
                     1 ...
                     0 ...",
                 },
                new object[] {
                   (3,3,3),
                   new []{(0,2), (1, 2), (2,2)},
                   new []{Move.Reveal(0, 0)},
                   @"You Won
                     __012
                     2 ###
                     1 232
                     0 ...",
                 },
                new object[] {
                  (1,1,1),
                  new []{(0,0)},
                  new []{Move.Flag(0,0)},
                  @"You Won
                    __0
                    0 F",
                },
                new object[] {
                  (4,4,3),
                  new [] {(2,3),(1,2),(3,3)},
                  new [] {Move.Reveal(0,0)},
                  @"
                    __0123
                    3 ####
                    2 ##32
                    1 111.
                    0 ....",
                },
                new object[] {
                  (4,4,3),
                  new [] {(2,3),(1,2),(3,3)},
                  new [] {Move.Reveal(0,0), Move.Reveal(3,3)},
                  @"You Lost
                    __0123
                    3 ##QX
                    2 #Q32
                    1 111.
                    0 ....",
                },
            };
        }
    }

    private string BoardToString(string board) => String.Join(Environment.NewLine, board.Split(Environment.NewLine).Select(s => s.Trim()));
    [TestMethod, DynamicData(nameof(ATestData))]
    public void ATest((int, int, int) board, (int, int)[] bombs, Move[] moves, string expected)
    {
        var mf = new Minefield(board.Item1, board.Item2, board.Item3);
        foreach (var (x, y) in bombs)
        {
            mf.SetBomb(x, y);
        }

        foreach (var m in moves)
        {
            mf.MakeMove(m);
        }
        var actual = WithStringWriter(mf.Render);
        Assert.AreEqual(BoardToString(expected), actual);
    }

    [TestMethod]
    public void CreatingNewMinefield()
    {
        Assert.ThrowsException<MinefieldInvalidSizeException>(() => new Minefield(0, -1, 1));
        Assert.ThrowsException<MinefieldInvalidSizeException>(() => new Minefield(-1, 0, 1));

        var expected = String.Join(Environment.NewLine, new[]
        {
            "",
            "__01234567",
            "9 ########",
            "8 ########",
            "7 ########",
            "6 ########",
            "5 ########",
            "4 ########",
            "3 ########",
            "2 ########",
            "1 ########",
            "0 ########",
        });

        var mf = new Minefield(8, 10, 1);
        var actual = WithStringWriter(mf.Render);
        Assert.AreEqual(expected, actual);
    }

    public static IEnumerable<object[]> MoveTests
    {
        get
        {
            return new[] {
                new object[] {
                    new [] {Move.Flag(1, 1)},
                    new [] {(2,3),(1,2),(3,3)},
                    String.Join(Environment.NewLine, new [] {
                        "",
                        "__0123",
                        "3 ####",
                        "2 ####",
                        "1 #F##",
                        "0 ####"
                    }),
                },
                new object[] {
                  new [] {Move.Reveal(0,0)},
                  new [] {(2,3),(1,2),(3,3)},
                  String.Join(Environment.NewLine, new[]
                      {
                      "",
                      "__0123",
                      "3 ####",
                      "2 ##32",
                      "1 111.",
                      "0 ....",
                      }),
                },
                new object[] {
                  new [] {Move.Reveal(0,0), Move.Reveal(2,3)},
                  new [] {(2,3),(1,2),(3,3)},
                  String.Join(Environment.NewLine, new[]
                      {
                      "You Lost",
                      "__0123",
                      "3 ##XQ",
                      "2 #Q32",
                      "1 111.",
                      "0 ....",
                      }),
                },
            };
        }
    }
    [TestMethod, DynamicData(nameof(MoveTests))]
    public void MakeAMove(Move[] moves, (int, int)[] bombs, string expected)
    {
        var mf = new Minefield(4, 4, 0);
        foreach (var (x, y) in bombs)
        {
            mf.SetBomb(x, y);
        }

        foreach (var move in moves)
        {
            mf.MakeMove(move);
        }
        var actual = WithStringWriter(mf.Render);
        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    public void ValidateRuleConditions()
    {
        var expected = String.Join(Environment.NewLine, new[]
        {
            "You Lost",
            "__012",
            "2 ###",
            "1 #X#",
            "0 ###"
        });
        var mf = new Minefield(3, 3, 1);
        mf.SetBomb(1, 1);
        mf.MakeMove(Move.Reveal(1, 1));

        var actual = WithStringWriter(mf.Render);
        Assert.AreEqual(expected, actual);
    }
    private string WithStringWriter(Action<TextWriter> fn)
    {
        using (var sw = new StringWriter())
        {
            fn(sw);
            return sw.ToString();
        }
    }
}
