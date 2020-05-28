using System;
using System.Collections.Generic;

namespace TichuAI
{
    public static class ListExtensions
    {
        public static void Shuffle<T>(this IList<T> list)
        {
            var count = list.Count;
            var last = count - 1;
            Random random = new Random();
            for (var i = 0; i < last; ++i) {
                var randomIndex = random.Next(i, count);
                var temp = list[i];
                list[i] = list[randomIndex];
                list[randomIndex] = temp;
            }
        }
    }
}