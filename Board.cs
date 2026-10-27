namespace Domino
{
    public sealed class Board : IBoard
    {
        private readonly IList<ITile> _tiles;
        private bool _started;

        public Board()
            : this([])
        {
        }

        public Board(IList<ITile> tiles)
        {
            _tiles = tiles;
        }

        public bool Started => _started;

        public IReadOnlyList<ITile> Tiles => _tiles.AsReadOnly();

        public ITile TopEnd => _tiles.First(x => !x.Applied);

        public ITile BottomEnd => _tiles.Last(x => !x.Applied);

        public void Start(ITile tile)
        {
            _tiles.Add(tile);
            _started = true;
        }

        public void Apply(ITile tile)
        {
            if (!_started) { Start(tile); return; }

            if (TopEnd.Applicable(tile))
            {
                TopEnd.Apply(tile);
                _tiles.Insert(0, tile);
            }
            else if (BottomEnd.Applicable(tile))
            {
                BottomEnd.Apply(tile);
                _tiles.Add(tile);
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        public bool Applicable(ITile tile)
        {
            if (!_started)
            {
                return true;
            }

            return TopEnd.Applicable(tile) || BottomEnd.Applicable(tile);
        }
    }
}
