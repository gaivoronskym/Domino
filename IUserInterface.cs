namespace Domino
{
    public interface IUserInterface
    {
        // Новый ход: очистить экран, показать доску и чей ход
        void BeginTurn(string playerName, IBoard board);

        void Print(IMessage message);

        void PrintTile(ITile tile);

        void PrintTable(Table table);

        // Пауза, чтобы игрок успел прочитать экран
        void WaitForKey(string prompt);

        ITile SelectTile(IReadOnlyList<ITile> tiles, bool hidden = false);
    }
}
