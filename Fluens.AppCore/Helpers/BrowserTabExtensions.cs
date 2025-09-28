using Fluens.AppCore.ViewModels;
using Fluens.Data.Entities;

namespace Fluens.AppCore.Helpers;

internal static class BrowserTabExtensions
{
    extension(Tab tab)
    {
        public AppTabViewModel ToAppTabViewModel()
        {
            return new AppTabViewModel
            {
                Id = tab.Id,
                Url = new Uri(tab.Place?.Url ?? Constants.AboutBlankUri.ToString()),
                IsSelected = tab.IsSelected,
                DocumentTitle = tab.Place?.Title ?? Constants.NewTabTitle,
                FaviconUrl = tab.Place?.FaviconUrl ?? string.Empty,
                Index = tab.Index
            };
        }
    }
}
