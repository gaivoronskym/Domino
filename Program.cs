namespace Domino
{
    public interface ITile
    {
        ITileValue Top { get; }

        ITileValue Bottom { get; }

        bool Applied { get; }

        bool Applicable(ITile tile);

        void Apply(ITile tile);
    }

    public interface ITileValue
    {
        int Value { get; }

        bool Applied { get; }

        bool Applicable(ITileValue value);

        void Apply();
    }

    public sealed class Tile : ITile
    {
        public Tile(int top,  int bottom)
            : this(new TileValue(top), new TileValue(bottom))
        {
        }

        public Tile(ITileValue top, ITileValue bottom)
        {
            Top = top;
            Bottom = bottom;
        }

        public ITileValue Top { get; }

        public ITileValue Bottom { get; }

        public bool Applied => Top.Applied && Bottom.Applied;

        public bool Applicable(ITile tile)
        {
            if(tile.Top.Applicable(Top) || tile.Bottom.Applicable(Bottom))
            {
                return true;
            }

            if (tile.Top.Applicable(Bottom) || tile.Bottom.Applicable(Top))
            {
                return true;
            }

            return false;
        }

        public void Apply(ITile tile)
        {
            if (Top.Applicable(tile.Top))
            {
                Top.Apply();
                tile.Top.Apply();
            }

            if (Top.Applicable(tile.Bottom))
            {
                Top.Apply();
                tile.Bottom.Apply();
            }

            if (Bottom.Applicable(tile.Top))
            {
                Bottom.Apply();
                tile.Top.Apply();
            }

            if (Bottom.Applicable(tile.Bottom))
            {
                Bottom.Apply();
                tile.Bottom.Apply();
            }
        }
    }

    public sealed class TileValue : ITileValue
    {
        private bool _applied;

        public TileValue(int value)
        {
            Value = value;
            _applied = false;
        }

        public int Value { get; }

        public bool Applied => _applied;

        public bool Applicable(ITileValue value)
        {
            return !_applied && !value.Applied && Value == value.Value;
        }

        public void Apply()
        {
            _applied = true;
        }
    }

    public interface IBoneyard
    {
        void AddTiles(IEnumerable<ITile> tiles);

        ITile Pop();
    }

    public interface IBoard
    {
        bool Started { get; }

        ITile TopEnd { get; }

        ITile BottomEnd { get; }

        void Start(ITile tile);

        void Apply(ITile tile);

        bool Applicable(ITile tile);
    }

    public sealed class Board
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

        public ITile TopEnd => _tiles.First(x => !x.Applied);

        public ITile BottomEnd => _tiles.Last(x => !x.Applied);

        public void Start(ITile tile)
        {
            _tiles.Add(tile);
            _started = true;
        }

        public void Apply(ITile tile)
        {
            if (!_started)
            {
                throw new InvalidOperationException();
            }
            
            var end = _tiles.First(x => x.Applicable(tile));
            end.Apply(tile);
            _tiles.Insert(0, tile);
        }

        public bool Applicable(ITile tile)
        {
            return TopEnd.Applicable(tile) || BottomEnd.Applicable(tile);
        }
    }

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

        public void AddTiles(IEnumerable<ITile> tiles)
        {
            foreach (var tile in tiles)
            {
                _tiles.Add(tile);
            }
        }

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
    }

    public sealed class Player
    {
        private readonly IList<ITile> _tiles;

        public Player(string name)
        {
            Name = name;
            _tiles = [];
        }

        public string Name { get; }

        public void AddTiles(IEnumerable<ITile> tiles)
        {
            foreach (var tile in tiles)
            {
                _tiles.Add(tile);
            }
        }

        public bool TryApplyTile(Board board)
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
    }

    public sealed class Game
    {
        private readonly IList<ITile> _tiles;
        private readonly IList<Player> _players;
        private readonly IUserInterface _ui;
        private readonly IBoard _board;
        private readonly IBoneyard _boneyard;

        private const int TilesPerPlayer = 7;

        public Game(IList<ITile> tiles, IList<Player> players, IBoard board, IBoneyard boneyard, IUserInterface ui)
        {
            _tiles = tiles;
            _players = players;
            _ui = ui;
            _board = board;
            _boneyard = boneyard;
        }

        public void ShaffleTiles()
        {
            if(_players.Count > 4)
            {
                throw new InvalidOperationException();
            }

            Random.Shared.Shuffle(_tiles.ToArray());

            var page = 0;

            foreach (var player in _players)
            {
                player.AddTiles(_tiles.Skip(page * TilesPerPlayer).Take(TilesPerPlayer));
            }

            _boneyard.AddTiles(_tiles.Skip(page * TilesPerPlayer));
        }

        public void Play()
        {
            var currentPlayer = 0;

            while (true)
            {
                var player = _players[currentPlayer];
                _ui.Print($"Ход игрока {player.Name}");
            }
        }
    }

    public interface IUserInterface
    {
        void Print(string message);
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            IEnumerable<ITile> tiles = [
                new Tile(0, 0),
                new Tile(0, 1),
                new Tile(0, 2),
                new Tile(0, 3),
                new Tile(0, 4),
                new Tile(0, 5),
                new Tile(0, 6),

                new Tile(1, 1),
                new Tile(1, 2),
                new Tile(1, 3),
                new Tile(1, 4),
                new Tile(1, 5),
                new Tile(1, 6),

                new Tile(2, 2),
                new Tile(2, 3),
                new Tile(2, 4),
                new Tile(2, 5),
                new Tile(2, 6),

                new Tile(3, 3),
                new Tile(3, 4),
                new Tile(3, 5),
                new Tile(3, 6),

                new Tile(4, 4),
                new Tile(4, 5),
                new Tile(4, 6),

                new Tile(5, 5),
                new Tile(5, 6),

                new Tile(6, 6)
            ];
        }
    }
}