namespace Domino.Tests
{
    public class PlayerTests
    {
        private static Player WithTiles(params ITile[] tiles)
        {
            var player = new Player("Ваня");
            player.AddTiles(tiles);
            return player;
        }

        [Fact]
        public void TileWeight_IsSumOfAllHalves()
        {
            Assert.Equal(15, WithTiles(new Tile(1, 2), new Tile(6, 6)).TileWeight);
        }

        [Fact]
        public void PenaltyPoints_AreTileWeight()
        {
            Assert.Equal(15, WithTiles(new Tile(1, 2), new Tile(6, 6)).PenaltyPoints);
        }

        [Fact]
        public void PenaltyPoints_ForLoneEmptyDouble_Are25()
        {
            Assert.Equal(25, WithTiles(new Tile(0, 0)).PenaltyPoints);
        }

        [Fact]
        public void PenaltyPoints_ForEmptyDoubleWithOtherTiles_AreTileWeight()
        {
            Assert.Equal(3, WithTiles(new Tile(0, 0), new Tile(1, 2)).PenaltyPoints);
        }

        [Fact]
        public void FindDouble_ReturnsDoubleWithValue()
        {
            var double3 = new Tile(3, 3);
            var player = WithTiles(new Tile(3, 4), double3);

            Assert.Same(double3, player.FindDouble(3));
            Assert.Null(player.FindDouble(4));
        }

        [Fact]
        public void HasDoubles_DetectsAnyDouble()
        {
            Assert.True(WithTiles(new Tile(1, 2), new Tile(0, 0)).HasDoubles);
            Assert.False(WithTiles(new Tile(1, 2), new Tile(3, 4)).HasDoubles);
        }

        [Fact]
        public void Apply_MovesTileFromHandToBoard()
        {
            var tile = new Tile(1, 2);
            var player = WithTiles(tile, new Tile(3, 4));
            var board = new Board();

            player.Apply(board, tile);

            Assert.Equal(1, player.TileCount);
            Assert.Equal([tile], board.Tiles);
        }

        [Fact]
        public void Apply_TileNotInHand_DoesNothing()
        {
            var player = WithTiles(new Tile(3, 4));
            var board = new Board();

            player.Apply(board, new Tile(1, 2));

            Assert.Equal(1, player.TileCount);
            Assert.Empty(board.Tiles);
        }

        [Fact]
        public void CanPlay_RequiresApplicableTile()
        {
            var board = new Board();
            board.Apply(new Tile(1, 1));

            Assert.True(WithTiles(new Tile(1, 5)).CanPlay(board));
            Assert.False(WithTiles(new Tile(2, 5)).CanPlay(board));
            Assert.False(new Player("Пустой").CanPlay(board));
        }

        [Fact]
        public void ClearTiles_EmptiesHand()
        {
            var player = WithTiles(new Tile(1, 2));

            player.ClearTiles();

            Assert.False(player.HasTiles);
        }
    }
}
