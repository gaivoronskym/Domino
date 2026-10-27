namespace Domino
{
    public sealed class PlayerHasNoTile : IGameOption
    {
        private readonly IBoard _board;
        private readonly IUserInterface _ui;
        private readonly IBoneyard _boneyard;

        public PlayerHasNoTile(IBoard board, IUserInterface ui, IBoneyard boneyard)
        {
            _board = board;
            _ui = ui;
            _boneyard = boneyard;
        }

        public void Run(Player player)
        {
            if (_boneyard.Empty)
            {
                _ui.Print(new Error($"Куча пуста. Игрок {player.Name} пропускает ход."));
                return;
            }

            _ui.Print(new Error($"У игрока {player.Name} нет подходящих костей. Необходимо взять из кучи..."));

            var tile = _boneyard.SelectTile(_ui);
            while (!_board.Applicable(tile))
            {
                _ui.PrintTile(tile);

                player.AddTiles([tile]);

                if (_boneyard.Empty)
                {
                    _ui.Print(new Error($"Куча пуста. Игрок {player.Name} пропускает ход."));
                    return;
                }

                _ui.Print(new Error("Выбранная кость не подходит. Попробуйте ещё раз."));

                tile = _boneyard.SelectTile(_ui);
            }

            _ui.PrintTile(tile);
            _board.Apply(tile);
        }
    }
}
