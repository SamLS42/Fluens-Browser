using DynamicData;
using Fluens.AppCore.Helpers;
using Fluens.AppCore.Services;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System.Reactive;
using System.Reactive.Linq;

namespace Fluens.AppCore.ViewModels.Settings.History;

public partial class HistoryPageViewModel : ReactiveObject, IDisposable
{
    public IObservableList<HistoryEntryViewModel> Entries => EntriesSource.AsObservableList();
    [Reactive]
    public partial bool MoreAvailable { get; set; } = true;
    [Reactive]
    public partial bool CanSelectAll { get; set; }
    [Reactive]
    public partial List<HistoryEntryViewModel> SelectedEntries { get; set; } = [];
    public ReactiveCommand<int, Unit> LoadHistoryCommand { get; }
    public ReactiveCommand<Unit, Unit> DeleteSelectedCommand { get; }
    public ReactiveCommand<Unit, Unit> ClearHistoryCommand { get; }
    private DateTime? NextLastDate { get; set; }
    private int? NextLastId { get; set; }
    private SourceList<HistoryEntryViewModel> EntriesSource { get; } = new();
    private VisitsService HistoryService { get; } = ServiceLocator.GetRequiredService<VisitsService>();

    public HistoryPageViewModel()
    {
        this.WhenAnyValue(vm => vm.SelectedEntries)
            .Subscribe(_ => UpdateActionsVisibility());

        IObservable<bool> anyEntryIsSelected = this.WhenAnyValue(vm => vm.SelectedEntries)
            .Select(entries => entries.Count > 0);

        DeleteSelectedCommand = ReactiveCommand.CreateFromTask(DeleteSelectedAsync, anyEntryIsSelected);
        ClearHistoryCommand = ReactiveCommand.CreateFromTask(HistoryService.ClearHistoryAsync);
        LoadHistoryCommand = ReactiveCommand.CreateFromTask<int>(LoadHistoryAsync);
    }

    private void UpdateActionsVisibility()
    {
        CanSelectAll = EntriesSource.Count == 0 || SelectedEntries.Count != EntriesSource.Count;
    }

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
                DocumentTitle = e.Title,
                LastVisitedOn = e.LastVisitDate.ToLocalTime(),
                Host = e.Hostname,
            }));
        });

        NextLastId = elementsPage.NextLastId;
        NextLastDate = elementsPage.NextLastDate;

        MoreAvailable = NextLastId is not null;
    }

    private async Task DeleteSelectedAsync(CancellationToken cancellationToken = default)
    {
        await HistoryService.DeleteEntriesAsync([.. SelectedEntries.Select(e => e.Id)], cancellationToken);

        EntriesSource.Clear();
        ResetCursors();

        await LoadHistoryAsync(Constants.HistoryPaginationSize, cancellationToken);
    }

    private void ResetCursors()
    {
        NextLastId = null;
        NextLastDate = null;
        MoreAvailable = true;
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
