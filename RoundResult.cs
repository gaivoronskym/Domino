namespace Domino
{
    // Итог раунда: кто выиграл и сколько очков записать каждому игроку
    public sealed record RoundResult(Player? Winner, bool Fish, IReadOnlyDictionary<Player, int> Points);
}
