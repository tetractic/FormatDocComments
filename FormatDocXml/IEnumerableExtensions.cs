// Copyright 2018 Carl Reinke
//
// This file is part of a library that is licensed under the terms of GNU Lesser
// General Public License version 3 as published by the Free Software
// Foundation.
//
// This license does not grant rights under trademark law for use of any trade
// names, trademarks, or service marks.

using System.Collections.Generic;

namespace FormatDocXml
{
    internal static class IEnumerableExtensions
    {
        public static int? Max(this IEnumerable<int> enumerable)
        {
            var enumerator = enumerable.GetEnumerator();
            if (!enumerator.MoveNext())
                return null;
            int max = enumerator.Current;
            while (enumerator.MoveNext())
                if (max < enumerator.Current)
                    max = enumerator.Current;
            return max;
        }
    }
}
