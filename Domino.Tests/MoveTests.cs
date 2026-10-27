namespace Domino.Tests
{
    public class MoveTests
    {
        private static Player WithTiles(params ITile[] tiles)
        {
            var player = new Player("Ваня");
            player.AddTiles(tiles);
            return player;
        }

        private static Board BoardWith(ITile tile)
        {
            var board = new Board();
            board.Apply(tile);
            return board;
        }

        [Fact]
        public void PlayerHasTile_WrongChoice_AsksAgain()
        {
            var ui = new FakeUserInterface();
            var board = BoardWith(new Tile(1, 1));
            var wrong = new Tile(5, 6);
            var right = new Tile(1, 2);
            var player = WithTiles(wrong, right);

            new PlayerHasTile(board, ui).Run(player);

            Assert.Single(ui.Errors);
            Assert.Contains(right, board.Tiles);
            Assert.Equal(1, player.TileCount);
        }

        [Fact]
        public void OpeningMove_RequiresGivenDouble()
        {
            var ui = new FakeUserInterface();
            var board = new Board();
            var other = new Tile(2, 5);
            var opening = new Tile(1, 1);
            var player = WithTiles(other, opening);

            new OpeningMove(board, ui, opening).Run(player);

            Assert.Single(ui.Errors);
            Assert.Equal([opening], board.Tiles);
            Assert.Equal(1, player.TileCount);
        }

        [Fact]
        public void PlayerHasNoTile_EmptyBoneyard_SkipsTurn()
        {
            var ui = new FakeUserInterface();
            var board = BoardWith(new Tile(1, 1));
            var player = WithTiles(new Tile(5, 6));

            new PlayerHasNoTile(board, ui, new Boneyard()).Run(player);

            Assert.Single(ui.Errors);
            Assert.Equal(1, player.TileCount);
            Assert.Single(board.Tiles);
        }

        [Fact]
        public void PlayerHasNoTile_DrawsUntilApplicable_AndPlaysIt()
        {
            var ui = new FakeUserInterface();
            var board = BoardWith(new Tile(1, 1));
            var player = WithTiles(new Tile(5, 6));
            var notFitting = new Tile(4, 4);
            var fitting = new Tile(1, 3);
            var yard = new List<ITile> { notFitting, fitting };

            new PlayerHasNoTile(board, ui, new Boneyard(yard)).Run(player);

            Assert.Empty(yard);
            Assert.Equal(2, player.TileCount);             // [5|6] и добранная [4|4]
            Assert.Equal(2, board.Tiles.Count);            // [1|1] и [1|3]
            Assert.Contains(fitting, board.Tiles);
            Assert.Contains(fitting, ui.ShownTiles);       // игрок видит, что легло на доску
        }

        [Fact]
        public void PlayerHasNoTile_FirstDrawFits_PlaysIt()
        {
            var ui = new FakeUserInterface();
            var board = BoardWith(new Tile(1, 1));
            var player = WithTiles(new Tile(5, 6));
            var fitting = new Tile(1, 3);

            new PlayerHasNoTile(board, ui, new Boneyard([fitting])).Run(player);

            Assert.Equal(1, player.TileCount);
            Assert.Contains(fitting, board.Tiles);
        }

        [Fact]
        public void PlayerHasNoTile_BoneyardRunsOut_KeepsAllDrawnTiles()
        {
            var ui = new FakeUserInterface();
            var board = BoardWith(new Tile(1, 1));
            var player = WithTiles(new Tile(5, 6));
            var yard = new List<ITile> { new Tile(4, 4), new Tile(2, 3) };

            new PlayerHasNoTile(board, ui, new Boneyard(yard)).Run(player);

            Assert.Empty(yard);
            Assert.Equal(3, player.TileCount);
            Assert.Single(board.Tiles);
        }
    }
}
