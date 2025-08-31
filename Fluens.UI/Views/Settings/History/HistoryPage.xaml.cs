using DynamicData;
using Fluens.AppCore.Helpers;
using Fluens.AppCore.ViewModels.Settings.History;
using Fluens.UI.Helpers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using WinRT;

namespace Fluens.UI.Views.Settings.History;
public sealed partial class HistoryPage : ReactiveHistoryPage, IDisposable
{
    private readonly CompositeDisposable _disposables = [];
    private readonly ObservableCollection<GroupHistoryEntry> historyEntries = [];

    public HistoryPage()
    {
        InitializeComponent();

        ViewModel ??= ServiceLocator.GetRequiredService<HistoryPageViewModel>();

        ViewModel.EntriesChanged.Subscribe(_ => RefreshListView())
            .DisposeWith(_disposables);

        ViewModel.LoadHistoryCommand.Execute(UIConstants.HistoryPaginationSize).Subscribe();

        this.BindCommand(ViewModel, vm => vm.LoadHistoryCommand, v => v.LoadMoreBtn, withParameter: Observable.Return(UIConstants.HistoryPaginationSize))
            .DisposeWith(_disposables);

        this.Bind(ViewModel, vm => vm.MoreAvailable, v => v.LoadMoreBtn.Visibility)
            .DisposeWith(_disposables);

        Observable.FromEventPattern(SelectAllBtn, nameof(SelectAllBtn.Click))
            .Subscribe(_ => SelectUnSelectAll());

        Observable.FromEventPattern(UnSelectAllBtn, nameof(UnSelectAllBtn.Click))
            .Subscribe(_ => SelectUnSelectAll());
    }

    private void RefreshListView()
    {
        historyEntries.Clear();

        historyEntries.AddRange(ViewModel!.Entries.Items.Select(vm => new HistoryEntryView() { ViewModel = vm })
            .GroupBy(v => v.ViewModel!.LastVisitedOn.ToLongDateString())
            .Select(g => new GroupHistoryEntry(g) { Key = g.Key }));
    }

    public void Dispose()
    {
        _disposables.Dispose();
    }

    private void SelectAllKeyboardAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        SelectUnSelectAll();
    }

    private void SelectUnSelectAll()
    {
        if (EntryList.Items.All(EntryList.SelectedItems.Contains))
        {
            EntryList.SelectedItems.Clear();
            UnSelectAllBtn.Visibility = Visibility.Collapsed;
        }
        else
        {
            EntryList.SelectAll();
        }
    }

    private void CommandBar_Closed(object sender, object e)
    {
        EntryList.Items.FirstOrDefault()?.As<HistoryEntryView>().Focus(FocusState.Pointer);
    }
}

public partial class ReactiveHistoryPage : ReactivePage<HistoryPageViewModel>;