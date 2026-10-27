namespace Domino
{
    public sealed class PlayerHasTile : IGameOption
    {
        private readonly IBoard _board;
        private readonly IUserInterface _ui;

        public PlayerHasTile(IBoard board, IUserInterface ui)
        {
            _board = board;
            _ui = ui;
        }

        public void Run(Player player)
        {
            var tile = SelectTile(player);

            player.Apply(_board, tile);
        }

        private ITile SelectTile(Player player)
        {
            var tile = player.SelectTile(_ui);
            while (!_board.Applicable(tile))
            {
                _ui.Print(new Error("Выбранная кость не подходит. Попробуйте ещё раз."));
                tile = player.SelectTile(_ui);
            }

            return tile;
        }
    }
}
