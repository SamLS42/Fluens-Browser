using Fluens.AppCore.ViewModels.Settings.History;
using System.Collections.ObjectModel;

namespace Fluens.UI.Views.Settings.History;

public partial class GroupHistoryEntry(string key, ReadOnlyObservableCollection<HistoryEntryViewModel> items, IDisposable cleanup) : IDisposable
{
    public string Key { get; } = key;
    public ReadOnlyObservableCollection<HistoryEntryViewModel> Items { get; } = items;

    public override string ToString()
    {
        return Key;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool dispose)
    {
        if (dispose)
        {
            cleanup.Dispose();
        }
    }
}
