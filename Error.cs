namespace Domino
{
    public sealed class Error(string text) : MessageEnvelope(text, ConsoleColor.Red);
}
