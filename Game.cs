namespace Domino
{
    public sealed class Game
    {
        private readonly IList<ITile> _tiles;
        private readonly IList<Player> _players;
        private readonly IUserInterface _ui;

        private const int TilesPerPlayer = 7;
        private const int LosingScore = 101;

        public Game(IList<ITile> tiles, IList<Player> players, IUserInterface ui)
        {
            _tiles = tiles;
            _players = players;
            _ui = ui;
        }

        public void Play()
        {
            if (_players.Count < 2 || _players.Count > 4)
            {
                throw new InvalidOperationException("В игре должно быть от 2 до 4 игроков.");
            }

            var scores = _players.ToDictionary(p => p, _ => 0);
            Player? starter = null;
            var roundNumber = 1;

            while (scores.Values.All(x => x < LosingScore))
            {
                var result = PlayRound(starter);

                foreach (var (player, points) in result.Points)
                {
                    scores[player] += points;
                }

                _ui.PrintTable(RoundTable(roundNumber, result, scores));
                _ui.WaitForKey("Нажмите любую клавишу, чтобы продолжить...");

                starter = result.Winner;
                roundNumber++;
            }

            _ui.PrintTable(FinalTable(scores));
        }

        private RoundResult PlayRound(Player? starter)
        {
            var boneyard = Deal();

            // Первый раунд (или раунд после ничьей) начинается с дубля.
            // Если дублей ни у кого нет — пересдаём.
            while (starter is null && !_players.Any(p => p.HasDoubles))
            {
                boneyard = Deal();
            }

            var round = new Round(_players, _ui, new Board(), new Boneyard(boneyard));
            var result = round.Run(starter);

            foreach (var player in _players)
            {
                player.ClearTiles();
            }

            return result;
        }

        // Раздаёт по 7 костей и возвращает остаток для базара
        private List<ITile> Deal()
        {
            foreach (var player in _players)
            {
                player.ClearTiles();
            }

            // Каждый раунд — новые кости: отметки «приложена» с прошлого раунда не должны переходить
            var tiles = _tiles
                .Select(x => (ITile)new Tile(x.Top.Value, x.Bottom.Value))
                .ToArray();
            Random.Shared.Shuffle(tiles);

            for (var i = 0; i < _players.Count; i++)
            {
                _players[i].AddTiles(tiles.Skip(i * TilesPerPlayer).Take(TilesPerPlayer));
            }

            return tiles.Skip(_players.Count * TilesPerPlayer).ToList();
        }

        private Table RoundTable(int roundNumber, RoundResult result, IReadOnlyDictionary<Player, int> scores)
        {
            var title = result.Winner is null
                ? $"Раунд {roundNumber}. Рыба! Ничья, победителя нет"
                : result.Fish
                    ? $"Раунд {roundNumber}. Рыба! Победил {result.Winner.Name}"
                    : $"Раунд {roundNumber}. Домино! Победил {result.Winner.Name}";

            var rows = _players
                .OrderBy(p => scores[p])
                .Select(p => new TableRow(
                    [p.Name, result.Points[p].ToString(), scores[p].ToString()],
                    Highlight: p == result.Winner))
                .ToList();

            return new Table(title, ["Игрок", "За раунд", "Всего"], rows);
        }

        private Table FinalTable(IReadOnlyDictionary<Player, int> scores)
        {
            var loser = _players.MaxBy(p => scores[p])!;
            var best = scores.Values.Min();

            var rows = _players
                .OrderBy(p => scores[p])
                .Select((p, i) => new TableRow(
                    [(i + 1).ToString(), p.Name, scores[p].ToString()],
                    Highlight: scores[p] == best))
                .ToList();

            return new Table($"Игра окончена. Козёл — {loser.Name}", ["Место", "Игрок", "Очки"], rows);
        }
    }
}
