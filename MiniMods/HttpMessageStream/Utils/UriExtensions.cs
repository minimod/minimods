using System;

namespace Minimod.HttpMessageStream.Utils
{
    public static class UriExtensions
    {
        public static string GetServerBaseUri(this Uri uri)
        {
            return uri.AbsoluteUri.Substring(0, uri.AbsoluteUri.Length - (uri.AbsolutePath.Length + uri.Query.Length));
        }
    }
}