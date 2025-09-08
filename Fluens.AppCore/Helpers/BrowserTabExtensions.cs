using Fluens.AppCore.ViewModels;
using Fluens.Data.Entities;

namespace Fluens.AppCore.Helpers;
internal static class BrowserTabExtensions
{
    extension(BrowserTab tab)
    {
        public AppTabViewModel ToAppTabViewModel()
        {
            return new AppTabViewModel
            {
                Id = tab.Id,
                Url = new Uri(tab.Url),
                IsSelected = tab.IsSelected,
                DocumentTitle = tab.DocumentTitle ?? Constants.NewTabTitle,
                FaviconUrl = tab.FaviconUrl,
                Index = tab.Index
            };
        }
    }
}
