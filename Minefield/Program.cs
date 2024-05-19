
var header = "[Minefield] ('q' to quit)";

Func<Minefield.Minefield> init = () =>
{
    var rnd = new Random();
    var bombs = Enumerable.Range(0, 3).Select(_ => (rnd.Next(0, 5), rnd.Next(0, 5)));
    return new Minefield.Minefield(5, 5, bombs);
};

var game = init();
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

    game.MakeMove(new Minefield.Reveal((x, y)));
}
Console.Clear();
game.Render(Console.Out);
