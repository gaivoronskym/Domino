namespace Domino
{
    public sealed class OpeningMove : IGameOption
    {
        private readonly IBoard _board;
        private readonly IUserInterface _ui;
        private readonly ITile _opening;

        public OpeningMove(IBoard board, IUserInterface ui, ITile opening)
        {
            _board = board;
            _ui = ui;
            _opening = opening;
        }

        public void Run(Player player)
        {
            _ui.Print(new Info($"Первый ход: выложите дубль {_opening}"));

            var tile = player.SelectTile(_ui);
            while (tile != _opening)
            {
                _ui.Print(new Error($"Первым ходом нужно выложить {_opening}. Попробуйте ещё раз."));
                tile = player.SelectTile(_ui);
            }

            player.Apply(_board, tile);
        }
    }
}
