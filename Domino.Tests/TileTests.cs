namespace Domino.Tests
{
    public class TileTests
    {
        [Fact]
        public void TileValue_SameValue_IsApplicable()
        {
            Assert.True(new TileValue(3).Applicable(new TileValue(3)));
        }

        [Fact]
        public void TileValue_DifferentValue_IsNotApplicable()
        {
            Assert.False(new TileValue(3).Applicable(new TileValue(4)));
        }

        [Fact]
        public void TileValue_AlreadyApplied_IsNotApplicable()
        {
            var applied = new TileValue(3);
            applied.Apply();

            Assert.False(applied.Applicable(new TileValue(3)));
            Assert.False(new TileValue(3).Applicable(applied));
        }

        [Theory]
        [InlineData(1, 2, 2, 3)]
        [InlineData(1, 2, 3, 1)]
        [InlineData(1, 2, 1, 5)]
        [InlineData(1, 2, 6, 2)]
        public void Tile_WithCommonValue_IsApplicable(int a, int b, int c, int d)
        {
            Assert.True(new Tile(a, b).Applicable(new Tile(c, d)));
        }

        [Fact]
        public void Tile_WithoutCommonValue_IsNotApplicable()
        {
            Assert.False(new Tile(1, 2).Applicable(new Tile(3, 4)));
        }

        [Fact]
        public void Apply_MarksOnlyMatchedHalves()
        {
            var onBoard = new Tile(1, 2);
            var placed = new Tile(2, 3);

            onBoard.Apply(placed);

            Assert.False(onBoard.Top.Applied);
            Assert.True(onBoard.Bottom.Applied);
            Assert.True(placed.Top.Applied);
            Assert.False(placed.Bottom.Applied);
        }

        [Fact]
        public void Apply_ToDouble_UsesOnlyOneHalfOfDouble()
        {
            var onBoard = new Tile(5, 5);
            var placed = new Tile(5, 2);

            onBoard.Apply(placed);

            Assert.True(onBoard.Top.Applied ^ onBoard.Bottom.Applied);
            Assert.True(placed.Top.Applied);
            Assert.False(placed.Bottom.Applied);
        }

        [Fact]
        public void Applied_WhenBothHalvesApplied()
        {
            var middle = new Tile(1, 2);
            middle.Apply(new Tile(0, 1));
            Assert.False(middle.Applied);

            middle.Apply(new Tile(2, 6));
            Assert.True(middle.Applied);
        }

        [Fact]
        public void ToString_ShowsBothHalves()
        {
            Assert.Equal("[3|5]", new Tile(3, 5).ToString());
        }
    }
}
