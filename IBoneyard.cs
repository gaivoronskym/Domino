namespace Domino
{
    public interface IBoneyard
    {
        bool Empty { get; }

        ITile Pop();

        IReadOnlyList<ITile> PopAll();

        ITile SelectTile(IUserInterface ui);
    }
}
