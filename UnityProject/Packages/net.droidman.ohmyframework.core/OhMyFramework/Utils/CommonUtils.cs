using System;
using System.Collections.Generic;

namespace OhMyFramework.Utils
{
    public class CommonUtils
    {
        /// <summary>
        /// 遍历枚举
        /// </summary>
        /// <param name="ids">枚举值</param>
        /// <param name="names">枚举名字</param>
        /// <typeparam name="T">枚举类</typeparam>
        public static void GetValuesAndFieldNames<T>(out int[] ids, out string[] names)
        {
            Type type = typeof(T);
            T[] valueArr =(T[]) Enum.GetValues(type);
            List<T> valueList = new List<T>(valueArr);
            valueList.Sort();

            names = new string[valueList.Count];
            ids = new int[valueList.Count];

            for(int i = 0; i < valueList.Count; i ++)
            {

                ids[i] = Convert.ToInt32(valueList[i]);
                names[i] = Enum.GetName(type, valueList[i]);
            }

        }
    }
}