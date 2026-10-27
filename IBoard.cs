namespace Domino
{
    public interface IBoard
    {
        bool Started { get; }

        IReadOnlyList<ITile> Tiles { get; }

        ITile TopEnd { get; }

        ITile BottomEnd { get; }

        void Start(ITile tile);

        void Apply(ITile tile);

        bool Applicable(ITile tile);
    }
}
