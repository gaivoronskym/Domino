namespace Domino
{
    public sealed class Boneyard : IBoneyard
    {
        private readonly IList<ITile> _tiles;

        public Boneyard()
            : this([])
        {
        }

        public Boneyard(IList<ITile> tiles)
        {
            _tiles = tiles;
        }

        public bool Has => _tiles.Count > 0;

        public bool Empty => _tiles.Count == 0;

        public ITile Pop()
        {
            if (!Has)
            {
                throw new InvalidOperationException();
            }

            var tile = _tiles[Random.Shared.Next(_tiles.Count)];

            _tiles.Remove(tile);

            return tile;
        }

        public IReadOnlyList<ITile> PopAll()
        {
            var tiles = _tiles.AsReadOnly();
            _tiles.Clear();
            return tiles;
        }

        public ITile SelectTile(IUserInterface ui)
        {
            var tile = ui.SelectTile(_tiles.AsReadOnly(), true);
            _tiles.Remove(tile);
            return tile;
        }
    }
}
