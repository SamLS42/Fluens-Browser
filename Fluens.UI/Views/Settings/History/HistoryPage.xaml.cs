using DynamicData;
using Fluens.AppCore.Helpers;
using Fluens.AppCore.ViewModels.Settings.History;
using Fluens.UI.Helpers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;

namespace Fluens.UI.Views.Settings.History;

public sealed partial class HistoryPage : ReactiveHistoryPage, IDisposable
{
    private readonly CompositeDisposable _disposables = [];
    private readonly ReadOnlyObservableCollection<GroupHistoryEntry> groupedHistoryEntries;

    public HistoryPage()
    {
        InitializeComponent();

        ViewModel ??= ServiceLocator.GetRequiredService<HistoryPageViewModel>();

        this.WhenActivated(d =>
        {
            ViewModel.RefreshCommand
                .Execute()
                .Subscribe();
        });

        ViewModel.Entries.Connect()
            .GroupOn(vm => vm.LastVisitedOn.ToLongDateString())
            .Transform(group =>
            {
                IDisposable innerListSubscription = group.List.Connect().Bind(out ReadOnlyObservableCollection<HistoryEntryViewModel> items).Subscribe();
                return new GroupHistoryEntry(group.GroupKey, items, innerListSubscription);
            })
            .DisposeMany()
            .Bind(out groupedHistoryEntries)
            .Subscribe()
            .DisposeWith(_disposables);

        ViewModel.LoadHistoryCommand
            .Execute(Constants.HistoryPaginationSize)
            .Subscribe();

        this.BindCommand(ViewModel, vm => vm.LoadHistoryCommand, v => v.LoadMoreBtn, withParameter: Observable.Return(Constants.HistoryPaginationSize))
            .DisposeWith(_disposables);

        this.BindCommand(ViewModel, vm => vm.DeleteSelectedCommand, v => v.DeleteSelectedBtn)
            .DisposeWith(_disposables);

        ViewModel.DeleteSelectedCommand.CanExecute
            .Subscribe(canExecute => DeleteSelectedBtn.Visibility = canExecute ? Visibility.Visible : Visibility.Collapsed);

        this.OneWayBind(ViewModel, vm => vm.MoreAvailable, v => v.LoadMoreBtn.Visibility, moreAvailable => moreAvailable is true ? Visibility.Visible : Visibility.Collapsed)
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
            .Subscribe(_ => EntryList.ItemsPanelRoot.Focus(FocusState.Pointer)); //Retrieve focus from the commandBar
    }

    private void UpdateSelection()
    {
        ViewModel!.SelectedEntries = [.. EntryList.SelectedItems.Cast<HistoryEntryViewModel>()];
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
        }
        else
        {
            EntryList.SelectAll();
        }
    }
}

public partial class ReactiveHistoryPage : ReactivePage<HistoryPageViewModel>;