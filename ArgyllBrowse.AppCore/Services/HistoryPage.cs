using ArgyllBrowse.Data.Entities;
using System.Collections.ObjectModel;

namespace ArgyllBrowse.AppCore.Services;
public class HistoryPage
{
    public required ReadOnlyCollection<HistoryEntry> Items { get; set; }
    public int? NextLastId { get; set; }
    public bool HasMore { get; set; }
}