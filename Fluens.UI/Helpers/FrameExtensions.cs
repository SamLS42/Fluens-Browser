using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using WinRT;

namespace Fluens.UI.Helpers;
internal static class FrameExtensions
{
    extension(Frame frame)
    {
        public T Navigate<T>(object? parameter = null, NavigationTransitionInfo? infoOverride = null)
        {
            frame.Navigate(typeof(T), parameter, infoOverride ?? new SuppressNavigationTransitionInfo());
            T content = frame.Content.As<T>();
            return content;
        }
    }
}
