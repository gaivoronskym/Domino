namespace Domino
{
    public sealed record TableRow(IReadOnlyList<string> Cells, bool Highlight = false);
}
