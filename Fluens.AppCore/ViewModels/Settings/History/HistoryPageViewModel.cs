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

    public IObservableList<HistoryEntryViewModel> Entries => EntriesSource.AsObservableList();
    public IObservable<Unit> EntriesChanges => EntriesChanged.AsObservable();
    [Reactive]
    public partial bool MoreAvailable { get; set; } = true;
    [Reactive]
    public partial bool CanSelectAll { get; set; } = true;
    [Reactive]
    public partial List<HistoryEntryViewModel> SelectedEntries { get; set; } = [];
    public ReactiveCommand<Unit, Unit> DeleteSelected { get; }
    private Subject<Unit> EntriesChanged { get; } = new();
    private DateTime? NextLastDate { get; set; }
    private int? NextLastId { get; set; }
    private SourceList<HistoryEntryViewModel> EntriesSource { get; } = new();
    private HistoryService HistoryService { get; } = ServiceLocator.GetRequiredService<HistoryService>();
    public HistoryPageViewModel()
    {
        this.WhenAnyValue(vm => vm.SelectedEntries)
            .WhereNotNull()
            .Subscribe(_ => UpdateActionsVisibility());

        IObservable<bool> anyEntryIsSelected = this.WhenAnyValue(vm => vm.SelectedEntries)
            .WhereNotNull()
            .Select(entries => entries.Count > 0);

        DeleteSelected = ReactiveCommand.CreateFromTask(DeleteSelectedAsync, anyEntryIsSelected);
    }

    private void UpdateActionsVisibility()
    {
        CanSelectAll = SelectedEntries.Count != EntriesSource.Count;
    }

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

        EntriesChanged.OnNext(Unit.Default);
    }

    private async Task DeleteSelectedAsync()
    {
        await Task.CompletedTask;
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
            EntriesChanged.OnCompleted();
            EntriesChanged.Dispose();
        }
    }
}
