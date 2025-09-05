namespace Fluens.UI.Views.Settings.History;

public partial class GroupHistoryEntry(IEnumerable<HistoryEntryView> items) : List<HistoryEntryView>(items)
{
    public required string Key { get; set; }

    public override string ToString()
    {
        return Key;
    }
}
