using Fluens.AppCore.Helpers;
using Fluens.AppCore.ViewModels;
using System.Collections.ObjectModel;
using System.Reactive;

namespace Fluens.AppCore.Contracts;

public interface IObservableTabView : IDisposable
{
    void CreateTabViewItem(AppTabViewModel vm);
    public IObservable<ReadOnlyCollection<AppTabViewModel>> Items { get; }
    public IObservable<Unit> CollectionEmptied { get; }
    public IObservable<AppTabViewModel> TabCloseRequested { get; }
    public IObservable<Unit> AddTabButtonClick { get; }
    public IObservable<AppTabViewModel> SelectedItem { get; }
    public int IndexOf(AppTabViewModel vm);
    public void SelectItem(AppTabViewModel vm);
    public void RemoveItem(AppTabViewModel vm);
}