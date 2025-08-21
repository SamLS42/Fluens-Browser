using System.Windows.Controls;

namespace ArgyllBrowse.WPF.AppTab
{
    internal class Utils
    {
        public static Func<object> NewItemFactory
        {
            get { return () => new TabContent("Introduction", new Border() { Child = new Label() { Content = "This is a new Tab" } }); }
        }
    }
}
