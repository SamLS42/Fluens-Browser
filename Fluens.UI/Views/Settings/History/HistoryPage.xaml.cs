using DynamicData;
using Fluens.AppCore.Helpers;
using Fluens.AppCore.ViewModels.Settings.History;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive.Disposables;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Fluens.UI.Views.Settings.History;
public sealed partial class HistoryPage : ReactiveHistoryPage, IDisposable
{
    private SourceList<GroupHistoryEntry> _historySource = new();
    private ReadOnlyObservableCollection<GroupHistoryEntry> historyEntries;

    public HistoryPage()
    {
        InitializeComponent();

        ViewModel ??= ServiceLocator.GetRequiredService<HistoryPageViewModel>();

        _historySource.Connect()
            .Bind(out historyEntries)
            .Subscribe();

        this.WhenActivated(disposables =>
        {
            ViewModel.Entries.Connect()
                .Transform(vm => new HistoryEntryView() { ViewModel = vm })
                .GroupOn(v => v.ViewModel!.LastVisitedOn.ToLongDateString())
                .Transform(g => new GroupHistoryEntry(g.List.Items) { Key = g.GroupKey })
                .PopulateInto(_historySource)
                .DisposeWith(disposables);
        });

    }

    public void Dispose()
    {
        _historySource.Dispose();
    }
}

public partial class ReactiveHistoryPage : ReactivePage<HistoryPageViewModel>;