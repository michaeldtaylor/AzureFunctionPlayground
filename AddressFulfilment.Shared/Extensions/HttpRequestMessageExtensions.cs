using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using AddressFulfilment.Shared.Utilities;

namespace AddressFulfilment.Shared.Extensions
{
    public static class HttpRequestMessageExtensions
    {
        private const char QueryStringSeparator = '&';

        public static IEnumerable<KeyValuePair<string, string>> GetQueryNameValuePairs(this HttpRequestMessage request)
        {
            Guard.NotNull(nameof(request), request);

            var keyValuePairs = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var query = request.RequestUri.Query;

            var i = 1; // skip the leading '?' character

            while (i < query.Length)
            {
                // find next & while noting first = on the way (and if there are more)
                var startIndex = i;
                var toIndex = -1;

                while (i < query.Length)
                {
                    var ch = query[i];

                    if (ch == '=')
                    {
                        if (toIndex < 0)
                        {
                            toIndex = i;
                        }
                    }
                    else if (ch == QueryStringSeparator) // e.g. '&' or ';'
                    {
                        break;
                    }

                    i++;
                }

                // extract the name / value pair
                string name = null;
                string value;

                if (toIndex >= 0)
                {
                    name = query.Substring(startIndex, toIndex - startIndex).Trim();
                    value = query.Substring(toIndex + 1, i - toIndex - 1).Trim();
                }
                else
                {
                    value = query.Substring(startIndex, i - startIndex).Trim();
                }

                var key = WebUtility.UrlDecode(name);

                if (!string.IsNullOrEmpty(key))
                {
                    keyValuePairs.Add(key, WebUtility.UrlDecode(value));
                }

                i++;
            }

            return keyValuePairs;
        }
    }
}