namespace Domino
{
    public static class BoardLayout
    {
        // Цепочка слева направо, каждая косточка повёрнута так, чтобы соседние половинки совпадали
        public static IReadOnlyList<TileFace> Chain(IReadOnlyList<ITile> tiles)
        {
            var result = new List<TileFace>(tiles.Count);
            if (tiles.Count == 0)
            {
                return result;
            }

            // У крайней левой косточки наружу смотрит свободная (не приложенная) половинка
            var first = tiles[0];
            var face = first.Top.Applied && !first.Bottom.Applied
                ? new TileFace(first.Bottom.Value, first.Top.Value)
                : new TileFace(first.Top.Value, first.Bottom.Value);
            result.Add(face);

            for (var i = 1; i < tiles.Count; i++)
            {
                var tile = tiles[i];
                face = tile.Top.Value == face.Right
                    ? new TileFace(tile.Top.Value, tile.Bottom.Value)
                    : new TileFace(tile.Bottom.Value, tile.Top.Value);
                result.Add(face);
            }

            return result;
        }
    }
}
