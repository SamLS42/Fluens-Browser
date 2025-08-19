using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace ArgyllBrowse.AppCore.ViewModels.Settings.History;
internal partial class HistoryEntryViewModel : ReactiveObject
{
    [Reactive]
    public partial int Id { get; set; }
    [Reactive]
    public partial string Url { get; set; }
    [Reactive]
    public partial string FaviconUrl { get; set; }
    [Reactive]
    public partial string? DocumentTitle { get; set; } = string.Empty;
    [Reactive]
    public partial DateTime LastVisitedOn { get; set; }
    [Reactive]
    public partial string? Host { get; set; } = string.Empty;
}
