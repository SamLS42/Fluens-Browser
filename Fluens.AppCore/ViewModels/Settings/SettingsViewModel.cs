using Fluens.AppCore.ViewModels.Settings.History;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace Fluens.AppCore.ViewModels.Settings;
public partial class SettingsViewModel : ReactiveObject
{
    [Reactive]
    public partial HistoryPageViewModel? HistoryPageViewModel { get; set; }
}
