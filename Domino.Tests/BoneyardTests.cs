namespace Domino.Tests
{
    public class BoneyardTests
    {
        [Fact]
        public void Empty_WhenNoTiles()
        {
            Assert.True(new Boneyard().Empty);
            Assert.False(new Boneyard([new Tile(1, 2)]).Empty);
        }

        [Fact]
        public void SelectTile_RemovesSelectedTile()
        {
            var first = new Tile(1, 2);
            var second = new Tile(3, 4);
            var tiles = new List<ITile> { first, second };
            var boneyard = new Boneyard(tiles);

            var selected = boneyard.SelectTile(new FakeUserInterface());

            Assert.Same(first, selected);
            Assert.Equal([second], tiles);
        }

        [Fact]
        public void SelectTile_HidesTileValues()
        {
            var ui = new FakeUserInterface();

            new Boneyard([new Tile(1, 2)]).SelectTile(ui);

            Assert.Equal([true], ui.SelectHidden);
        }

        [Fact]
        public void Pop_TakesTileOut()
        {
            var tile = new Tile(1, 2);
            var boneyard = new Boneyard([tile]);

            Assert.Same(tile, boneyard.Pop());
            Assert.True(boneyard.Empty);
            Assert.Throws<InvalidOperationException>(() => boneyard.Pop());
        }
    }
}
