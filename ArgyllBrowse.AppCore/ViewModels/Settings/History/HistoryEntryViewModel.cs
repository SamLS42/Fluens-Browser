using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System.Reactive;

namespace ArgyllBrowse.AppCore.ViewModels.Settings.History;
public partial class HistoryEntryViewModel : ReactiveObject
{
    [Reactive]
    public partial int Id { get; set; }
    [Reactive]
    public partial Uri Url { get; set; }
    [Reactive]
    public partial string FaviconUrl { get; set; }
    [Reactive]
    public partial string? DocumentTitle { get; set; } = string.Empty;
    [Reactive]
    public partial DateTime LastVisitedOn { get; set; }
    [Reactive]
    public partial string? Host { get; set; } = string.Empty;

    public ReactiveCommand<Unit, Unit> OpenUrl { get; } = ReactiveCommand.Create(() => { });
}
