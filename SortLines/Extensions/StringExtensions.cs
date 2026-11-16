using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SortLines
{
    public static class StringExtensions
    {
        public static bool IsNumeric(this string value)
        {
            return value.All(char.IsNumber);
        }
    }
}
