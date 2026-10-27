namespace Domino
{
    public sealed class Round
    {
        // Порядок, в котором ищется дубль для первого хода: [1|1], [2|2] … [6|6], затем [0|0]
        private static readonly int[] OpeningDoubles = [1, 2, 3, 4, 5, 6, 0];

        private readonly IList<Player> _players;
        private readonly IUserInterface _ui;
        private readonly IBoard _board;
        private readonly IBoneyard _boneyard;

        public Round(IList<Player> players, IUserInterface ui, IBoard board, IBoneyard boneyard)
        {
            _players = players;
            _ui = ui;
            _board = board;
            _boneyard = boneyard;
        }

        public RoundResult Run(Player? starter)
        {
            var (current, opening) = ChooseFirst(starter);

            while (true)
            {
                var player = _players[current];
                _ui.BeginTurn(player.Name, _board);

                var option = opening is null
                    ? MakeDesicion(player)
                    : new OpeningMove(_board, _ui, opening);
                option.Run(player);
                opening = null;

                if (!player.HasTiles || !CanPlay())
                {
                    break;
                }

                current = (current + 1) % _players.Count;
            }

            return CountPoints();
        }

        private (int Index, ITile? Opening) ChooseFirst(Player? starter)
        {
            if (starter is not null)
            {
                return (_players.IndexOf(starter), null);
            }

            foreach (var value in OpeningDoubles)
            {
                for (var i = 0; i < _players.Count; i++)
                {
                    var tile = _players[i].FindDouble(value);
                    if (tile is not null)
                    {
                        return (i, tile);
                    }
                }
            }

            throw new InvalidOperationException("Ни у одного игрока нет дубля.");
        }

        private RoundResult CountPoints()
        {
            var points = _players.ToDictionary(p => p, p => p.PenaltyPoints);
            var fish = _players.All(p => p.HasTiles);

            var winner = _players.FirstOrDefault(p => !p.HasTiles);
            if (fish)
            {
                var best = points.Values.Min();
                var leaders = _players.Where(p => points[p] == best).ToList();
                winner = leaders.Count == 1 ? leaders[0] : null;
            }

            if (winner is not null)
            {
                points[winner] = 0;
            }

            return new RoundResult(winner, fish, points);
        }

        private bool CanPlay()
        {
            return !_boneyard.Empty || _players.Any(p => p.CanPlay(_board));
        }

        private IGameOption MakeDesicion(Player player)
        {
            if (player.HasApplicableTiles(_board))
            {
                return new PlayerHasTile(_board, _ui);
            }
            else
            {
                return new PlayerHasNoTile(_board, _ui, _boneyard);
            }
        }
    }
}
