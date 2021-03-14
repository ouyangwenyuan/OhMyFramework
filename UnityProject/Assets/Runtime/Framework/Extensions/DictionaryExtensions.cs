using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dlugin
{
    public static class DictionaryExtensions {
        public static TValue GetDefault<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, TValue defval)
        {
            if (dict == null)
                return defval;
            TValue v;
            if (dict.TryGetValue(key, out v))
                return v;
            else
                return defval;
        }


        /// <summary>
        /// 获取字典Value
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dictionary"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static TValue GetValue<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key)
        {
            TValue value = default(TValue);
            dictionary.TryGetValue(key, out value);
            return value;
        }
    }
}
