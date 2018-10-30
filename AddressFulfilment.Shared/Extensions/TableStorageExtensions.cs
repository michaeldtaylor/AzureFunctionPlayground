using System;
using System.Collections.Generic;
using System.Linq;

namespace AddressFulfilment.Shared.Extensions
{
    public static class TableStorageExtensions
    {
        public static string ToSafeKey(this string key)
        {
            var validChars = key.ToCharArray();

            validChars = Array.FindAll(validChars, c => (char.IsLetterOrDigit(c) || c == '-'));

            return new string(validChars).ToLowerInvariant();
        }

        public static string ToSafeKeyNotLowered(this string key)
        {
            var validChars = key.ToCharArray();

            validChars = Array.FindAll(validChars, c => char.IsLetterOrDigit(c) || c == '-');

            return new string(validChars);
        }

        public static string ToAlphaNumericKey(this string key)
        {
            var validChars = key.ToCharArray();
            validChars = Array.FindAll(validChars, char.IsLetterOrDigit);

            return new string(validChars);
        }

        public static IEnumerable<T> Paginate<T>(this IQueryable<T> query, int skip, int take)
        {
            query = query.Skip(skip);

            if (take > 0)
            {
                query = query.Take(take);
            }

            var count = 0;

            foreach (var item in query)
            {
                count++;
                yield return item;

                if (count == take)
                {
                    yield break;
                }
            }
        }
    }
}
