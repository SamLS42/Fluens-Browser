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
    private readonly ObservableCollection<GroupHistoryEntry> groupedHistoryEntries = [];

    public HistoryPage()
    {
        InitializeComponent();

        ViewModel ??= ServiceLocator.GetRequiredService<HistoryPageViewModel>();

        ViewModel.Entries.CountChanged
            .Throttle(TimeSpan.FromMilliseconds(200))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ => RefreshListView())
            .DisposeWith(_disposables);

        ViewModel.LoadHistoryCommand.Execute(Constants.HistoryPaginationSize).Subscribe();

        this.BindCommand(ViewModel, vm => vm.LoadHistoryCommand, v => v.LoadMoreBtn, withParameter: Observable.Return(Constants.HistoryPaginationSize))
            .DisposeWith(_disposables);

        this.BindCommand(ViewModel, vm => vm.DeleteSelectedCmd, v => v.DeleteSelectedBtn)
            .DisposeWith(_disposables);

        ViewModel.DeleteSelectedCmd.CanExecute.Subscribe(canExecute => DeleteSelectedBtn.Visibility = canExecute ? Visibility.Visible : Visibility.Collapsed);

        this.OneWayBind(ViewModel, vm => vm.MoreAvailable, v => v.LoadMoreBtn.Visibility)
            .DisposeWith(_disposables);

        this.OneWayBind(ViewModel, vm => vm.CanSelectAll, v => v.SelectAllBtn.Visibility)
            .DisposeWith(_disposables);

        this.OneWayBind(ViewModel, vm => vm.CanSelectAll, v => v.UnSelectAllBtn.Visibility, canSelectAll => canSelectAll ? Visibility.Collapsed : Visibility.Visible)
            .DisposeWith(_disposables);

        Observable.FromEventPattern(SelectAllBtn, nameof(SelectAllBtn.Click))
            .Subscribe(_ => SelectUnSelectAll());

        Observable.FromEventPattern(UnSelectAllBtn, nameof(UnSelectAllBtn.Click))
            .Subscribe(_ => SelectUnSelectAll());

        Observable.FromEventPattern(EntryList, nameof(EntryList.SelectionChanged))
            .Subscribe(_ => UpdateSelection());

        Observable.FromEventPattern(CommandBar, nameof(CommandBar.Closed))
            .Subscribe(_ => EntryList.Items.FirstOrDefault()?.As<HistoryEntryView>().Focus(FocusState.Pointer)); //Retrieve focus from the commandBar
    }

    private void UpdateSelection()
    {
        ViewModel!.SelectedEntries = [.. EntryList.SelectedItems.Cast<HistoryEntryView>().Select(v => v.ViewModel!)];
    }

    private void RefreshListView()
    {
        groupedHistoryEntries.Clear();

        groupedHistoryEntries.AddRange(ViewModel!.Entries.Items.Select(vm => new HistoryEntryView() { ViewModel = vm })
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
            //UnSelectAllBtn.Visibility = Visibility.Collapsed;
            //SelectAllBtn.Visibility = Visibility.Visible;
        }
        else
        {
            EntryList.SelectAll();
            //UnSelectAllBtn.Visibility = Visibility.Visible;
            //SelectAllBtn.Visibility = Visibility.Collapsed;
        }
    }
}

public partial class ReactiveHistoryPage : ReactivePage<HistoryPageViewModel>;