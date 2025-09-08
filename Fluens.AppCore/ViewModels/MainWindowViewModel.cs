using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace Fluens.AppCore.ViewModels;

public partial class MainWindowViewModel : ReactiveObject
{
    [Reactive]
    public partial int Id { get; set; }
}
