namespace Domino.Tests
{
    public class BoardLayoutTests
    {
        [Fact]
        public void EmptyBoard_GivesEmptyChain()
        {
            Assert.Empty(BoardLayout.Chain([]));
        }

        [Fact]
        public void SingleTile_IsShownAsIs()
        {
            var board = new Board();
            board.Apply(new Tile(3, 5));

            Assert.Equal([new TileFace(3, 5)], BoardLayout.Chain(board.Tiles));
        }

        [Fact]
        public void Chain_TurnsTilesSoNeighboursMatch()
        {
            var board = new Board();
            board.Apply(new Tile(3, 5));
            board.Apply(new Tile(5, 6));
            board.Apply(new Tile(3, 4));
            board.Apply(new Tile(1, 6));

            Assert.Equal(
                [new TileFace(1, 6), new TileFace(6, 5), new TileFace(5, 3), new TileFace(3, 4)],
                BoardLayout.Chain(board.Tiles));
        }

        [Fact]
        public void Chain_WithDoubles_StaysConnected()
        {
            var board = new Board();
            board.Apply(new Tile(2, 2));
            board.Apply(new Tile(2, 5));
            board.Apply(new Tile(5, 5));
            board.Apply(new Tile(2, 0));

            var chain = BoardLayout.Chain(board.Tiles);

            for (var i = 1; i < chain.Count; i++)
            {
                Assert.Equal(chain[i - 1].Right, chain[i].Left);
            }
        }
    }
}
