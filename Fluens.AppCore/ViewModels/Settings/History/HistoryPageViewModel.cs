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

    [Reactive]
    public partial bool? MoreAvailable { get; set; }

    [Reactive]
    public partial bool CanSelectAll { get; set; }

    [Reactive]
    public partial List<HistoryEntryViewModel> SelectedEntries { get; set; } = [];

    public ReactiveCommand<Unit, Unit> DeleteSelectedCommand { get; }
    public ReactiveCommand<Unit, Unit> ClearHistoryCommand { get; }
    private DateTime? NextLastDate { get; set; }
    private int? NextLastId { get; set; }
    private SourceList<HistoryEntryViewModel> EntriesSource { get; } = new();
    public IObservableList<HistoryEntryViewModel> Entries => EntriesSource.AsObservableList();
    private VisitsService HistoryService { get; } = ServiceLocator.GetRequiredService<VisitsService>();

    public HistoryPageViewModel()
    {
        this.WhenAnyValue(vm => vm.SelectedEntries)
            .Subscribe(_ => UpdateActionsVisibility());

        IObservable<bool> anyEntryIsSelected = this.WhenAnyValue(vm => vm.SelectedEntries)
            .Select(entries => entries.Count > 0);

        DeleteSelectedCommand = ReactiveCommand.CreateFromTask(DeleteSelectedAsync, anyEntryIsSelected);
        ClearHistoryCommand = ReactiveCommand.CreateFromTask(HistoryService.ClearHistoryAsync);
    }

    private void UpdateActionsVisibility()
    {
        CanSelectAll = EntriesSource.Count == 0 || SelectedEntries.Count != EntriesSource.Count;
    }

    [ReactiveCommand]
    private async Task LoadHistoryAsync(int limit, CancellationToken cancellationToken = default)
    {
        if (MoreAvailable is not null and false)
        {
            return;
        }

        HistoryPage elementsPage = await HistoryService.GetEntriesAsync(NextLastDate, NextLastId, limit, cancellationToken);

        EntriesSource.Edit(updateAction =>
        {
            updateAction.AddRange(elementsPage.Items);
        });

        NextLastId = elementsPage.NextLastId;
        NextLastDate = elementsPage.NextLastDate;

        MoreAvailable = NextLastId is not null;
    }

    [ReactiveCommand]
    private async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        MoreAvailable = default;
        NextLastDate = default;
        NextLastId = default;

        EntriesSource.Clear();

        await LoadHistoryAsync(Constants.HistoryPaginationSize, cancellationToken);
    }

    private async Task DeleteSelectedAsync(CancellationToken cancellationToken = default)
    {
        await HistoryService.DeleteEntriesAsync([.. SelectedEntries.Select(e => e.PlaceId)], cancellationToken);

        await RefreshAsync(cancellationToken);
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
