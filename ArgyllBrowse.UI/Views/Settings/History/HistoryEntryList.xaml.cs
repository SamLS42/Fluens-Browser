using Microsoft.UI.Xaml.Controls;
using System;
using System.Reactive.Linq;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ArgyllBrowse.UI.Views.Settings.History;
public sealed partial class HistoryEntryList : UserControl
{


    public HistoryEntryList()
    {
        InitializeComponent();

        Observable.FromEventPattern(List, nameof(List.Loaded)).Subscribe(_ => LoadData());
    }

    private object LoadData()
    {
        throw new NotImplementedException();
    }
}
