using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dlugin
{
	public static class ArrayExtensions 
	{
		public static T[] AppendEnumerable<T>(this T[] array, IEnumerable<T> collection)
		{
			List<T> list = array.ToList();
			foreach (var e in collection) {
				list.Add (e);
			}
			return list.ToArray ();
		}

		public static T[] Append<T>(this T[] array, T element)
		{
			List<T> list = array.ToList ();
			list.Add (element);
			return list.ToArray ();
		}

		public static List<T> ToList<T>(this T[] array)
		{
			List<T> list = new List<T> ();
			if (array != null) {
				for (var i = 0; i < array.Length; i++)
					list.Add (array [i]);
			}
			return list;
		}

        public static T GetWithDefault<T>(this T[] array, int index, T def)
        {
            if (array == null || array.Length <= index || index < 0)
            {
                return def;
            }
            else
            {
                return array[index];
            }
        }
	}
}
