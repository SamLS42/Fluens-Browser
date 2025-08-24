using System.Diagnostics.CodeAnalysis;

namespace Fluens.AppCore.Helpers;
public static class UriExtensions
{
    public static Uri EnforceHttps([NotNull] this Uri uri)
    {
        if (uri.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.Ordinal))
        {
            return uri;
        }

        UriBuilder builder = new(uri)
        {
            Scheme = Uri.UriSchemeHttps
        };

        return builder.Uri;
    }
}
