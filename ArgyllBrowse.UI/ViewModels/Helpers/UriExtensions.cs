using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArgyllBrowse.UI.ViewModels.Helpers;
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
