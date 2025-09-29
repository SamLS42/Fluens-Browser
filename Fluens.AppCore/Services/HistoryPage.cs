using Fluens.AppCore.ViewModels.Settings.History;
using System.Collections.ObjectModel;

namespace Fluens.AppCore.Services;

public class HistoryPage
{
    public required ReadOnlyCollection<HistoryEntryViewModel> Items { get; set; }
    public DateTime? NextLastDate { get; set; }
    public int? NextLastId { get; set; }
}