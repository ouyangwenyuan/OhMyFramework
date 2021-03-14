using System.Collections.Generic;

namespace DragonU3DSDK
{
    public static class MaskWord
    {
        static Dictionary<string, bool> wordDict = new Dictionary<string, bool>();

        public static void SetWord(string word)
        {
            word = word.ToLower();
            for (int i = 0; i < word.Length - 1; i++)
            {
                string key = word.Substring(0, i + 1);
                if (!wordDict.ContainsKey(key))
                {
                    wordDict.Add(key, false);
                }
            }
            wordDict[word] = true;
        }

        public static void SetWords(IEnumerable<string> words)
        {
            foreach (string word in words)
            {
                SetWord(word);
            }
        }

        public static bool Check(string text)
        {
            for (int i = 0; i < text.Length; i++)
            {
                for (int j = i; j < text.Length; j++)
                {
                    string key = text.Substring(i, j - i + 1).ToLower();
                    bool matched;

                    if (!wordDict.TryGetValue(key, out matched))
                    {
                        break;
                    }

                    if (matched)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public static string Format(string text)
        {
            return Format(text, '*');
        }

        public static string Format(string text, char mask)
        {
            char[] chars = text.ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                int end = -1;
                for (int j = i; j < chars.Length; j++)
                {
                    string key = new string(chars, i, j - i + 1).ToLower();
                    bool matched;

                    if (!wordDict.TryGetValue(key, out matched))
                    {
                        break;
                    }

                    if (matched)
                    {
                        end = j;
                    }
                }

                if (end != -1)
                {
                    for (int k = i; k <= end; k++)
                    {
                        chars[k] = mask;
                    }
                    i = end;
                }
            }

            return new string(chars);
        }
    }
}