using Fluens.AppCore.ViewModels.Settings.History;
using Fluens.UI.Helpers;
using ReactiveUI;
using System.Reactive.Disposables;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Fluens.UI.Views.Settings.History;
public sealed partial class HistoryEntryView : ReactiveHistoryEntryView
{
    public HistoryEntryView(HistoryEntryViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;

        this.WhenActivated(d =>
        {
            this.OneWayBind(ViewModel, vm => vm.LastVisitedOn, v => v.Time.Text, p => p.ToShortDateString()).DisposeWith(d);
            this.OneWayBind(ViewModel, vm => vm.FaviconUrl, v => v.Favicon.Source, p => ImageSourceExtensions.GetFromUrl(p)).DisposeWith(d);
            this.OneWayBind(ViewModel, vm => vm.DocumentTitle, v => v.DocumentTitle.Text).DisposeWith(d);
            this.OneWayBind(ViewModel, vm => vm.Host, v => v.Host.Text).DisposeWith(d);
            this.BindCommand(ViewModel, x => x.OpenUrl, v => v.HyperlinkBtn).DisposeWith(d);
        });
    }
}
public partial class ReactiveHistoryEntryView : ReactiveUserControl<HistoryEntryViewModel>;
