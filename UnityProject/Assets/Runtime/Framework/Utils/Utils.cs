using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;

namespace DragonU3DSDK
{
    public static class EnumDef
    {
        public enum Currency
        {
            Ruby = 0,
            Cash = 1,
            Coins = 2
        }
    }

    public static class Utils
    {

        public static string WWWLoadText(string fileName)
        {
            UnityWebRequest www = UnityWebRequest.Get(fileName);//WWW会自动开始读取文件 
            while (!www.isDone) { }//WWW是异步读取，所以要用循环来等待 
            return www.downloadHandler.text;
        }


        public static string FormatNumber(long num)
        {
            return string.Format("{0:N0}", num);
        }

        public static string FormatNumber(float num)
        {
            return string.Format("{0:N0}", num);
        }

        public static string FormatMoney(float num)
        {
            return num.ToString("C2");
        }

        public static string FormatSize(float fBytes)
        {
            if (fBytes <= 0)
            {
                return "0K";
            }
            else if (fBytes < 102.4f)
            {
                return "0.1K";
            }
            else if (fBytes < 1024 * 1024)
            {
                return string.Format("{0:F1}K", (fBytes / 1024));
            }
            else
            {
                return string.Format("{0:F1}M", (fBytes / (1024 * 1024)));
            }
        }

        public static string Validate(string text)
        {
            //https://msdn.microsoft.com/en-us/library/20bw873z(v=vs.110).aspx
            text = RemoveEmoji(text, @"\p{C}");
            return text;
        }

        static string ValidateEx(string text)
        {
            //https://en.wikipedia.org/wiki/Emoji#Emoji_in_the_Unicode_standard
            // 635 of the 766 codepoints in the Miscellaneous Symbols and Pictographs block are considered emoji.
            string pattern = @"[^\u1F300-\u1F5FF]";
            text = RemoveEmoji(text, pattern);

            // All of the 15 codepoints in the Supplemental Symbols and Pictographs block are considered emoji.
            pattern = @"[^\u1F910-\u1F918\u1F980-\u1F984\u1F9C0]";
            text = RemoveEmoji(text, pattern);

            // All of the 80 codepoints in the Emoticons block are considered emoji.
            //      pattern = @"[^\u1F60-\u1F64]";
            //      text = RemoveEmoji (text, pattern);

            // 87 of the 98 codepoints in the Transport and Map Symbols block are considered emoji.
            //      pattern = @"[^\u1F68-\u1F6F]";
            //      text = RemoveEmoji (text, pattern);

            // 77 of the 256 codepoints in the Miscellaneous Symbols block are considered emoji.
            //      pattern = @"[^\u260-\u26F]";
            //      text = RemoveEmoji (text, pattern);

            // 77 of the 256 codepoints in the Miscellaneous Symbols block are considered emoji.
            //      pattern = @"[^\u270-\u27B]";
            //      text = RemoveEmoji (text, pattern);

            return text;
        }

        static string RemoveEmoji(string text, string pattern)
        {
            return System.Text.RegularExpressions.Regex.Replace(text, pattern, string.Empty);
        }

        public static void AmendBuildingRotation(Transform building)
        {
            var vr = building.localEulerAngles;
            vr.y %= 360;
            if (vr.y < 0)
            {
                vr.y += 360;
            }

            if (vr.y <= 45)
            {
                vr.y = 0;
            }
            else if (vr.y <= 135)
            {
                vr.y = 90;
            }
            else if (vr.y <= 225)
            {
                vr.y = 180;
            }
            else if (vr.y <= 315)
            {
                vr.y = 270;
            }
            else
            {
                vr.y = 0;
            }

            building.localRotation = Quaternion.Euler(vr);
        }

        public static long TotalSeconds()
        {
            TimeSpan ts = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc));
            return Convert.ToInt64(ts.TotalSeconds);
        }

        public static long TotalMilliseconds()
        {
            TimeSpan ts = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc));
            return Convert.ToInt64(ts.TotalMilliseconds);
        }

        public static double Countdown(DateTime dt)
        {
            TimeSpan ts = (dt - DateTime.UtcNow);
            return ts.TotalSeconds;
        }

        public static double Countdown(long seconds)
        {
            TimeSpan ts = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc));
            return seconds - ts.TotalSeconds;
        }

        public static DateTime ParseTime(long seconds)
        {
            DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return dt.AddSeconds(seconds);
        }

        public static DateTime ParseTimeMilliSecond(long milliseconds)
        {
            DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return dt.AddMilliseconds(milliseconds);
        }

        public static DateTime ParseTimeFromNow(long countdown)
        {
            return DateTime.UtcNow.AddSeconds(countdown);
        }

        public static DateTime Now()
        {
            return DateTime.UtcNow;
        }

        public static double PastTime(DateTime pastTime)
        {
            TimeSpan ts = (DateTime.UtcNow - pastTime);
            return ts.TotalSeconds;
        }

        public static int GetDayBySeconds(long seconds)
        {
            return (int)Mathf.Floor(seconds / (3600 * 24));
        }

        public static bool IsSameDay(long seconds1, long seconds2)
        {
            return GetDayInterval(seconds1, seconds2) == 0;
        }

        public static int GetDayInterval(long seconds1, long seconds2)
        {
            int day1 = Utils.GetDayBySeconds(seconds1);
            int day2 = Utils.GetDayBySeconds(seconds2);
            return Mathf.Abs(day1 - day2);
        }

        //获取第二天凌晨时间戳
        public static long GetTomorrowTimestamp()
        {
            DateTime tomorrow = DateTime.UtcNow.AddDays(1);
            DateTime tomorrowMidnight = new DateTime(tomorrow.Year, tomorrow.Month, tomorrow.Day, 0, 0, 0, 0, DateTimeKind.Utc);
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan diff = tomorrowMidnight.ToUniversalTime() - origin;
            return (long)Math.Floor(diff.TotalSeconds);
        }

        public static string GetDateString()
        {
            return DateTime.Now.ToString();
        }

        public static List<int> GetRandomList(int beginInt, int endInt)
        {
            List<int> arr = new List<int>();
            for (int i = 0; i < endInt - beginInt + 1; i++)
            {
                arr.Add(beginInt + i);
            }
            for (int i = beginInt; i <= endInt; i++)
            {
                int index = UnityEngine.Random.Range(i, endInt);
                int tmp = arr[i];
                arr[i] = arr[index];
                arr[index] = tmp;
            }
            return arr;
        }

        /// <summary>
        /// Array to string. split with ,
        /// </summary>
        /// <returns>formatted string.</returns>
        /// <param name="array">Array.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static string ArrayToString<T>(T[] array)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            if (array != null && array.Length > 0)
            {
                for (int i = 0; i < array.Length; i++)
                {
                    if (i != 0)
                    {
                        sb.Append(',');
                    }
                    sb.Append(array[i]);
                }
            }
            return sb.ToString();
        }


        public static string GetTimeString(string format, int seconds)
        {
            string label = format;
            int ms = seconds * 1000;
            int s = seconds;
            int m = s / 60;
            int h = m / 60;
            int d = h / 24;

            string t = "";
            //处理天
            if (label.Contains("%dd"))
            {
                t = d >= 10 ? d.ToString() : ("0" + d.ToString());
                label = label.Replace("%dd", t);
                h = h % 24;
            }
            else if (label.Contains("%d"))
            {
                label = label.Replace("%d", d.ToString());
                h = h % 24;
            }

            //处理小时
            if (label.Contains("%hh"))
            {
                t = h >= 10 ? h.ToString() : ("0" + h.ToString());
                label = label.Replace("%hh", t);
                m = m % 60;
            }
            else if (label.Contains("%h"))
            {
                label = label.Replace("%h", h.ToString());
                m = m % 60;
            }

            //处理分
            if (label.Contains("%mm"))
            {
                t = m >= 10 ? m.ToString() : ("0" + m.ToString());
                label = label.Replace("%mm", t);
                s = s % 60;
            }
            else if (label.Contains("%m"))
            {
                label = label.Replace("%m", m.ToString());
                s = s % 60;
            }

            //处理秒
            if (label.Contains("%ss"))
            {
                t = s >= 10 ? s.ToString() : ("0" + s.ToString());
                label = label.Replace("%ss", t);
                ms = ms % 1000;
            }
            else if (label.Contains("%s"))
            {
                label = label.Replace("%s", s.ToString());
                ms = ms % 1000;
            }

            //处理毫秒
            if (label.Contains("ms"))
            {
                t = ms.ToString();
                label = label.Replace("%ms", t);
            }

            return label;
        }

        // 把一个记录的时间戳转换成日期显示(毫秒)
        public static string GetTimeStampDateString(double timeStamp, string format = "yyyy-MM-dd HH:mm:ss")
        {
            var offset = TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now);
            var dt = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).Add(offset).AddMilliseconds(timeStamp);

            //TimeZoneInfo systemTimeZone = TimeZoneInfo.Local;

            //DateTime dateTime = DateTime.Parse(tim)

            //var dt = TimeZoneInfo.ConvertTimeFromUtc(timeStamp, systemTimeZone);

            return dt.ToString(format);
        }

        public static T ArrayFind<T>(T[] array, Predicate<T> condition)
        {
            T item = default(T);
            if (array != null && array.Length > 0)
            {
                for (int i = 0; i < array.Length; i++)
                {
                    if (condition(array[i]))
                    {
                        item = array[i];
                        break;
                    }
                }
            }
            return item;
        }

        public static int ArrayFindIndex<T>(T[] array, Predicate<T> condition)
        {
            int index = -1;
            if (array != null && array.Length > 0)
            {
                for (int i = 0; i < array.Length; i++)
                {
                    if (condition(array[i]))
                    {
                        index = i;
                        break;
                    }
                }
            }
            return index;
        }

        public static T[] ArrayAdd<T>(T[] array, T item)
        {
            List<T> newArray = new List<T>();
            if (array != null)
            {
                newArray.AddRange(array);
            }
            newArray.Add(item);
            return newArray.ToArray();
        }

        public static int RandomByWeight(Dictionary<int, int> idWeights)
        {
            int weights = 0;
            int id = 0;
            foreach (var kv in idWeights)
            {
                weights += kv.Value;
                id = kv.Key;
            }
            int pos = UnityEngine.Random.Range(0, weights);
            weights = 0;
            foreach (var kv in idWeights)
            {
                weights += kv.Value;
                if (pos < weights)
                {
                    id = kv.Key;
                    break;
                }
            }
            return id;
        }

        public static int RandomByWeight(List<int> weightList)
        {
            int weights = 0;
            int idx = 0;
            for (int i = 0; i < weightList.Count; i++)
            {
                weights += weightList[i];
            }
            int pos = UnityEngine.Random.Range(0, weights);
            weights = 0;
            for (int i = 0; i < weightList.Count; i++)
            {
                weights += weightList[i];
                if (pos < weights)
                {
                    idx = i;
                    break;
                }
            }
            return idx;
        }

        public static int RandomByWeight(int[] weightList)
        {
            int weights = 0;
            int idx = 0;
            for (int i = 0; i < weightList.Length; i++)
            {
                weights += weightList[i];
            }
            int pos = UnityEngine.Random.Range(0, weights);
            weights = 0;
            for (int i = 0; i < weightList.Length; i++)
            {
                weights += weightList[i];
                if (pos < weights)
                {
                    idx = i;
                    break;
                }
            }
            return idx;
        }


        public static void JumpToAnimation(Animator animator, string stateName, float normalizedTime, int layer = -1)
        {
            animator.Play(stateName, layer, normalizedTime);
        }

        public static string GetLevelDisplay(int levelId)
        {
            //潜规则，策划修改为301 - 但需要显示为1
            int lv = levelId % 100;
            return lv.ToString();
        }

        /// <summary>
        /// Gets the ADIDB y platform async.
        /// </summary>
        /// <param name="callback">Application.AdvertisingIdentifierCallback回调参数一共有三：string adid， bool 是否成功 ， string error.</param>
        public static void GetADIDByPlatformAsync(Application.AdvertisingIdentifierCallback callback)
        {
            Application.RequestAdvertisingIdentifierAsync(callback);
        }

        public static string PlayerIdToString(ulong playerId)
        {
            return (playerId + 0xb2c3d4).ToString("x");
        }

        public static ulong StringToPlayerId(string str)
        {
            ulong playerId = Convert.ToUInt64(str, 16);
            playerId -= 0xb2c3d4;
            return playerId;
        }

        public static void SaveToLocal(string key, string data)
        {
            byte[] encryptData = Util.RijndaelManager.Instance.EncryptStringToBytes(data);
            PlayerPrefs.SetString(key, System.Convert.ToBase64String(encryptData));
        }

        public static string ReadFromLocal(string key, string defaultData = "")
        {
            if (PlayerPrefs.HasKey(key))
            {
                byte[] encryptData = System.Convert.FromBase64String(PlayerPrefs.GetString(key));
                defaultData = Util.RijndaelManager.Instance.DecryptStringFromBytes(encryptData);
            }
            return defaultData;
        }

        public static NameValueCollection ParseQueryString(string s)
        {
            NameValueCollection nvc = new NameValueCollection();

            // remove anything other than query string from url
            if (s.Contains("?"))
            {
                s = s.Substring(s.IndexOf('?') + 1);
            }

            foreach (string vp in Regex.Split(s, "&"))
            {
                DebugUtil.Log("vp = " + vp);
                string[] singlePair = Regex.Split(vp, "=");
                if (singlePair.Length == 2)
                {
                    DebugUtil.Log("key = " + singlePair[0] + " value = " + singlePair[1]);
                    nvc.Add(singlePair[0], singlePair[1]);
                }
                else
                {
                    DebugUtil.Log("key = " + singlePair[0] + " value = " + string.Empty);
                    // only one key with no value specified in query string
                    nvc.Add(singlePair[0], string.Empty);
                }
            }

            return nvc;
        }

        public static string GetMD5Hash(string input)
        {
            // step 1, calculate MD5 hash from input
            MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
            byte[] hash = md5.ComputeHash(inputBytes);

            // step 2, convert byte array to hex string
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            return sb.ToString();
        }

        /// <summary>
        /// 获取当前时间戳(毫秒)
        /// </summary>
        /// <returns></returns>
        public static long GetTimeStamp()
        {
            DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1)); // 当地时区
            long timeStamp = (long)(DateTime.Now - startTime).TotalMilliseconds;
            return timeStamp;
        }

        /// <summary>
        /// 判断是否是磁盘空间已满异常
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        public static bool IsDiskFull(Exception ex)
        {
            if(ex == null)
            {
                return false;
            }

            const int HR_ERROR_HANDLE_DISK_FULL = unchecked((int)0x80070027);
            const int HR_ERROR_DISK_FULL = unchecked((int)0x80070070);

            return ex.HResult == HR_ERROR_HANDLE_DISK_FULL
                || ex.HResult == HR_ERROR_DISK_FULL;
        }
    }

}