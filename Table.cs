namespace Domino
{
    public sealed record Table(string Title, IReadOnlyList<string> Headers, IReadOnlyList<TableRow> Rows);
}
