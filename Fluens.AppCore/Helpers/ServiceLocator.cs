using Microsoft.Extensions.DependencyInjection;

namespace Fluens.AppCore.Helpers;

public static class ServiceLocator
{
    private static ServiceProvider ServiceProvider { get; set; } = null!;

    public static void SetLocator(ServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }

    public static T GetRequiredService<T>(string? key = null) where T : notnull
    {
        return key == null
            ? ServiceProvider.GetRequiredService<T>() ?? throw new InvalidOperationException($"There is no service of type {typeof(T)}.")
            : ServiceProvider.GetRequiredKeyedService<T>(key) ?? throw new InvalidOperationException($"There is no service of type {typeof(T)} and key {key}.");
    }
}
