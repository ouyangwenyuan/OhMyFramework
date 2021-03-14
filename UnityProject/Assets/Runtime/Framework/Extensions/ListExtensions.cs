using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dlugin
{
    public static class ListExtensions
    {
        public static List<T> Append<T>(this List<T> self, IEnumerable<T> him)
        {
            if (self == null)
            {
                self = new List<T>();
            }
            var iter = him.GetEnumerator();
            while (iter.MoveNext())
            {
                self.Add(iter.Current);
            }
            return self;
        }
    }
}
