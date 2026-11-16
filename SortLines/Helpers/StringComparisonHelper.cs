using System;
using System.Collections.ObjectModel;

namespace SortLines
{
    public static class StringComparisonHelper
    {
        public static readonly ReadOnlyCollection<string> StrComparersDescr =
            Array.AsReadOnly(new string[]
        {
            "StringComparer.InvariantCulture",
            "StringComparer.InvariantCultureIgnoreCase",
            "StringComparer.CurrentCulture",
            "StringComparer.CurrentCultureIgnoreCase",
            "StringComparer.Ordinal",
            "StringComparer.OrdinalIgnoreCase"
        });

        public static readonly ReadOnlyCollection<StringComparer> StrComparers =
            Array.AsReadOnly(new StringComparer[]
        {
            StringComparer.InvariantCulture,
            StringComparer.InvariantCultureIgnoreCase,
            StringComparer.CurrentCulture,
            StringComparer.CurrentCultureIgnoreCase,
            StringComparer.Ordinal,
            StringComparer.OrdinalIgnoreCase
        });
    }
}