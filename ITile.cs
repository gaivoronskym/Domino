namespace Domino
{
    public interface ITile
    {
        ITileValue Top { get; }

        ITileValue Bottom { get; }

        bool Applied { get; }

        bool Applicable(ITile tile);

        void Apply(ITile tile);
    }
}
