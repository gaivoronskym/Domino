namespace Domino
{
    public sealed class TileValue : ITileValue
    {
        private bool _applied;

        public TileValue(int value)
        {
            Value = value;
            _applied = false;
        }

        public int Value { get; }

        public bool Applied => _applied;

        public bool Applicable(ITileValue value)
        {
            return !_applied && !value.Applied && Value == value.Value;
        }

        public void Apply()
        {
            _applied = true;
        }
    }
}
