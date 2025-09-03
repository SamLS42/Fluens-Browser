using Fluens.AppCore.ViewModels;

namespace Fluens.AppCore.Contracts;

public interface ITabPageManager
{
    ITabPage GetParentTabPage(AppTabViewModel tab);
}