namespace Domino
{
    public interface ITileValue
    {
        int Value { get; }

        bool Applied { get; }

        bool Applicable(ITileValue value);

        void Apply();
    }
}
