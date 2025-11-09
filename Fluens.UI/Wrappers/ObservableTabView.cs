using Fluens.AppCore.Contracts;
using Fluens.AppCore.Helpers;
using Fluens.AppCore.ViewModels;
using Fluens.UI.Helpers;
using Fluens.UI.Views;
using Microsoft.UI.Xaml.Controls;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Fluens.UI.Wrappers;

internal partial class ObservableTabView : IObservableTabView
{
    private TabView TabView { get; set; }
    private CompositeDisposable Disposables { get; } = [];
    public ObservableCollection<TabViewItem> Tabs { get; } = [];

    private BehaviorSubject<ReadOnlyCollection<AppTabViewModel>> ItemsSource { get; }
    public IObservable<ReadOnlyCollection<AppTabViewModel>> Items => ItemsSource.AsObservable();

    public Subject<Unit> CollectionEmptiedSource { get; } = new();
    public IObservable<Unit> CollectionEmptied => CollectionEmptiedSource.AsObservable();

    public Subject<AppTabViewModel> TabCloseRequestedSource { get; } = new();
    public IObservable<AppTabViewModel> TabCloseRequested => TabCloseRequestedSource.AsObservable();

    public Subject<Unit> AddTabButtonClickSource { get; } = new();
    public IObservable<Unit> AddTabButtonClick => AddTabButtonClickSource.AsObservable();

    public BehaviorSubject<AppTabViewModel?> SelectedItemSource { get; }
    public IObservable<AppTabViewModel> SelectedItem => SelectedItemSource.AsObservable().WhereNotNull();

    public ObservableTabView(TabView tabView)
    {
        TabView = tabView;
        TabView.TabItemsSource = Tabs;
        ItemsSource = new([.. Tabs.Select(t => t.ViewModel)]);
        SelectedItemSource = new(tabView.SelectedItem is TabViewItem tvi ? tvi.ViewModel : null);

        Observable.FromEventPattern<object?, NotifyCollectionChangedEventArgs>(Tabs, nameof(Tabs.CollectionChanged))
            .Subscribe(ep => ItemsSource.OnNext([.. Tabs.Select(t => t.ViewModel)]))
            .DisposeWith(Disposables);

        Observable.FromEventPattern<object, SelectionChangedEventArgs>(TabView, nameof(TabView.SelectionChanged))
            .Subscribe(ep => SelectedItemSource.OnNext(tabView.SelectedItem is TabViewItem tvi ? tvi.ViewModel : null))
            .DisposeWith(Disposables);

        Observable.FromEventPattern<TabView, TabViewTabCloseRequestedEventArgs>(TabView, nameof(TabView.TabCloseRequested))
            .Subscribe(ep => TabCloseRequestedSource.OnNext(ep.EventArgs.Tab.ViewModel))
            .DisposeWith(Disposables);

        Observable.FromEventPattern<TabView, object>(TabView, nameof(TabView.AddTabButtonClick))
            .Subscribe(_ => AddTabButtonClickSource.OnNext(Unit.Default))
            .DisposeWith(Disposables);

        Observable.FromEventPattern<object?, NotifyCollectionChangedEventArgs>(Tabs, nameof(Tabs.CollectionChanged))
            .Where(_ => Tabs.Count == 0)
            .Subscribe(_ => CollectionEmptiedSource.OnNext(Unit.Default))
            .DisposeWith(Disposables);
    }

    public void CreateTabViewItem(AppTabViewModel vm)
    {
        AppTabContent appTabContent = new() { ViewModel = vm };

        TabViewItem tab = new()
        {
            Header = Constants.NewTabTitle,
            IconSource = UIConstants.BlankPageIcon,
            Content = appTabContent
        };

        CompositeDisposable disposables = [];

        vm.WhenAnyValue(x => x.DocumentTitle)
            .Subscribe(docTitle => tab.Header = GetCorrectTitle(docTitle))
            .DisposeWith(disposables);

        vm.WhenAnyValue(x => x.FaviconUrl)
            .Subscribe(faviconUrl => tab.IconSource = IconSource.GetFromUrl(faviconUrl))
            .DisposeWith(disposables);

        tab.CloseRequested += (sender, args) => disposables.Dispose();

        if (vm.Index != null)
        {
            Tabs.Insert(vm.Index.Value, tab);
        }
        else
        {
            Tabs.Add(tab);
        }
    }

    public int IndexOf(AppTabViewModel vm)
    {
        TabViewItem tvi = GetTabViewItem(vm)!;
        return Tabs.IndexOf(tvi);
    }

    public void SelectItem(AppTabViewModel vm)
    {
        TabViewItem? tvi = GetTabViewItem(vm);
        TabView.SelectedItem = tvi;
    }

    public void RemoveItem(AppTabViewModel vm)
    {
        TabViewItem? tvi = GetTabViewItem(vm);
        Tabs.Remove(tvi!);
    }

    public void Dispose()
    {
        Disposables.Dispose();
        ItemsSource.OnCompleted();
        CollectionEmptiedSource.OnCompleted();
        TabCloseRequestedSource.OnCompleted();
        AddTabButtonClickSource.OnCompleted();
        SelectedItemSource.OnCompleted();
    }

    private TabViewItem? GetTabViewItem(AppTabViewModel vm)
    {
        return Tabs.SingleOrDefault(t => ReferenceEquals(t.ViewModel, vm));
    }

    private static string GetCorrectTitle(string title)
    {
        return string.IsNullOrWhiteSpace(title)
                            || title.Equals(Constants.AboutBlankUri.ToString(), StringComparison.Ordinal)
                            ? Constants.NewTabTitle
                            : title;
    }
}
