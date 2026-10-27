namespace Domino
{
    public interface IUserInterface
    {
        void Print(IMessage message);

        void PrintTile(ITile tile);

        void PrintTable(Table table);

        ITile SelectTile(IReadOnlyList<ITile> tiles, bool hidden = false);
    }

    public interface IMessage
    {
        string Text { get; }

        ConsoleColor Color { get; }
    }

    public abstract class MessageEnvelope : IMessage
    {
        protected MessageEnvelope(string text, ConsoleColor color)
        {
            Text = text;
            Color = color;
        }

        public string Text { get; }

        public ConsoleColor Color { get; }
    }

    public sealed class Info(string text) : MessageEnvelope(text, ConsoleColor.White);

    public sealed class Error(string text) : MessageEnvelope(text, ConsoleColor.Red);

    public sealed class ConsoleInterface : IUserInterface
    {
        private const int TileWidth = 10;

        public void Print(IMessage message)
        {
            Console.ForegroundColor = message.Color;
            Console.WriteLine(message.Text);
            Console.ResetColor();
        }

        public ITile SelectTile(IReadOnlyList<ITile> tiles, bool hidden = false)
        {
            var index = 0;
            Console.CursorVisible = false;

            var height = Draw(tiles, index, hidden);

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
                        Console.CursorVisible = true;
                        return tiles[index];
                    default:
                        continue;   // другие клавиши: не перерисовываем
                }

                // Возвращаемся к началу блока относительно текущего курсора, а не сохранённого номера строки
                Console.SetCursorPosition(0, Console.CursorTop - height);
                height = Draw(tiles, index, hidden);
            }
        }

        public void PrintTile(ITile tile)
        {
            Draw([tile], 0, false);
        }

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
                    Console.WriteLine();
                    lines++;
                }

                // Маркер только в ряду с выбранной костью; в остальных рядах строка из пробелов стирает старую ▲
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
            1 => $"│ ? │ ? │",
            _ => "└───┴───┘",
        };

        public void PrintTable(Table table)
        {
            var widths = table.Headers
                .Select((header, col) => table.Rows
                    .Select(r => r.Cells[col].Length)
                    .Append(header.Length)
                    .Max())
                .ToArray();

            Console.WriteLine();
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
    }
}

public sealed record TableRow(IReadOnlyList<string> Cells, bool Highlight = false);

public sealed record Table(string Title, IReadOnlyList<string> Headers, IReadOnlyList<TableRow> Rows);