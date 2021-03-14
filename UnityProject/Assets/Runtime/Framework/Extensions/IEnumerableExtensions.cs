using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dlugin
{
	public static class IEnumerableExtensions {
        public delegate bool JudgeDelegate<T>(T element);
        public delegate TResult SelectorDelegate<T, TResult>(T element);
		public delegate int CompareDelegate<T>(T e1, T e2);
        public delegate string ToStringDelegate<T>(T e);

        public static List<T> Select<T>(this IEnumerable<T> collection, JudgeDelegate<T> judger)
        {
            return collection.Select(judger, e => e);
        }

        public static List<TResult> Select<T, TResult>(this IEnumerable<T> collection, JudgeDelegate<T> judger, SelectorDelegate<T, TResult> selector)
		{
            List<TResult> ret = new List<TResult> ();
			var iter = collection.GetEnumerator ();
			while (iter.MoveNext ()) 
			{
                if (judger == null || judger (iter.Current)) 
				{
                    ret.Add(selector(iter.Current));
				}
			}
			return ret;
        }

        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> collection)
        {
            HashSet<T> ret = new HashSet<T>();
            if (collection != null)
            {
                var iter = collection.GetEnumerator ();
                while (iter.MoveNext ()) 
                {
                    if (!ret.Contains(iter.Current))
                    {
                        ret.Add(iter.Current);
                    }
                }
            }
            return ret;
        }

		public static bool CheckContains<T>(this IEnumerable<T> collection, T e)
		{
			var iter = collection.GetEnumerator ();
			while (iter.MoveNext ()) 
			{
                if (iter.Current.Equals(e)) 
				{
					return true;
				}
			}
			return false;
		}

        public static List<T> ToListEx<T>(this IEnumerable<T> collection)
        {
            List<T> ret = new List<T>();
            var iter = collection.GetEnumerator();
            while (iter.MoveNext())
            {
                ret.Add(iter.Current);
            }
            return ret;
        }

        public static T[] ToArrayEx<T>(this IEnumerable<T> collection)
        {
            return collection.ToListEx().ToArray();
        }

        public static int IndexOfEx<T>(this IEnumerable<T> collection, T element)
        {
            int ret = -1;
            var iter = collection.GetEnumerator();
            while (iter.MoveNext())
            {
                ret++;
                if (iter.Current.Equals(element))
                    break;
            }
            return ret;
        }
        
        public static int IndexOfEx<T>(this IEnumerable<T> collection, Func<T,bool> condition)
        {
            int ret = -1;
            if (condition != null)
            {
                var iter = collection.GetEnumerator();
                while (iter.MoveNext())
                {
                    ret++;
                    if (condition.Invoke(iter.Current))
                        break;
                }
            }
            return ret;
        }

        public static string ToStringEx<T>(this IEnumerable<T> collection, string name, ToStringDelegate<T> toString)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder(name);
            sb.Append(" = {\n");
            if (collection != null)
            {
                var iter = collection.GetEnumerator();
                while (iter.MoveNext())
                {
                    sb.AppendFormat("\t{0},\n", toString(iter.Current));
                }
            }
            sb.Append("}");
            return sb.ToString();
        }

        public static string ToStringEx<T>(this IEnumerable<T> collection, string name)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder(name);
            sb.Append(" = {\n");
            if (collection != null)
            {
                var iter = collection.GetEnumerator();
                while (iter.MoveNext())
                {
                    sb.AppendFormat("\t{0},\n", iter.Current.ToString());
                }
            }
            sb.Append("}");
            return sb.ToString();
        }
	}
}