namespace Domino.Tests
{
    public class BoardTests
    {
        [Fact]
        public void EmptyBoard_AcceptsAnyTile()
        {
            var board = new Board();

            Assert.False(board.Started);
            Assert.True(board.Applicable(new Tile(2, 6)));
        }

        [Fact]
        public void FirstTile_StartsBoard()
        {
            var board = new Board();
            var tile = new Tile(3, 5);

            board.Apply(tile);

            Assert.True(board.Started);
            Assert.Equal([tile], board.Tiles);
        }

        [Fact]
        public void Tiles_AreKeptInChainOrder()
        {
            var board = new Board();
            var start = new Tile(3, 5);
            var left = new Tile(5, 6);   // ложится к первой кости, становится левым концом
            var right = new Tile(3, 4);  // подходит только к правому концу
            var farLeft = new Tile(6, 1);

            board.Apply(start);
            board.Apply(left);
            board.Apply(right);
            board.Apply(farLeft);

            Assert.Equal([farLeft, left, start, right], board.Tiles);
        }

        [Fact]
        public void Applicable_ChecksOnlyOpenEnds()
        {
            var board = new Board();
            board.Apply(new Tile(3, 5));
            board.Apply(new Tile(5, 6));
            board.Apply(new Tile(3, 4));

            // Открытые концы: 6 и 4. Пятёрка и тройка уже внутри цепочки.
            Assert.True(board.Applicable(new Tile(6, 0)));
            Assert.True(board.Applicable(new Tile(0, 4)));
            Assert.False(board.Applicable(new Tile(5, 0)));
            Assert.False(board.Applicable(new Tile(3, 0)));
        }

        [Fact]
        public void Apply_NotMatchingTile_Throws()
        {
            var board = new Board();
            board.Apply(new Tile(1, 1));

            Assert.Throws<InvalidOperationException>(() => board.Apply(new Tile(2, 3)));
        }
    }
}
