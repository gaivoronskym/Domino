using System.Numerics;
using System.Text;

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

        public override string ToString()
        {
            return $"[{Top.Value}|{Bottom.Value}]";
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
        bool Empty { get; }

        ITile Pop();

        ITile SelectTile(IUserInterface ui);
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
                Start(tile);
                return;
            }
            
            var end = _tiles.First(x => x.Applicable(tile));
            end.Apply(tile);
            _tiles.Insert(0, tile);
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

        public ITile SelectTile(IUserInterface ui)
        {
            var tile = ui.SelectTile(_tiles.AsReadOnly(), true);
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

        public bool HasTiles => _tiles.Count > 0;

        public int TileWeight => _tiles.Sum(x => x.Top.Value + x.Bottom.Value);

        public int TileCount => _tiles.Count;

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

    public interface IGameOption
    {
        void Run(Player player);
    }

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

            if (!_board.Applicable(tile))
            {
                while (!_board.Applicable(tile))
                {
                    if (_boneyard.Empty)
                    {
                        _ui.Print(new Error($"Куча пуста. Игрок {player.Name} пропускает ход."));
                        return;
                    }

                    _ui.PrintTile(tile);

                    player.AddTiles([tile]);
                    _ui.Print(new Error("Выбранная кость не подходит. Попробуйте ещё раз."));

                    tile = _boneyard.SelectTile(_ui);
                }
            }
            else
            {
                _ui.PrintTile(tile);
                _board.Apply(tile);
            }

        }
    }

    public sealed class Round
    {
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

        public void Run()
        {
            while (true)
            {
                var winners = _players.Where(x => !x.HasTiles).Select(x => x.Name).ToList();
                if (winners.Count > 0)
                {
                    _ui.Print(new Info($"Победители раунда: {string.Join(',', winners)}"));
                }

                if (CanPlay())
                {
                    foreach (var player in _players.Where(x => x.HasTiles))
                    {
                        _ui.Print(new Info($"Ход игрока {player.Name}"));

                        var option = MakeDesicion(player);
                        option.Run(player);
                    }
                }
                else
                {
                    var bestWeight = _players.Min(p => p.TileWeight);

                    var rows = _players
                        .OrderBy(p => p.TileWeight)
                        .Select((p, i) => new TableRow(
                            [(i + 1).ToString(), p.Name, p.TileCount.ToString(), p.TileWeight.ToString()],
                            Highlight: p.TileWeight == bestWeight))
                        .ToList();

                    _ui.PrintTable(new Table(
                        "Раунд окончен. Результаты",
                        ["Место", "Игрок", "Костей", "Очки"],
                        rows));

                    return;
                }
            }
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

    public sealed class Game
    {
        private readonly IList<ITile> _tiles;
        private readonly IList<Player> _players;
        private readonly IUserInterface _ui;

        private const int TilesPerPlayer = 7;

        public Game(IList<ITile> tiles, IList<Player> players, IUserInterface ui)
        {
            _tiles = tiles;
            _players = players;
            _ui = ui;
        }

        public void Play()
        {
            if (_players.Count > 4)
            {
                throw new InvalidOperationException();
            }

            var tiles = _tiles.ToArray();
            Random.Shared.Shuffle(tiles);

            var page = 0;

            foreach (var player in _players)
            {
                player.AddTiles(tiles.Skip(page * TilesPerPlayer).Take(TilesPerPlayer));
                page++;
            }

            var round = new Round(_players, _ui, new Board(), new Boneyard(tiles.Skip(page * TilesPerPlayer).ToList()));

            round.Run();

            foreach (var player in _players)
            {
                player.ClearTiles();
            }
        }
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

            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;

            var game = new Game(
                tiles.ToList(),
                [new Player("Ваня"), new Player("Миша")],
                new ConsoleInterface()
            );

            game.Play();
        }
    }
}