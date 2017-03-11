using System;
using System.Collections.Generic;
using System.Text;

namespace OniBot
{
    public static class EnumerableExtensions
    {
        public static Random _random = new Random();

        public static T Random<T>(this IList<T> items)
        {
            var index = _random.Next(0, items.Count - 1);
            var item = items[index];

            return item;
        }
    }
}
