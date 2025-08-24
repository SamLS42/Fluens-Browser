using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Fluens.UI.Views.Settings.History;
public sealed partial class HistoryPage : Page
{
    public HistoryPage()
    {
        InitializeComponent();

        EntryList.Items.Add(new HistoryEntryView(new()
        {
            LastVisitedOn = DateTime.Now.AddDays(-1),
            DocumentTitle = " | An advanced, composable, reactive model-view-viewmodel framework ",
            FaviconUrl = "https://www.reactiveui.net/images/favicons/favicon.ico",
            Url = new Uri("https://www.reactiveui.net/docs/getting-started/"),
            Host = "reactiveui.net"
        }));

        EntryList.Items.Add(new HistoryEntryView(new()
        {
            LastVisitedOn = DateTime.Now.AddDays(-1),
            DocumentTitle = " | An advanced, composable, reactive model-view-viewmodel framework ",
            FaviconUrl = "https://www.reactiveui.net/images/favicons/favicon.ico",
            Url = new Uri("https://www.reactiveui.net/docs/getting-started/"),
            Host = "reactiveui.net"
        }));
    }

    private object LoadData()
    {
        throw new NotImplementedException();
    }
}
