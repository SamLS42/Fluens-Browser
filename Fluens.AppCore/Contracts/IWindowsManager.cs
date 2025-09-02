using Fluens.AppCore.ViewModels;

namespace Fluens.AppCore.Contracts;
public interface IWindowsManager
{
    IMainWindow CreateWindow();
    ITabView GetParentTabView(AppTabViewModel tab);
}