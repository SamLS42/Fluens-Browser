using DynamicData;
using Fluens.AppCore.Helpers;
using Fluens.AppCore.Services;
using Microsoft.CodeAnalysis;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Fluens.AppCore.ViewModels.Settings.History;
public partial class HistoryPageViewModel : ReactiveObject, IDisposable
{
    private SourceList<HistoryEntryViewModel> EntriesSource { get; } = new();
    public IObservableList<HistoryEntryViewModel> Entries => EntriesSource.AsObservableList();

    private readonly Subject<Unit> _entriesChanged = new();
    public IObservable<Unit> EntriesChanged => _entriesChanged.AsObservable();

    [Reactive]
    public partial bool MoreAvailable { get; set; } = true;
    private DateTime? NextLastDate { get; set; }
    private int? NextLastId { get; set; }

    private HistoryService HistoryService { get; } = ServiceLocator.GetRequiredService<HistoryService>();

    [ReactiveCommand]
    private async Task LoadHistoryAsync(int limit, CancellationToken cancellationToken = default)
    {
        if (!MoreAvailable)
        {
            return;
        }

        HistoryPage elementsPage = await HistoryService.GetEntriesAsync(NextLastDate, NextLastId, limit, cancellationToken);

        EntriesSource.Edit(updateAction =>
        {
            updateAction.AddRange(elementsPage.Items.Select(e => new HistoryEntryViewModel()
            {
                Id = e.Id,
                Url = new Uri(e.Url),
                FaviconUrl = e.FaviconUrl,
                DocumentTitle = e.DocumentTitle,
                LastVisitedOn = e.LastVisitedOn,
                Host = e.Host,
            }));
        });

        NextLastId = elementsPage.NextLastId;
        NextLastDate = elementsPage.NextLastDate;

        MoreAvailable = NextLastId is not null;

        _entriesChanged.OnNext(Unit.Default);
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
            _entriesChanged.OnCompleted();
            _entriesChanged.Dispose();
        }
    }
}
