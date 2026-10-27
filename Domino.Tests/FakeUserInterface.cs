namespace Domino.Tests
{
    // Интерфейс без консоли: запоминает, что игра показала, и выбирает кости по кругу.
    // Выбор по кругу гарантирует, что повторный выбор после ошибки рано или поздно дойдёт до нужной кости.
    public sealed class FakeUserInterface : IUserInterface
    {
        private int _counter;

        public List<string> Turns { get; } = [];

        public List<IMessage> Messages { get; } = [];

        public List<ITile> ShownTiles { get; } = [];

        public List<Table> Tables { get; } = [];

        public List<bool> SelectHidden { get; } = [];

        public IEnumerable<IMessage> Errors => Messages.OfType<Error>();

        public void BeginTurn(string playerName, IBoard board) => Turns.Add(playerName);

        public void Print(IMessage message) => Messages.Add(message);

        public void PrintTile(ITile tile) => ShownTiles.Add(tile);

        public void PrintTable(Table table) => Tables.Add(table);

        public void WaitForKey(string prompt)
        {
        }

        public ITile SelectTile(IReadOnlyList<ITile> tiles, bool hidden = false)
        {
            SelectHidden.Add(hidden);
            return tiles[_counter++ % tiles.Count];
        }
    }
}
