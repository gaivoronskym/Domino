namespace Domino
{
    public sealed class Player
    {
        private readonly IList<ITile> _tiles;

        public Player(string name)
        {
            Name = name;
            _tiles = [];
        }

        public string Name { get; }

        public bool HasTiles => _tiles.Count > 0;

        public int TileWeight => _tiles.Sum(x => x.Top.Value + x.Bottom.Value);

        public int TileCount => _tiles.Count;

        // Очки, которые записываются игроку в конце раунда.
        // Если на руке остался только пустой дубль [0|0], он стоит 25 очков.
        public int PenaltyPoints =>
            _tiles.Count == 1 && _tiles[0].Top.Value == 0 && _tiles[0].Bottom.Value == 0
                ? 25
                : TileWeight;

        public bool HasDoubles => _tiles.Any(x => x.Top.Value == x.Bottom.Value);

        public ITile? FindDouble(int value)
        {
            return _tiles.FirstOrDefault(x => x.Top.Value == value && x.Bottom.Value == value);
        }

        public void AddTiles(IEnumerable<ITile> tiles)
        {
            foreach (var tile in tiles)
            {
                _tiles.Add(tile);
            }
        }

        public void Apply(IBoard board, ITile tile)
        {
            if (_tiles.Remove(tile))
            {
                board.Apply(tile);
            }
        }

        public bool HasApplicableTiles(IBoard board)
        {
            return _tiles.Any(board.Applicable);
        }

        public ITile SelectTile(IUserInterface ui)
        {
            return ui.SelectTile(_tiles.AsReadOnly(), false);
        }

        public bool TryApplyTile(IBoard board)
        {
            var hasApplicable = _tiles.Any(board.Applicable);

            if (hasApplicable)
            {
                var tile = _tiles.First(board.Applicable);
                _tiles.Remove(tile);
                board.Apply(tile);

                return true;
            }

            return false;
        }

        public bool CanPlay(IBoard board)
        {
            if(_tiles.Count == 0)
            {
                return false;
            }

            return HasApplicableTiles(board);
        }

        public void ClearTiles()
        {
            _tiles.Clear();
        }
    }
}
