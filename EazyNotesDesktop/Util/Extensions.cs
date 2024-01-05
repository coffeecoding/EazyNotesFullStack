using System;
using System.Collections.Generic;
using System.Text;

namespace EazyNotesDesktop.Util
{
    public static class Extensions
    {
        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            if (enumerable == null)
                return;
            foreach (var cur in enumerable)
            {
                action(cur);
            }
        }

        public static List<T> Clone<T>(this List<T> list) where T : ICloneable
        {
            List<T> clonedList = new List<T>();
            list.ForEach((element) => clonedList.Add((T)element.Clone()));
            return clonedList;
        }
    }
}
