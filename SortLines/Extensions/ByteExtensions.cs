using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SortLines
{
    public static class ByteExtensions
    {
        public static string ListBytes(this byte[] bytes, string separator, string format)
        {
            string ret;

            if (bytes.Any())
            {
                ret = string.Join(separator, bytes.Select(b => b.ToString(format)).ToArray());
            }
            else
            {
                ret = string.Empty;
            }

            return ret;
        }
    }
}
