namespace Domino
{
    public sealed class ConsoleInterface : IUserInterface
    {
        private const int TileWidth = 10;       // "┌───┬───┐" + пробел
        private const int ShortTileWidth = 5;   // "[2|5]"

        private IBoard? _board;
        private string _header = "";
        private readonly List<IMessage> _messages = [];
        private int _lastHeight;                // сколько строк занял прошлый кадр

        public void BeginTurn(string playerName, IBoard board)
        {
            _board = board;
            _header = $"Ход игрока {playerName}";
            _messages.Clear();

            Console.Clear();
            _lastHeight = 0;
            Render();
        }

        public void Print(IMessage message)
        {
            _messages.Add(message);
            Render();
        }

        public void PrintTile(ITile tile)
        {
            _messages.Add(new Info($"Из кучи: [{tile.Top.Value}|{tile.Bottom.Value}]"));
            Render();
        }

        public ITile SelectTile(IReadOnlyList<ITile> tiles, bool hidden = false)
        {
            var index = 0;
            Render(tiles, index, hidden);

            while (true)
            {
                switch (Console.ReadKey(intercept: true).Key)
                {
                    case ConsoleKey.LeftArrow:
                        index = (index - 1 + tiles.Count) % tiles.Count;
                        break;
                    case ConsoleKey.RightArrow:
                        index = (index + 1) % tiles.Count;
                        break;
                    case ConsoleKey.Enter:
                        return tiles[index];
                    default:
                        continue;   // другие клавиши: не перерисовываем
                }

                Render(tiles, index, hidden);
            }
        }

        public void WaitForKey(string prompt)
        {
            Console.WriteLine();
            Console.WriteLine(prompt);
            Console.ReadKey(intercept: true);
        }

        public void PrintTable(Table table)
        {
            Console.Clear();
            _lastHeight = 0;
            WriteBoard();
            WriteLine("");

            var widths = table.Headers
                .Select((header, col) => table.Rows
                    .Select(r => r.Cells[col].Length)
                    .Append(header.Length)
                    .Max())
                .ToArray();

            Console.WriteLine(table.Title);
            Console.WriteLine(Border('┌', '┬', '┐'));
            Console.WriteLine(Line(table.Headers));
            Console.WriteLine(Border('├', '┼', '┤'));

            foreach (var row in table.Rows)
            {
                if (row.Highlight)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                }

                Console.WriteLine(Line(row.Cells));
                Console.ResetColor();
            }

            Console.WriteLine(Border('└', '┴', '┘'));

            string Border(char left, char middle, char right) =>
                left + string.Join(middle, widths.Select(w => new string('─', w + 2))) + right;

            string Line(IReadOnlyList<string> cells) =>
                "│" + string.Join("│", cells.Select((cell, i) => $" {Align(cell, widths[i])} ")) + "│";

            static string Align(string cell, int width) =>
                int.TryParse(cell, out _) ? cell.PadLeft(width) : cell.PadRight(width);
        }

        // Весь экран целиком, с позиции (0,0): доска, заголовок, сообщения, селектор
        private void Render(IReadOnlyList<ITile>? hand = null, int selected = -1, bool hidden = false)
        {
            Console.CursorVisible = false;
            Console.SetCursorPosition(0, 0);

            var lines = WriteBoard();
            lines += WriteLine("");
            lines += WriteLine(_header, ConsoleColor.Cyan);

            foreach (var message in _messages)
            {
                lines += WriteLine(message.Text, message.Color);
            }

            if (hand is not null)
            {
                lines += Draw(hand, selected, hidden);
            }

            for (var i = lines; i < _lastHeight; i++)
            {
                WriteLine("");
            }

            _lastHeight = lines;
            Console.CursorVisible = true;
        }

        private int WriteBoard()
        {
            IReadOnlyList<TileFace> chain = _board is null ? [] : BoardLayout.Chain(_board.Tiles);
            var lines = WriteLine("Доска:");

            if (chain.Count == 0)
            {
                return lines + WriteLine("(пусто)", ConsoleColor.DarkGray);
            }

            var perRow = Math.Max(1, (Console.WindowWidth - 1) / ShortTileWidth);
            for (var start = 0; start < chain.Count; start += perRow)
            {
                var row = string.Concat(chain
                    .Skip(start)
                    .Take(perRow)
                    .Select(f => $"[{f.Left}|{f.Right}]"));

                lines += WriteLine(row);
            }

            return lines;
        }

        // Строка, дополненная пробелами до ширины окна: затирает то, что было на этом месте в прошлом кадре
        private static int WriteLine(string text, ConsoleColor color = ConsoleColor.Gray)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(text.PadRight(Console.WindowWidth - 1));
            Console.ResetColor();
            return 1;
        }

        // Ряды косточек с рамкой; возвращает число напечатанных строк
        private static int Draw(IReadOnlyList<ITile> tiles, int selected, bool hidden)
        {
            Func<ITile, int, string> tileLine = hidden ? HiddenTileLine : TileLine;

            // -1, чтобы строка не доходила до правого края и консоль не переносила её сама
            var perRow = Math.Max(1, (Console.WindowWidth - 1) / TileWidth);
            var lines = 0;

            for (var start = 0; start < tiles.Count; start += perRow)
            {
                var end = Math.Min(start + perRow, tiles.Count);

                for (var line = 0; line < 3; line++)
                {
                    for (var i = start; i < end; i++)
                    {
                        if (i == selected)
                        {
                            Console.BackgroundColor = ConsoleColor.Yellow;
                            Console.ForegroundColor = ConsoleColor.Black;
                        }

                        Console.Write(tileLine(tiles[i], line));
                        Console.ResetColor();
                        Console.Write(' ');
                    }

                    // Неполный ряд добиваем пробелами, чтобы стереть рамки прошлого кадра
                    Console.Write(new string(' ', (perRow - (end - start)) * TileWidth));
                    Console.WriteLine();
                    lines++;
                }

                // Маркер только в ряду с выбранной костью; в остальных рядах строка из пробелов
                var marker = selected >= start && selected < end
                    ? new string(' ', (selected - start) * TileWidth) + "    ▲"
                    : "";
                Console.WriteLine(marker.PadRight(perRow * TileWidth));
                lines++;
            }

            return lines;
        }

        private static string TileLine(ITile tile, int line) => line switch
        {
            0 => "┌───┬───┐",
            1 => $"│ {tile.Top.Value} │ {tile.Bottom.Value} │",
            _ => "└───┴───┘",
        };

        private static string HiddenTileLine(ITile tile, int line) => line switch
        {
            0 => "┌───┬───┐",
            1 => "│ ? │ ? │",
            _ => "└───┴───┘",
        };
    }
}
