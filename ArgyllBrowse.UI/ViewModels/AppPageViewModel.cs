using ArgyllBrowse.Data.Services;
using ArgyllBrowse.UI.Views;
using DynamicData;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace ArgyllBrowse.UI.ViewModels;
public class AppPageViewModel()
{
    private SourceList<AppTabViewModel> OpenTabViewModels { get; } = new();

    internal void RestoreOpenTabs()
    {
        return;
    }
}
