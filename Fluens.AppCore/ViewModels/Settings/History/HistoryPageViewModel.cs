using DynamicData;
using Fluens.AppCore.Helpers;
using Fluens.AppCore.Services;
using Microsoft.CodeAnalysis;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System.Reactive.Linq;

namespace Fluens.AppCore.ViewModels.Settings.History;
public partial class HistoryPageViewModel : ReactiveObject, IDisposable
{
    public HistoryPageViewModel()
    {
        Observable.FromAsync(async cancellationToken => await LoadHistoryAsync(cancellationToken: cancellationToken))
            .Subscribe();
    }

    private SourceList<HistoryEntryViewModel> EntriesSource { get; } = new();
    public IObservableList<HistoryEntryViewModel> Entries => EntriesSource.AsObservableList();

    [Reactive]
    public partial bool MoreAvailable { get; set; }
    private DateTime? NextLastDate { get; set; }
    private int? NextLastId { get; set; }

    private HistoryService HistoryService { get; } = ServiceLocator.GetRequiredService<HistoryService>();

    public async Task LoadHistoryAsync(int limit = 10, CancellationToken cancellationToken = default)
    {
        HistoryPage elementsPage = await HistoryService.GetEntriesAsync(NextLastDate, NextLastId, limit, cancellationToken);

        EntriesSource.AddRange(elementsPage.Items.Select(e => new HistoryEntryViewModel()
        {
            Id = e.Id,
            Url = new Uri(e.Url),
            FaviconUrl = e.FaviconUrl,
            DocumentTitle = e.DocumentTitle,
            LastVisitedOn = e.LastVisitedOn,
            Host = e.Host,
        }));

        NextLastId = elementsPage.NextLastId;
        NextLastDate = elementsPage.NextLastDate;

        MoreAvailable = NextLastId is not null;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            EntriesSource.Dispose();
        }
    }
}
