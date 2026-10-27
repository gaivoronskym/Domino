namespace Domino.Tests
{
    public class RoundTests
    {
        private static Player WithTiles(string name, params ITile[] tiles)
        {
            var player = new Player(name);
            player.AddTiles(tiles);
            return player;
        }

        private static (RoundResult Result, Board Board, FakeUserInterface Ui) Play(
            Player? starter, params Player[] players)
        {
            var ui = new FakeUserInterface();
            var board = new Board();
            var result = new Round(players, ui, board, new Boneyard()).Run(starter);
            return (result, board, ui);
        }

        [Fact]
        public void FirstRound_StartsWithSmallestDouble()
        {
            var opening = new Tile(1, 1);
            var a = WithTiles("А", new Tile(2, 2), new Tile(5, 6));
            var b = WithTiles("Б", opening, new Tile(4, 6));

            var (_, board, ui) = Play(null, a, b);

            Assert.Equal("Б", ui.Turns[0]);
            Assert.Same(opening, board.Tiles[0]);
        }

        [Fact]
        public void EmptyDouble_IsCheckedLast()
        {
            var opening = new Tile(3, 3);
            var a = WithTiles("А", new Tile(0, 0), new Tile(5, 6));
            var b = WithTiles("Б", opening, new Tile(4, 6));

            var (_, board, ui) = Play(null, a, b);

            Assert.Equal("Б", ui.Turns[0]);
            Assert.Same(opening, board.Tiles[0]);
        }

        [Fact]
        public void NoDoubles_WithoutStarter_Throws()
        {
            var a = WithTiles("А", new Tile(1, 2));
            var b = WithTiles("Б", new Tile(3, 4));

            Assert.Throws<InvalidOperationException>(() => Play(null, a, b));
        }

        [Fact]
        public void Starter_MovesFirst_WithAnyTile()
        {
            var a = WithTiles("А", new Tile(1, 1));
            var b = WithTiles("Б", new Tile(2, 3), new Tile(6, 6));

            var (_, board, ui) = Play(b, a, b);

            Assert.Equal("Б", ui.Turns[0]);
            Assert.Equal(new Tile(2, 3).ToString(), board.Tiles[0].ToString());
        }

        [Fact]
        public void Domino_WinnerGetsZero_OthersGetTheirPoints()
        {
            var a = WithTiles("А", new Tile(1, 1), new Tile(1, 2));
            var b = WithTiles("Б", new Tile(2, 2), new Tile(6, 6));

            var (result, _, ui) = Play(null, a, b);

            Assert.Equal(["А", "Б", "А"], ui.Turns);       // А открыл, Б пропустил, А вышел
            Assert.Same(a, result.Winner);
            Assert.False(result.Fish);
            Assert.Equal(0, result.Points[a]);
            Assert.Equal(16, result.Points[b]);
        }

        [Fact]
        public void Domino_EndsRoundImmediately()
        {
            var a = WithTiles("А", new Tile(1, 1), new Tile(1, 2));
            var b = WithTiles("Б", new Tile(6, 6));
            var c = WithTiles("В", new Tile(5, 5), new Tile(2, 3));   // [2|3] подойдёт после хода А, но раунд уже кончится

            var (result, _, ui) = Play(null, a, b, c);

            Assert.Equal(["А", "Б", "В", "А"], ui.Turns);
            Assert.Same(a, result.Winner);
            Assert.Equal(2, c.TileCount);
        }

        [Fact]
        public void Fish_LowestPointsWins()
        {
            var a = WithTiles("А", new Tile(1, 1), new Tile(5, 5));
            var b = WithTiles("Б", new Tile(6, 6), new Tile(4, 4));

            var (result, _, _) = Play(null, a, b);

            Assert.True(result.Fish);
            Assert.Same(a, result.Winner);
            Assert.Equal(0, result.Points[a]);
            Assert.Equal(20, result.Points[b]);
        }

        [Fact]
        public void Fish_Tie_HasNoWinner()
        {
            var a = WithTiles("А", new Tile(1, 1), new Tile(2, 3));
            var b = WithTiles("Б", new Tile(0, 5));

            var (result, _, _) = Play(null, a, b);

            Assert.True(result.Fish);
            Assert.Null(result.Winner);
            Assert.Equal(5, result.Points[a]);
            Assert.Equal(5, result.Points[b]);
        }

        [Fact]
        public void LoneEmptyDouble_Costs25()
        {
            var a = WithTiles("А", new Tile(1, 1), new Tile(1, 2));
            var b = WithTiles("Б", new Tile(0, 0));

            var (result, _, _) = Play(null, a, b);

            Assert.Same(a, result.Winner);
            Assert.Equal(25, result.Points[b]);
        }
    }
}
