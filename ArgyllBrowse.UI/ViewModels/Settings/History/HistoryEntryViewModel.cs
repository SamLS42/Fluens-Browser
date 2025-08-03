using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArgyllBrowse.UI.ViewModels.Settings.History;
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
