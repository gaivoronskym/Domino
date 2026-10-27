namespace Domino
{
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
}
