namespace Domino
{
    public abstract class MessageEnvelope : IMessage
    {
        protected MessageEnvelope(string text, ConsoleColor color)
        {
            Text = text;
            Color = color;
        }

        public string Text { get; }

        public ConsoleColor Color { get; }
    }
}
