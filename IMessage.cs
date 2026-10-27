namespace Domino
{
    public interface IMessage
    {
        string Text { get; }

        ConsoleColor Color { get; }
    }
}
