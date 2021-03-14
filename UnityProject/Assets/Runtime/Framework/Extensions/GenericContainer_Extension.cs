
using System;
using System.Collections.Generic;

public static class LinkedListExtensions
{
	public static void RemoveAll<T>(this LinkedList<T> linkedList,
	                                Func<T, bool> predicate)
	{
		for (LinkedListNode<T> node = linkedList.First; node != null; )
		{
			LinkedListNode<T> next = node.Next;
			if (predicate(node.Value))
				linkedList.Remove(node);
			node = next;
		}
	}
}

public static class DictionaryExtensions
{
	public static T1 FindKeyByValue<T1, T2>(this Dictionary<T1, T2> dictionary, T2 value)
	{
		List<T1> keys = new List<T1>(dictionary.Keys);
		for(int i = keys.Count - 1; i >= 0 ; --i)
		{
			T1 key = keys[i];
			if (EqualityComparer<T2>.Default.Equals(dictionary[key], value))
			{
				return key;
			}
		}
		return default(T1);
	}

	public static bool RemoveByValue<T1, T2>(this Dictionary<T1, T2> dictionary, T2 value)
	{
		List<T1> keys = new List<T1>(dictionary.Keys);
		for(int i = keys.Count - 1; i >= 0 ; --i)
		{
			T1 key = keys[i];
			if (EqualityComparer<T2>.Default.Equals(dictionary[key], value))
			{
				dictionary.Remove(key);
				return true;
			}
		}
		return false;
	}
}

public static class TimeSpanExtensions
{
	public static string GetTimeStringByHourMinSec(long sec)
	{
		TimeSpan ts = TimeSpan.FromSeconds(sec>0?sec:0);
		return string.Format("{0:D2}:{1:D2}:{2:D2}", ts.Days*24 + ts.Hours, ts.Minutes, ts.Seconds);
	}

	public static string GetTimeStringBy_DHMS(long sec)
	{
		TimeSpan ts = TimeSpan.FromSeconds(sec>0?sec:0);
		return string.Format("{0}D:{1:D2}:{2:D2}:{3:D2}", ts.Days, ts.Hours, ts.Minutes, ts.Seconds);
	}

	public static string GetDateTimeString(long sec)
	{
        var v = DragonU3DSDK.Utils.ParseTime (sec);
		return string.Format("{0}/{1}/{2} {3:D2}:{4:D2}:{5:D2}", v.Year, v.Month, v.Day, v.Hour, v.Minute, v.Second);
	}
}