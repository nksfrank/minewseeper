
var rnd = new Random();
var header = "[Minefield] ('q' to quit)";
var game = new Minefield.Minefield(5, 5, 3);
Console.Write("bomb at: ");
foreach (var b in Enumerable.Range(0, 3).Select(_ => rnd.Next(0, 5 * 5)))
{
    var x = b % 5;
    var y = b / 5;
    game.SetBomb(x, y);
    Console.Write($"{x} {y},");
}

while (game.State == Minefield.GameState.Runnig)
{
    Console.Clear();
    Console.WriteLine(header);
    game.Render(Console.Out);
    int x, y;
    while (true)
    {
        Console.Write("Input move [x y]: ");
        var input = Console.ReadLine();
        var parts = input?.Split(" ");
        if (parts?.Length == 2 && int.TryParse(parts[0], out x) && int.TryParse(parts[1], out y))
        {
            break;
        }
        else
        {
            Console.WriteLine("Invalid input. Please enter two numbers separated by a space.");
        }
    }

    game.MakeMove(Minefield.Move.Reveal(x, y));
}
Console.Clear();
game.Render(Console.Out);
