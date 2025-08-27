using DynamicData;
using Fluens.AppCore.Helpers;
using Fluens.AppCore.ViewModels.Settings.History;
using Microsoft.UI.Xaml.Controls;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Fluens.UI.Views.Settings.History;
public sealed partial class HistoryPage : Page
{
    private CompositeDisposable disposables = [];
    public HistoryPageViewModel? ViewModel { get; } = ServiceLocator.GetRequiredService<HistoryPageViewModel>();

    private ReadOnlyObservableCollection<HistoryEntryView> historyEntries;

    public HistoryPage()
    {
        InitializeComponent();

        ViewModel.Entries.Connect()
            .Transform(vm => new HistoryEntryView() { ViewModel = vm })
            .Bind(out historyEntries)
            .Subscribe()
            .DisposeWith(disposables);
    }

    private object LoadData()
    {
        throw new NotImplementedException();
    }
}
