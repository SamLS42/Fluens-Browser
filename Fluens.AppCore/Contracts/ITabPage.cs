using Fluens.AppCore.ViewModels;

namespace Fluens.AppCore.Contracts;

public interface ITabPage
{
    Task AddTabAsync(Uri? uri = null, bool isSelected = true, bool activate = false);
    bool HasTab(AppTabViewModel tab);
}
