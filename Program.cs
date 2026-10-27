using System.Text;

namespace Domino
{
    internal class Program
    {
        static void Main(string[] args)
        {
            IEnumerable<ITile> tiles = [
                new Tile(0, 0),
                new Tile(0, 1),
                new Tile(0, 2),
                new Tile(0, 3),
                new Tile(0, 4),
                new Tile(0, 5),
                new Tile(0, 6),

                new Tile(1, 1),
                new Tile(1, 2),
                new Tile(1, 3),
                new Tile(1, 4),
                new Tile(1, 5),
                new Tile(1, 6),

                new Tile(2, 2),
                new Tile(2, 3),
                new Tile(2, 4),
                new Tile(2, 5),
                new Tile(2, 6),

                new Tile(3, 3),
                new Tile(3, 4),
                new Tile(3, 5),
                new Tile(3, 6),

                new Tile(4, 4),
                new Tile(4, 5),
                new Tile(4, 6),

                new Tile(5, 5),
                new Tile(5, 6),

                new Tile(6, 6)
            ];

            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;

            var game = new Game(
                tiles.ToList(),
                [new Player("Ваня"), new Player("Миша")],
                new ConsoleInterface()
            );

            game.Play();
        }
    }
}
