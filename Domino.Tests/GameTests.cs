namespace Domino.Tests
{
    public class GameTests
    {
        private static List<ITile> FullSet()
        {
            var tiles = new List<ITile>();
            for (var a = 0; a <= 6; a++)
            {
                for (var b = a; b <= 6; b++)
                {
                    tiles.Add(new Tile(a, b));
                }
            }

            return tiles;
        }

        private static List<Player> Players(int count) =>
            Enumerable.Range(1, count).Select(i => new Player($"Игрок {i}")).ToList();

        [Theory]
        [InlineData(1)]
        [InlineData(5)]
        public void WrongPlayerCount_Throws(int count)
        {
            var game = new Game(FullSet(), Players(count), new FakeUserInterface());

            Assert.Throws<InvalidOperationException>(game.Play);
        }

        [Theory]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        public void Game_PlaysRoundsUntilSomeoneReaches101(int count)
        {
            var ui = new FakeUserInterface();
            var players = Players(count);

            new Game(FullSet(), players, ui).Play();

            // Таблица после каждого раунда и итоговая в конце
            Assert.True(ui.Tables.Count >= 2);
            var final = ui.Tables[^1];
            Assert.StartsWith("Игра окончена", final.Title);

            var totals = final.Rows.Select(r => int.Parse(r.Cells[2])).ToList();
            Assert.True(totals.Max() >= 101);

            // Козёл — игрок с наибольшим счётом
            var loser = final.Rows.Single(r => final.Title.EndsWith(r.Cells[1]));
            Assert.Equal(totals.Max(), int.Parse(loser.Cells[2]));
        }

        [Fact]
        public void RoundTotals_AddUpToFinalScore()
        {
            var ui = new FakeUserInterface();

            new Game(FullSet(), Players(2), ui).Play();

            var roundTables = ui.Tables.Take(ui.Tables.Count - 1).ToList();
            var final = ui.Tables[^1];

            foreach (var row in final.Rows)
            {
                var name = row.Cells[1];
                var sum = roundTables.Sum(t => int.Parse(t.Rows.Single(r => r.Cells[0] == name).Cells[1]));
                Assert.Equal(sum, int.Parse(row.Cells[2]));
            }
        }

        [Fact]
        public void Hands_AreEmptyAfterGame()
        {
            var players = Players(3);

            new Game(FullSet(), players, new FakeUserInterface()).Play();

            Assert.All(players, p => Assert.False(p.HasTiles));
        }
    }
}
