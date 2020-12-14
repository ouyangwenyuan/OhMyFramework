using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace OhMyFramework.Editor
{

    public static class Utils
    {

        static Utils()
        {
            allSides = new List<Side>()
            {
                Side.Right,
                Side.TopRight,
                Side.Top,
                Side.TopLeft,
                Side.Left,
                Side.BottomLeft,
                Side.Bottom,
                Side.BottomRight
            };
            straightSides = new Side[]
            {
                Side.Right, Side.Top,
                Side.Left, Side.Bottom
            };
            slantedSides = new Side[]
            {
                Side.TopRight, Side.TopLeft,
                Side.BottomLeft, Side.BottomRight
            };
        }

        public static T DeepClone<T>(this T t)
        {
            MemoryStream memoryStream = new MemoryStream();
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(memoryStream, t);
            memoryStream.Position = 0;
            return (T)formatter.Deserialize(memoryStream);
        }

        #region Sides

        public static readonly List<Side> allSides;
        public static readonly Side[] straightSides;
        public static readonly Side[] slantedSides;

        public static Side RotateSide(this Side side, int steps)
        {
            int index = (int)side;
            index += steps;
            index = Mathf.CeilToInt(Mathf.Repeat(index, allSides.Count));
            return (Side)index;
        }

        public static Side MirrorSide(this Side side)
        {
            int index = (int)side;
            index += 4;
            index = Mathf.CeilToInt(Mathf.Repeat(index, allSides.Count));
            return (Side)index;
        }

        public static bool IsNotNull(this Side side)
        {
            return side != Side.Null;
        }

        public static bool IsStraight(this Side side)
        {
            int index = (int)side;
            return index >= 0 && index % 2 == 0;
        }

        const float sin45 = 0.7071f;

        public static int X(this Side s)
        {
            switch (s)
            {
                case Side.Top:
                case Side.Bottom:
                    return 0;
                case Side.TopLeft:
                case Side.BottomLeft:
                case Side.Left:
                    return -1;
                case Side.BottomRight:
                case Side.TopRight:
                case Side.Right:
                    return 1;
            }
            return 0;
        }

        public static int Y(this Side s)
        {
            switch (s)
            {
                case Side.Left:
                case Side.Right:
                    return 0;
                case Side.Bottom:
                case Side.BottomRight:
                case Side.BottomLeft:
                    return -1;
                case Side.TopLeft:
                case Side.TopRight:
                case Side.Top:
                    return 1;
            }
            return 0;
        }

        public static float Cos(this Side s)
        {
            switch (s)
            {
                case Side.Top:
                case Side.Bottom:
                    return 0;
                case Side.TopLeft:
                case Side.BottomLeft:
                    return -sin45;
                case Side.Left:
                    return -1;
                case Side.BottomRight:
                case Side.TopRight:
                    return sin45;
                case Side.Right:
                    return 1;
            }
            return 0;
        }

        public static float Sin(this Side s)
        {
            switch (s)
            {
                case Side.Left:
                case Side.Right:
                    return 0;
                case Side.Bottom:
                    return -1;
                case Side.BottomRight:
                case Side.BottomLeft:
                    return -sin45;
                case Side.TopLeft:
                case Side.TopRight:
                    return sin45;
                case Side.Top:
                    return 1;
            }
            return 0;
        }

        public static int2 ToInt2(this Side s)
        {
            switch (s)
            {
                case Side.Right:
                    return int2.right;
                case Side.TopRight:
                    return int2.upRight;
                case Side.Top:
                    return int2.up;
                case Side.TopLeft:
                    return int2.upLeft;
                case Side.Left:
                    return int2.left;
                case Side.BottomLeft:
                    return int2.downLeft;
                case Side.Bottom:
                    return int2.down;
                case Side.BottomRight:
                    return int2.downRight;
                default:
                    return int2.zero;
            }
        }

        public static Side Horizontal(this Side s)
        {
            switch (s)
            {
                case Side.Left:
                case Side.TopLeft:
                case Side.BottomLeft:
                    return Side.Left;
                case Side.Right:
                case Side.TopRight:
                case Side.BottomRight:
                    return Side.Right;
                default:
                    return Side.Null;
            }
        }

        public static Side Vertical(this Side s)
        {
            switch (s)
            {
                case Side.Top:
                case Side.TopLeft:
                case Side.TopRight:
                    return Side.Top;
                case Side.Bottom:
                case Side.BottomLeft:
                case Side.BottomRight:
                    return Side.Bottom;
                default:
                    return Side.Null;
            }
        }

        public static float ToAngle(this Side s)
        {
            if (s.IsNotNull())
                return (int)s * 45;
            return 0;
        }

        #endregion

        #region INT2

        public static bool DistanceIsMoreThen(this Vector2 vector, float maxDistance)
        {
            return !vector.DistanceIsLessThen(maxDistance);
        }

        public static bool DistanceIsLessThen(this Vector2 vector, float maxDistance)
        {
            if (Mathf.Abs(vector.x) > maxDistance || Mathf.Abs(vector.y) > maxDistance)
                return false;
            if (vector.x * vector.x + vector.y * vector.y > maxDistance * maxDistance)
                return false;
            return true;
        }

        #endregion

        #region Dictionary Extensions

        public static void Set<K, V>(this IDictionary<K, V> dictionary, K key, V value)
        {
            if (dictionary.ContainsKey(key))
                dictionary[key] = value;
            else
                dictionary.Add(key, value);
        }

        public static V Get<K, V>(this IDictionary<K, V> dictionary, K key)
        {
            if (dictionary.ContainsKey(key))
                return dictionary[key];
            else
                return default(V);
        }

        public static V GetAndAdd<K, V>(this IDictionary<K, V> dictionary, K key)
        {
            if (!dictionary.ContainsKey(key))
            {
                V value = Activator.CreateInstance<V>();
                dictionary.Add(key, value);
                return value;
            }
            return dictionary[key];
        }

        public static Dictionary<N, M> Unsort<N, M>(this IDictionary<N, M> dictionary, URandom random = null)
        {
            if (dictionary == null)
                return null;

            int[] a = new int[dictionary.Count];
            for (int i = 0; i < a.Length; i++)
                a[i] = i;

            for (int i = a.Length - 1; i > 0; i--)
            {
                int j = random == null ? UnityEngine.Random.Range(0, i) : random.Range(0, i - 1);
                a[j] = a[j] + a[i];
                a[i] = a[j] - a[i];
                a[j] = a[j] - a[i];
            }

            Dictionary<N, M> result = new Dictionary<N, M>();

            for (int i = 0; i < a.Length; i++)
                result.Add(dictionary.Keys.ElementAt(a[i]), dictionary.Values.ElementAt(a[i]));

            return result;
        }

        public static Dictionary<N, M> RemoveAll<N, M>(this IDictionary<N, M> dictionary,
            Func<KeyValuePair<N, M>, bool> condition)
        {
            if (dictionary == null)
                return null;

            Dictionary<N, M> result = new Dictionary<N, M>();

            foreach (KeyValuePair<N, M> pair in dictionary)
                if (!condition(pair))
                    result.Add(pair.Key, pair.Value);

            return result;
        }

        public static void AddPairs<N, M>(this IDictionary<N, M> original, IDictionary<N, M> source)
        {
            foreach (N key in source.Keys)
            {
                original[key] = source[key];
            }
        }

        #endregion

        #region Collection Extensions

        public static T GetRandom<T>(this ICollection<T> collection)
        {
            if (collection == null || collection.Count == 0)
                return default(T);
            return collection.ElementAtOrDefault(UnityEngine.Random.Range(0, collection.Count));
        }

        public static T GetRandom<T>(this IEnumerable<T> collection)
        {
            if (collection == null)
                return default(T);
            T[] elements = collection.ToArray();
            if (elements.Length == 0)
                return default(T);
            return elements[UnityEngine.Random.Range(0, elements.Length)];
        }

        public static List<T> GetRandom<T>(this ICollection<T> collection, int count)
        {
            if (collection == null || collection.Count == 0 || count <= 0)
                return new List<T>();
            if (count == 1)
                return new List<T>() { collection.GetRandom() };
            if (count == collection.Count)
                return new List<T>(collection);
            bool[] mask = new bool[collection.Count];
            int _count = 0;
            int index;
            while (count != _count)
            {
                index = UnityEngine.Random.Range(0, collection.Count);
                if (!mask[index])
                {
                    mask[index] = true;
                    index++;
                }
            }
            List<T> result = new List<T>(count);
            for (index = 0; index < mask.Length; index++)
                if (mask[index])
                    result.Add(collection.ElementAt(index));
            return result;
        }

        public static List<T> GetRandom<T>(this IEnumerable<T> collection, int count)
        {
            if (collection == null)
                return new List<T>();
            if (count == 1)
                return new List<T>() { collection.GetRandom() };
            return collection.ToArray().GetRandom(count);
        }

        public static T GetRandom<T>(this IEnumerable<T> collection, URandom random, string key = null)
        {
            if (collection == null)
                return default(T);
            T[] elements = collection.ToArray();
            if (elements.Length == 0)
                return default(T);
            return elements[random.Range(0, elements.Length - 1, key)];
        }

        public static ICollection<T> Unsort<T>(this ICollection<T> collection, URandom random = null)
        {
            if (collection == null)
                return null;

            int[] a = new int[collection.Count];
            for (int i = 0; i < a.Length; i++)
                a[i] = i;

            for (int i = a.Length - 1; i > 0; i--)
            {
                int j = random == null ? UnityEngine.Random.Range(0, i) : random.Range(0, i - 1);
                a[j] = a[j] + a[i];
                a[i] = a[j] - a[i];
                a[j] = a[j] - a[i];
            }

            List<T> result = new List<T>();

            for (int i = 0; i < a.Length; i++)
                result.Add(collection.ElementAt(a[i]));

            return result;
        }

        public static int Count<T>(this ICollection<T> collection, Func<T, bool> filter)
        {
            if (collection == null)
                return 0;

            int result = 0;

            for (int i = 0; i < collection.Count; i++)
                if (filter(collection.ElementAt(i)))
                    result++;

            return result;
        }

        public static T GetMin<T>(this IEnumerable<T> collection, Func<T, int> filter)
        {
            if (collection == null)
                return default(T);

            int min = int.MaxValue;
            T result = default(T);

            int value;
            foreach (T element in collection)
            {
                value = filter.Invoke(element);
                if (value < min)
                {
                    min = value;
                    result = element;
                }
            }

            return result;
        }

        public static T GetMin<T>(this IEnumerable<T> collection, Func<T, float> filter)
        {
            if (collection == null)
                return default(T);

            float min = float.MaxValue;
            T result = default(T);

            float value;
            foreach (T element in collection)
            {
                value = filter.Invoke(element);
                if (value < min)
                {
                    min = value;
                    result = element;
                }
            }

            return result;
        }

        public static T GetMax<T>(this IEnumerable<T> collection, Func<T, int> filter)
        {
            if (collection == null)
                return default(T);

            int min = int.MinValue;
            T result = default(T);

            int value;
            foreach (T element in collection)
            {
                value = filter.Invoke(element);
                if (value > min)
                {
                    min = value;
                    result = element;
                }
            }

            return result;
        }

        public static T GetMax<T>(this IEnumerable<T> collection, Func<T, float> filter)
        {
            if (collection == null)
                return default(T);

            float min = int.MinValue;
            T result = default(T);

            float value;
            foreach (T element in collection)
            {
                value = filter.Invoke(element);
                if (value > min)
                {
                    min = value;
                    result = element;
                }
            }

            return result;
        }

        public static void ForEach<T>(this IEnumerable<T> collection, Action<T> function)
        {
            if (collection == null)
                return;

            foreach (T t in collection)
                function(t);
        }

        public static bool Contains<T>(this IEnumerable<T> source, Func<T, bool> function)
        {
            foreach (T t in source)
                if (function(t))
                    return true;
            return false;
        }

        public static T Get<T>(this ICollection<T> collection, int index)
        {
            if (collection == null)
                throw new NullReferenceException("Collection is null");
            if (collection.Count == 0 || index < 0 || collection.Count - 1 < index)
                return default(T);

            return collection.ElementAt(index);
        }

        public static ICollection<T> Swap<T>(this ICollection<T> collection, int from, int to)
        {
            if (collection == null)
                throw new NullReferenceException("Collection is null");
            if (from < 0 || from >= collection.Count || to < 0 || to >= collection.Count)
                throw new IndexOutOfRangeException();

            List<T> result = new List<T>();

            for (int i = 0; i < collection.Count; i++)
            {
                if (i == from)
                    result.Add(collection.ElementAt(to));
                else if (i == to)
                    result.Add(collection.ElementAt(from));
                else
                    result.Add(collection.ElementAt(i));
            }

            return result;
        }

        public static Dictionary<N, M> ToDictionary<N, M>(this IEnumerable<KeyValuePair<N, M>> collection)
        {
            Dictionary<N, M> result = new Dictionary<N, M>();
            foreach (KeyValuePair<N, M> pair in collection)
                result.Add(pair.Key, pair.Value);

            return result;
        }

        public static string Join(this IEnumerable<string> values, string separator)
        {
            string result = "";
            int index = 0;
            foreach (string value in values)
            {
                if (index++ > 0)
                    result += separator;
                result += value;
            }
            return result;
        }

        #endregion

        #region Enumerator Extensions

        public static List<T> ToList<T>(this IEnumerator<T> enumerator)
        {
            List<T> result = new List<T>();
            while (enumerator.MoveNext())
                result.Add(enumerator.Current);
            try
            {
                enumerator.Reset();
            }
            catch
            {
            }
            return result;
        }

        public static List<T> ToList<T>(this Func<IEnumerator<T>> enumeratorFunc)
        {
            List<T> result = new List<T>();
            IEnumerator<T> enumerator = enumeratorFunc();
            while (enumerator.MoveNext())
                result.Add(enumerator.Current);
            return result;
        }

        public static IEnumerator<T> Collect<T>(IEnumerator enumerator)
        {
            while (enumerator.MoveNext())
            {
                if (enumerator.Current is T)
                    yield return (T)enumerator.Current;
                else if (enumerator.Current is IEnumerator)
                {
                    IEnumerator subKeys = Collect<T>(enumerator.Current as IEnumerator);
                    while (subKeys.MoveNext())
                        yield return (T)subKeys.Current;
                }
            }
        }

        #endregion

        #region Array Extensions

        public static int IndexOf<T>(this T[] array, T value)
        {
            if (array == null || array.Length == 0)
                throw new NullReferenceException("Array is null or empty");
            for (int index = 0; index < array.Length; index++)
                if (array[index].Equals(value))
                    return index;
            return -1;
        }

        #endregion

        #region Vector Extensions        

        public static Vector2 To2D(this Vector3 original, Asix2 plane = Asix2.XY, bool inverse = false)
        {
            switch (plane)
            {
                case Asix2.XY:
                    return inverse ? new Vector2(original.y, original.x) : new Vector2(original.x, original.y);
                case Asix2.YZ:
                    return inverse ? new Vector2(original.z, original.y) : new Vector2(original.y, original.z);
                case Asix2.XZ:
                    return inverse ? new Vector2(original.z, original.x) : new Vector2(original.x, original.z);
            }
            return Vector2.zero;
        }

        public static Vector3 To3D(this Vector2 original, float z = 0, Asix2 plane = Asix2.XY, bool inverse = false)
        {
            switch (plane)
            {
                case Asix2.XY:
                    return inverse ? new Vector3(original.y, original.x, z) : new Vector3(original.x, original.y, z);
                case Asix2.YZ:
                    return inverse ? new Vector3(z, original.y, original.x) : new Vector3(z, original.x, original.y);
                case Asix2.XZ:
                    return inverse ? new Vector3(original.y, z, original.x) : new Vector3(original.x, z, original.y);
            }
            return Vector3.zero;
        }

        public static Vector3 Scale(this Vector3 original, float x = 1f, float y = 1f, float z = 1f)
        {
            return new Vector3(original.x * x, original.y * y, original.z * z);
        }

        public static Vector2 Rotate(this Vector2 v, float angle)
        {
            float a = angle * Mathf.Deg2Rad;
            return new Vector2(
                v.x * Mathf.Cos(a) - v.y * Mathf.Sin(a),
                v.x * Mathf.Sin(a) + v.y * Mathf.Cos(a));
        }

        public static float Angle(this Vector2 vector)
        {
            float angle = Vector2.Angle(Vector2.right, vector);
            if (vector.y < 0)
                angle = 360f - angle;
            return angle;
        }

        #endregion

        #region Transform Extensions

        public static void DestroyChilds(this Transform transform, bool immediate = false)
        {
            for (int i = 0; i < transform.childCount; i++)
                if (immediate)
                    MonoBehaviour.DestroyImmediate(transform.GetChild(i).gameObject);
                else
                    MonoBehaviour.Destroy(transform.GetChild(i).gameObject);
        }

        public static IEnumerable<Transform> AllChild(this Transform transform, bool all = true)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                yield return transform.GetChild(i);
                if (all)
                    foreach (Transform child in transform.GetChild(i).AllChild(true))
                        yield return child;
            }
        }

        public static Transform GetChildByPath(this Transform transform, string path)
        {
            Transform result = transform;
            foreach (string name in path.Split('\\', '/'))
            {
                if (result == null)
                    return null;
                if (name.IsNullOrEmpty())
                    continue;
                result = result.AllChild(false).FirstOrDefault(c => c.name == name);
            }
            if (result == transform)
                result = null;
            return result;
        }

        public static IEnumerable<Transform> AndAllChild(this Transform transform, bool all = true)
        {
            yield return transform;
            foreach (Transform child in transform.AllChild(all))
                yield return child;
        }

        public static void Reset(this Transform transform)
        {
            transform.localRotation = Quaternion.identity;
            transform.localPosition = Vector3.zero;
            transform.localScale = Vector3.one;
        }

        #endregion

        #region String Extensions

        static Regex addSpacesRegex = new Regex(@"(?<=.)(?=[A-Z])");

        public static string NameFormat(this string name, string prefix, string suffix, bool addSpaces)
        {
            int start, end;
            if (!string.IsNullOrEmpty(prefix) && name.StartsWith(prefix))
                start = prefix.Length;
            else
                start = 0;
            if (!string.IsNullOrEmpty(suffix) && name.EndsWith(suffix))
                end = name.Length - suffix.Length;
            else
                end = name.Length;
            name = name.Substring(start, end);
            if (addSpaces) name = name.NameFormat();
            return name;
        }

        public static string NameFormat(this string name)
        {
            return addSpacesRegex.Replace(name, " ");
        }

        public static bool IsNullOrEmpty(this string text)
        {
            return string.IsNullOrEmpty(text);
        }

        public static string FormatText(this string text, params object[] values)
        {
            return string.Format(text, values);
        }

        public static double CheckSum(this string text)
        {
            double result = 0;
            int last = 0;
            int current;
            for (int i = 0; i < text.Length; i++)
            {
                current = char.ConvertToUtf32(text[i].ToString(), 0);
                result += (current + 1) * (last + 1) * (i + 1);
                last = current;
            }
            return result;
        }

        #endregion

        public static string GenerateKey(int length)
        {
            const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            StringBuilder res = new StringBuilder();
            System.Random rnd = new System.Random();
            while (0 < length--)
            {
                res.Append(valid[rnd.Next(valid.Length)]);
            }
            return res.ToString();
        }

        public static List<Type> FindInheritorTypes<T>()
        {
            Type type = typeof(T);
            return type.Assembly.GetTypes().Where(x => type != x && type.IsAssignableFrom(x)).ToList();
        }

        public static T GetAttribute<T>(this Type type) where T : Attribute
        {
            Type attributeType = typeof(T);
            foreach (object attribute in type.GetCustomAttributes(true))
                if (attributeType.IsAssignableFrom(attribute.GetType()))
                    return (T)attribute;
            return null;
        }

        public static IEnumerable<T> GetAttributes<T>(this Type type) where T : Attribute
        {
            Type attributeType = typeof(T);
            return type.GetCustomAttributes(true).Where(x => attributeType.IsAssignableFrom(x.GetType())).Cast<T>();
        }

        public static List<T> EnumValues<T>()
        {
            if (typeof(T).IsEnum)
                return Enum.GetValues(typeof(T)).OfType<T>().ToList();
            return null;
        }

        public static RectTransform rect(this Transform transform)
        {
            return transform as RectTransform;
        }

        public static string ObjectToJson(List<string> names, List<Type> types, List<object> values)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{");
            for (int i = 0; i < names.Count; i++)
            {
                if(types[i] == typeof(string) && values[i] == null)
                {
                    values[i] = "";
                }

                if (types[i] == typeof(string) && !((values[i] as string).Contains("\"")))
                {
                    sb.AppendFormat("\"{0}\" :\"{1}\"", names[i], values[i]);
                }
                else
                {
                    sb.AppendFormat("\"{0}\" :{1}", names[i], values[i]);
                }

                             
                if (i != names.Count - 1)
                {
                    sb.Append(",");
                }

            }
            sb.Append("}");
            return sb.ToString();
        }


        public static Type GetTypeFromAssembly(string typeName)
        {
            Type instanceType = null;
            Assembly[] assemblies2 = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies2)
            {
                Type type = assembly.GetType(typeName);
                if (type != null)
                {
                    instanceType = type; //(Type)Activator.CreateInstance(type);
                    break;
                }
               
            }
            return instanceType;
        }

    }

    #region Custom Yield Instructions

    public class WaitWithDelay : CustomYieldInstruction
    {
        float? lastTrue;
        float delay;
        Func<bool> predicate;

        public WaitWithDelay(Func<bool> predicate, float delay)
        {
            lastTrue = null;
            this.delay = delay;
            this.predicate = predicate;
        }

        public override bool keepWaiting
        {
            get
            {
                if (predicate())
                {
                    if (!lastTrue.HasValue)
                        lastTrue = Time.time;
                    if (lastTrue.Value + delay < Time.time)
                        return false;
                }
                else
                    lastTrue = null;
                return true;
            }
        }
    }

    #endregion

    #region Pairs

    [Serializable]
    public class Pair
    {
        public string a;
        public string b;

        public Pair(string pa, string pb)
        {
            a = pa;
            b = pb;
        }

        public static bool operator ==(Pair a, Pair b)
        {
            return Equals(a, b);
        }

        public static bool operator !=(Pair a, Pair b)
        {
            return !Equals(a, b);
        }


        public override bool Equals(object obj)
        {
            Pair sec = (Pair)obj;
            return (a.Equals(sec.a) && b.Equals(sec.b)) ||
                   (a.Equals(sec.b) && b.Equals(sec.a));
        }

        public override int GetHashCode()
        {
            return a.GetHashCode() + b.GetHashCode();
        }
    }

    public class Pair<T>
    {
        public T a;
        public T b;

        public Pair(T pa, T pb)
        {
            a = pa;
            b = pb;
        }

        public static bool operator ==(Pair<T> a, Pair<T> b)
        {
            return Equals(a, b);
        }

        public static bool operator !=(Pair<T> a, Pair<T> b)
        {
            return !Equals(a, b);
        }


        public override bool Equals(object obj)
        {
            Pair<T> sec = (Pair<T>)obj;
            return (a.Equals(sec.a) && b.Equals(sec.b)) ||
                   (a.Equals(sec.b) && b.Equals(sec.a));
        }

        public override int GetHashCode()
        {
            return a.GetHashCode() + b.GetHashCode();
        }
    }

    #endregion

    #region Int2 and Area

    [System.Serializable]
    public class int2
    {
        public static readonly int2 zero = new int2(0, 0);

        public static readonly int2 right = new int2(1, 0);
        public static readonly int2 up = new int2(0, 1);
        public static readonly int2 left = new int2(-1, 0);
        public static readonly int2 down = new int2(0, -1);

        public static readonly int2 upRight = new int2(1, 1);
        public static readonly int2 upLeft = new int2(-1, 1);
        public static readonly int2 downRight = new int2(1, -1);
        public static readonly int2 downLeft = new int2(-1, -1);

        public static readonly int2 one = new int2(1, 1);
        public int x;
        public int y;

        private static Queue<int2> PoolQueue = new Queue<int2>();

        public int2(int __x, int __y)
        {
            x = __x;
            y = __y;
        }

        public int2()
        {
            x = 0;
            y = 0;
        }

        public int2(int2 coord)
        {
            x = coord.x;
            y = coord.y;
        }

        public static bool operator ==(int2 a, int2 b)
        {
            if ((object)a == null)
                return (object)b == null;
            return a.Equals(b);
        }

        public static bool operator !=(int2 a, int2 b)
        {
            if ((object)a == null)
                return (object)b != null;
            return !a.Equals(b);
        }

        public static int2 operator *(int2 a, int b)
        {
            return new int2(a.x * b, a.y * b);
        }

        public static int2 operator *(int b, int2 a)
        {
            return a * b;
        }

        public static int2 operator +(int2 a, int2 b)
        {
            return new int2(a.x + b.x, a.y + b.y);
            //return int2.Borrow(a.x + b.x, a.y + b.y);
        }

        public static int2 operator -(int2 a, int2 b)
        {
            return new int2(a.x - b.x, a.y - b.y);
            //return int2.Borrow(a.x - b.x, a.y - b.y);
        }

        public static int2 operator +(int2 a, Side side)
        {
            return a + side.ToInt2();
        }

        public static int2 operator -(int2 a, Side side)
        {
            return a + side.ToInt2();
        }

        public static int2 Borrow(int x = 0,int y = 0)
        {
            if (PoolQueue.Count > 0)
            {
                int2 v = PoolQueue.Dequeue();
                v.x = x;
                v.y = y;
                return v;
            }
            else
            {
                return new int2(x,y);
            }
        }
        public static void Repay(int2 v)
        {
            if (!PoolQueue.Contains(v) && PoolQueue.Count < 500)
            {
                PoolQueue.Enqueue(v);
            }
        }


        public bool IsItHit(int min_x, int min_y, int max_x, int max_y)
        {
            return x >= min_x && x <= max_x && y >= min_y && y <= max_y;
        }

        public int FourSideDistanceTo(int2 destination)
        {
            return Mathf.Abs(x - destination.x) + Mathf.Abs(y - destination.y);
        }

        public int EightSideDistanceTo(int2 destination)
        {
            return Mathf.Max(Mathf.Abs(x - destination.x), Mathf.Abs(y - destination.y));
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is int2))
                return false;
            int2 b = (int2)obj;
            return x == b.x && y == b.y;
        }
   

        public override int GetHashCode()
        {
            return string.Format("{0},{1}", x, y).GetHashCode();
        }
        public int Key()
        {
            //return (x + 500) + (y + 500) * 1000;
            return x + y * 1000 + 500500;
        }
        /**
         * From Key
         */
        public static int2 FK(int key)
        {
            return new int2(key % 1000 - 500, key / 1000 - 500);
        }
        public static int KeyOffset(int key,int ofX = 0,int ofY = 0)
        {
            return key + ofX + ofY * 1000;
        }
        public override string ToString()
        {
            return string.Format("({0}, {1})", x, y);
        }
        public string ToJson()
        {
            return string.Format("{{\"x\":{0},\"y\":{1}}}", x, y);
        }

        static Regex parser = new Regex(@"\((?<x>-?\d+)\,\s*(?<y>-?\d+)\)");

        public static int2 Parse(string raw)
        {
            Match match = parser.Match(raw);
            if (match.Success)
            {
                return new int2(int.Parse(match.Groups["x"].Value), int.Parse(match.Groups["y"].Value));
            }
               
            throw new FormatException("Can't to parse \"" + raw +
                                      "\" to int2 format. It must have next format: (int,int)");
        }

        public int2 XtoY()
        {
            return new int2(y, x);
        }

        public int2 GetClone()
        {
            return (int2)MemberwiseClone();
        }

        public static explicit operator Vector2(int2 coord)
        {
            return new Vector2(coord.x, coord.y);
        }



        public Vector3 ToVector(Asix2 plane, bool inverse = false)
        {
            switch (plane)
            {
                case Asix2.XY:
                    return inverse ? new Vector3(y, x, 0) : new Vector3(x, y, 0);
                case Asix2.YZ:
                    return inverse ? new Vector3(0, x, y) : new Vector3(0, y, x);
                case Asix2.XZ:
                    return inverse ? new Vector3(y, 0, x) : new Vector3(x, 0, y);
            }
            return Vector3.zero;
        }
    }

    [System.Serializable]
    public class int3
    {
        public static readonly int3 zero = new int3(0, 0, 0);
        public static readonly int3 right = new int3(1, 0, 0);
        public static readonly int3 up = new int3(0, 1, 0);
        public static readonly int3 left = new int3(-1, 0, 0);
        public static readonly int3 down = new int3(0, -1, 0);
        public static readonly int3 forward = new int3(0, 0, 1);
        public static readonly int3 back = new int3(0, 0, -1);
        public static readonly int3 one = new int3(1, 1, 1);
        public int x;
        public int y;
        public int z;

        public int3(int _x, int _y, int _z)
        {
            x = _x;
            y = _y;
            z = _z;
        }

        public int3() : this(0, 0, 0)
        {
        }

        public int3(int3 coord)
        {
            x = coord.x;
            y = coord.y;
            z = coord.z;
        }

        public static bool operator ==(int3 a, int3 b)
        {
            if ((object)a == null)
                return (object)b == null;
            return a.Equals(b);
        }

        public static bool operator !=(int3 a, int3 b)
        {
            if ((object)a == null)
                return (object)b != null;
            return !a.Equals(b);
        }

        public static int3 operator *(int3 a, int b)
        {
            return new int3(a.x * b, a.y * b, a.z * b);
        }

        public static int3 operator *(int b, int3 a)
        {
            return a * b;
        }

        public static int3 operator +(int3 a, int3 b)
        {
            return new int3(a.x + b.x, a.y + b.y, a.z + b.z);
        }

        public static int3 operator -(int3 a, int3 b)
        {
            return new int3(a.x - b.x, a.y - b.y, a.z - b.z);
        }

        public bool IsItHit(int min_x, int min_y, int min_z, int max_x, int max_y, int max_z)
        {
            return x >= min_x && x <= max_x && y >= min_y && y <= max_y && z >= min_z && z <= max_z;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is int3))
                return false;
            int3 b = (int3)obj;
            return x == b.x && y == b.y && z == b.z;
        }

        public override int GetHashCode()
        {
            return string.Format("{0},{1},{2}", x, y, z).GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("({0}, {1}, {2})", x, y, z);
        }

        static Regex parser = new Regex(@"\((?<x>-?\d+)\,\s*(?<y>-?\d+)\,\s*(?<z>-?\d+)\)");

        public static int3 Parse(string raw)
        {
            if (parser.IsMatch(raw))
            {
                Match match = parser.Match(raw);
                int3 result = new int3(int.Parse(match.Groups["x"].Value), int.Parse(match.Groups["y"].Value),
                    int.Parse(match.Groups["z"].Value));
                return result;
            }
            throw new FormatException("Can't to parse \"" + raw +
                                      "\" to int3 format. It must have next format: (int,int,int)");
        }

        public int3 GetClone()
        {
            return (int3)MemberwiseClone();
        }

        public static explicit operator Vector3(int3 coord)
        {
            return new Vector3(coord.x, coord.y, coord.z);
        }
    }

    [System.Serializable]
    public class area
    {
        public int left
        {
            get { return position.x; }
        }

        public int down
        {
            get { return position.y; }
        }

        public int right
        {
            get { return position.x + size.x - 1; }
        }

        public int up
        {
            get { return position.y + size.y - 1; }
        }

        public int width
        {
            get { return size.x; }
        }

        public int height
        {
            get { return size.y; }
        }

        public int2 position;
        public int2 size;

        public area(int2 position, int2 size)
        {
            this.position = position.GetClone();
            this.size = size.GetClone();
        }

        public area(int2 position) : this(position, int2.one)
        {
        }

        public area() : this(int2.zero)
        {
        }

        public static bool operator ==(area a, area b)
        {
            if ((object)a == null)
                return (object)b == null;
            return a.Equals(b);
        }

        public static bool operator !=(area a, area b)
        {
            if ((object)a == null)
                return (object)b != null;
            return !a.Equals(b);
        }

        public bool IsItInclude(int2 point)
        {
            return left <= point.x &&
                   right >= point.x &&
                   down <= point.y &&
                   up >= point.y;
        }

        public bool IsItInclude(area subarea)
        {
            return left <= subarea.left &&
                   right >= subarea.right &&
                   down <= subarea.down &&
                   up >= subarea.up;
        }

        public bool IsItIntersect(area subarea)
        {
            return (Mathf.Max(left, subarea.left) <= Mathf.Min(right, subarea.right)) &&
                   (Mathf.Max(down, subarea.down) <= Mathf.Min(up, subarea.up));
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is area))
                return false;
            area b = (area)obj;
            return position == b.position && size == b.size;
        }

        public override int GetHashCode()
        {
            return position.GetHashCode() + size.GetHashCode();
        }

        public override string ToString()
        {
            if (this == null)
                return "(Null)";
            else
                return "position:" + position.ToString() + " size:" + size.ToString();
        }

        public area GetClone()
        {
            return (area)MemberwiseClone();
        }

        public IEnumerable<int2> GetPoints()
        {
            List<int2> result = new List<int2>();
            for (int x = left; x <= right; x++)
                for (int y = down; y <= up; y++)
                    result.Add(new int2(x, y));
            return result;
        }
    }

    #endregion

    #region Color

    [System.Serializable]
    public struct HSBColor
    {
        public float h;
        public float s;
        public float b;
        public float a;

        public HSBColor(float h, float s, float b, float a)
        {
            this.h = h;
            this.s = s;
            this.b = b;
            this.a = a;
        }

        public HSBColor(float h, float s, float b)
        {
            this.h = h;
            this.s = s;
            this.b = b;
            this.a = 1f;
        }

        public HSBColor(Color col)
        {
            HSBColor temp = FromColor(col);
            h = temp.h;
            s = temp.s;
            b = temp.b;
            a = temp.a;
        }

        public static HSBColor FromColor(Color color)
        {
            HSBColor ret = new HSBColor(0f, 0f, 0f, color.a);

            float r = color.r;
            float g = color.g;
            float b = color.b;

            float max = Mathf.Max(r, Mathf.Max(g, b));

            if (max <= 0)
            {
                return ret;
            }

            float min = Mathf.Min(r, Mathf.Min(g, b));
            float dif = max - min;

            if (max > min)
            {
                if (g == max)
                {
                    ret.h = (b - r) / dif * 60f + 120f;
                }
                else if (b == max)
                {
                    ret.h = (r - g) / dif * 60f + 240f;
                }
                else if (b > g)
                {
                    ret.h = (g - b) / dif * 60f + 360f;
                }
                else
                {
                    ret.h = (g - b) / dif * 60f;
                }
                if (ret.h < 0)
                {
                    ret.h = ret.h + 360f;
                }
            }
            else
            {
                ret.h = 0;
            }

            ret.h *= 1f / 360f;
            ret.s = (dif / max) * 1f;
            ret.b = max;

            return ret;
        }

        public static Color ToColor(HSBColor hsbColor)
        {
            float r = hsbColor.b;
            float g = hsbColor.b;
            float b = hsbColor.b;
            if (hsbColor.s != 0)
            {
                float max = hsbColor.b;
                float dif = hsbColor.b * hsbColor.s;
                float min = hsbColor.b - dif;

                float h = hsbColor.h * 360f;

                if (h < 60f)
                {
                    r = max;
                    g = h * dif / 60f + min;
                    b = min;
                }
                else if (h < 120f)
                {
                    r = -(h - 120f) * dif / 60f + min;
                    g = max;
                    b = min;
                }
                else if (h < 180f)
                {
                    r = min;
                    g = max;
                    b = (h - 120f) * dif / 60f + min;
                }
                else if (h < 240f)
                {
                    r = min;
                    g = -(h - 240f) * dif / 60f + min;
                    b = max;
                }
                else if (h < 300f)
                {
                    r = (h - 240f) * dif / 60f + min;
                    g = min;
                    b = max;
                }
                else if (h <= 360f)
                {
                    r = max;
                    g = min;
                    b = -(h - 360f) * dif / 60 + min;
                }
                else
                {
                    r = 0;
                    g = 0;
                    b = 0;
                }
            }

            return new Color(Mathf.Clamp01(r), Mathf.Clamp01(g), Mathf.Clamp01(b), hsbColor.a);
        }

        public Color ToColor()
        {
            return ToColor(this);
        }

        public override string ToString()
        {
            return "H:" + h + " S:" + s + " B:" + b;
        }

        public static HSBColor Lerp(HSBColor a, HSBColor b, float t)
        {
            float h, s;

            if (a.b == 0)
            {
                h = b.h;
                s = b.s;
            }
            else if (b.b == 0)
            {
                h = a.h;
                s = a.s;
            }
            else
            {
                if (a.s == 0)
                {
                    h = b.h;
                }
                else if (b.s == 0)
                {
                    h = a.h;
                }
                else
                {
                    // works around bug with LerpAngle
                    float angle = Mathf.LerpAngle(a.h * 360f, b.h * 360f, t);
                    while (angle < 0f)
                        angle += 360f;
                    while (angle > 360f)
                        angle -= 360f;
                    h = angle / 360f;
                }
                s = Mathf.Lerp(a.s, b.s, t);
            }
            return new HSBColor(h, s, Mathf.Lerp(a.b, b.b, t), Mathf.Lerp(a.a, b.a, t));
        }
    }

    #endregion

    [Serializable]
    public class TreeFolder
    {
        public string path = "";
        public string name = "";

        public string fullPath
        {
            get
            {
                if (path.Length > 0)
                    return path + '/' + name;
                else
                    return name;
            }
            set
            {
                int sep = value.LastIndexOf('/');
                if (sep >= 0)
                {
                    path = value.Substring(0, sep);
                    name = value.Substring(sep + 1, value.Length - sep - 1);
                }
                else
                {
                    path = "";
                    name = value;
                }
            }
        }

        public override int GetHashCode()
        {
            return fullPath.GetHashCode();
        }
    }

    public class EasingFunctions
    {
        public static float linear(float t)
        {
            return t;
        }

        public static float easeInQuad(float t)
        {
            return t * t;
        }

        public static float easeOutQuad(float t)
        {
            return t * (2 - t);
        }

        public static float easeInOutQuad(float t)
        {
            return t < .5f ? 2 * t * t : -1 + (4 - 2 * t) * t;
        }

        public static float easeInCubic(float t)
        {
            return t * t * t;
        }

        public static float easeOutCubic(float t)
        {
            return (--t) * t * t + 1;
        }

        public static float easeInOutCubic(float t)
        {
            return t < .5f ? 4 * t * t * t : (t - 1) * (2 * t - 2) * (2 * t - 2) + 1;
        }

        public static float easeInQuart(float t)
        {
            return t * t * t * t;
        }

        public static float easeOutQuart(float t)
        {
            return 1 - (--t) * t * t * t;
        }

        public static float easeInOutQuart(float t)
        {
            return t < .5f ? 8 * t * t * t * t : 1 - 8 * (--t) * t * t * t;
        }

        public static float easeInQuint(float t)
        {
            return t * t * t * t * t;
        }

        public static float easeOutQuint(float t)
        {
            return 1 + (--t) * t * t * t * t;
        }

        public static float easeInOutQuint(float t)
        {
            return t < .5f ? 16 * t * t * t * t * t : 1 + 16 * (--t) * t * t * t * t;
        }

        public static float easeInElastic(float t)
        {
            if (t == 0 || t == 1)
                return t;
            float p = 0.5f;
            return -(Mathf.Pow(2, -10 * t) * Mathf.Sin(-(t + p / 4) * (2 * Mathf.PI) / p));
        }

        public static float easeOutElastic(float t)
        {
            if (t == 0 || t == 1)
                return t;
            float p = 0.5f;
            return Mathf.Pow(2, -10 * t) * Mathf.Sin((t - p / 4) * (2 * Mathf.PI) / p) + 1;
        }

        public static float easeInOutElastic(float t)
        {
            if (t <= 0 || t >= 1)
                return Mathf.Clamp01(t);
            t = Mathf.Lerp(-1, 1, t);

            float p = 0.9f;

            if (t < 0)
                return 0.5f * (Mathf.Pow(2, 10 * t) * Mathf.Sin((t + p / 4) * (2 * Mathf.PI) / p));
            else
                return Mathf.Pow(2, -10 * t) * Mathf.Sin((t - p / 4) * (2 * Mathf.PI) / p) * 0.5f + 1;
        }
    }

    [Serializable]
    public class SortingLayerAndOrder
    {
        public int layerID = 0;
        public int order = 0;

        public SortingLayerAndOrder(int layerID, int order)
        {
            this.layerID = layerID;
            this.order = order;
        }
    }

    public abstract class MonoBehaviourAwakeAssistant<T> : MonoBehaviour where T : MonoBehaviour
    {
        static T _main = null;


        public static T main
        {
            get { return _main; }
            set { _main = value; }
        }

        public void Awake()
        {
            _main = this as T;
        }
    }

    public class URandom
    {
        public readonly int seed;

        public static URandom main = new URandom();

        public URandom(int seed)
        {
            this.seed = seed;
            last = seed;
            m = 325648;
            c = 270312;
            a = 123856;
        }

        public URandom() : this(UnityEngine.Random.Range(int.MinValue, int.MaxValue))
        {
        }

        int m;
        int c;
        int a;

        int last;

        public float Value(string key = null)
        {
            int x;
            if (string.IsNullOrEmpty(key))
            {
                last = (a * last + c) % m;
                x = last;
            }
            else
                x = seed + GetCode(key);
            return System.Math.Abs((1f * (a * x + c) % m) / m);
        }

        public T ValueRange<T>(string key, params T[] values)
        {
            if (values.Length == 0)
                return default(T);
            return values[Range(0, values.Length - 1, key)];
        }

        public bool Chance(float probability, string key = null)
        {
            float value = Value(key);
            return probability > value;
        }

        public float Range(float min, float max, string key = null)
        {
            if (min >= max)
            {
                float z = min;
                min = max;
                max = z;
            }
            float value = Value(key);
            return min + (max - min) * value;
        }

        public int Range(int min, int max, string key = null)
        {
            if (min >= max)
            {
                int z = min;
                min = max;
                max = z;
            }
            float value = Range((float)min, (float)max + 1, key);
            return (int)Math.Floor(value);
        }

        public float Range(FloatRange range, string key = null)
        {
            return Range(range.min, range.max, key);
        }

        public int Range(IntRange range, string key = null)
        {
            return Range(range.min, range.max, key);
        }

        public int Seed(string key = null)
        {
            int x;
            if (string.IsNullOrEmpty(key))
            {
                last = (a * last + c) % m;
                x = last;
            }
            else
                x = seed + GetCode(key);
            return (a * x + c) % m;
        }

        public URandom NewRandom(string key = null)
        {
            return new URandom(Seed(key));
        }

        static int GetCode(string key)
        {
            return (int)(Mathf.Pow(key.GetHashCode() % 9651348, 3) % 7645289);
        }

        public T ValueByProbability<T>(List<Event<T>> values, string key)
        {
            if (values != null)
            {
                float probability = values.Sum(x => x.probability) * Value(key);
                foreach (Event<T> value in values)
                {
                    probability -= value.probability;
                    if (probability <= 0)
                        return value.info;
                }
            }
            return default(T);
        }

        public class Event<T>
        {
            internal T info;
            internal float probability;

            public Event(T info, float probability)
            {
                this.probability = probability;
                this.info = info;
            }
        }
    }

    [Serializable]
    public class IntRange : IEnumerable<int>
    {
        public int min;
        public int max;

        public int interval
        {
            get { return Mathf.Abs(max - min); }
        }

        public int Max
        {
            get { return Mathf.Max(min, max); }
        }

        public int Min
        {
            get { return Mathf.Min(min, max); }
        }

        public int count
        {
            get { return Mathf.Abs(max - min) + 1; }
        }

        public IntRange(int min, int max)
        {
            this.min = min;
            this.max = max;
        }

        public bool IsInRange(int value)
        {
            return value >= Min && value <= Max;
        }

        public int Lerp(float t)
        {
            return Mathf.RoundToInt(Mathf.Lerp(min, max, t));
        }

        internal IntRange GetClone()
        {
            return (IntRange)MemberwiseClone();
        }

        IEnumerator<int> IEnumerable<int>.GetEnumerator()
        {
            for (int value = Min; value <= Max; value++)
                yield return value;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (this as IEnumerable<int>).GetEnumerator();
        }

        static Regex parser = new Regex(@"\((?<min>\d+)\,(?<max>\d+)\)");

        public static IntRange Parse(string raw)
        {
            var match = parser.Match(raw);
            if (match.Success)
                return new IntRange(int.Parse(match.Groups["min"].Value), int.Parse(match.Groups["max"].Value));
            throw new FormatException("Can't to parse \"" + raw +
                                      "\" to IntRange format. It must have next format: (int,int)");
        }

        static string format = "({0},{1})";

        public override string ToString()
        {
            return string.Format(format, min, max);
        }

        public static implicit operator IntRange(int number)
        {
            return new IntRange(number, number);
        }
    }

    [Serializable]
    public class FloatRange
    {
        public float min;
        public float max;

        public float interval
        {
            get { return Mathf.Abs(max - min); }
        }

        public FloatRange(float min, float max)
        {
            this.min = min;
            this.max = max;
        }

        public bool IsInRange(float value)
        {
            return value >= Mathf.Min(min, max) && value <= Mathf.Max(min, max);
        }

        public float Lerp(float t)
        {
            return Mathf.Lerp(min, max, t);
        }

        internal FloatRange GetClone()
        {
            return (FloatRange)MemberwiseClone();
        }

        static Regex parser = new Regex(@"\((?<min>\d*\.?\d+)\,(?<max>\d*\.?\d+)\)");

        public static FloatRange Parse(string raw)
        {
            var match = parser.Match(raw);
            if (match.Success)
                return new FloatRange(float.Parse(match.Groups["min"].Value), float.Parse(match.Groups["max"].Value));
            throw new FormatException("Can't to parse \"" + raw +
                                      "\" to FloatRange format. It must have next format: (float,float)");
        }

        static string format = "({0},{1})";

        public override string ToString()
        {
            return string.Format(format, min, max);
        }

        public static implicit operator FloatRange(float value)
        {
            return new FloatRange(value, value);
        }
    }

    public class DelayedAccess
    {
        DateTime nextTime;
        TimeSpan timeSpan;

        public DelayedAccess(float delaySecond, bool fromThisMoment = false)
        {
            timeSpan = new TimeSpan(0, 0, 0, 0, Mathf.RoundToInt(delaySecond * 1000));
            if (fromThisMoment) ResetTimer();
        }

        public void Break()
        {
            nextTime = new DateTime();
        }

        public void ResetTimer()
        {
            nextTime = DateTime.Now + timeSpan;
        }

        public bool GetAccess()
        {
            if (DateTime.Now < nextTime) return false;
            ResetTimer();
            return true;
        }
    }

    public abstract class IScriptingDefineSymbol
    {
        public bool enable = false;

        public IScriptingDefineSymbol()
        {
        }

        public abstract string GetSybmol();
        public abstract string GetDescription();
        public abstract string GetBerryLink();
    }

    #region Enums

    public enum Side
    {
        Null = -1,
        Right = 0,
        TopRight = 1,
        Top = 2,
        TopLeft = 3,
        Left = 4,
        BottomLeft = 5,
        Bottom = 6,
        BottomRight = 7,
    }

    public enum Orientation
    {
        Right = 0,
        Forward = 1,
        Left = 2,
        Backward = 3
    }

    public enum OrientationLine
    {
        Horizontal,
        Vertical
    }

    public enum Asix2
    {
        XY = 0,
        XZ = 1,
        YZ = 2
    }

    public enum Asix
    {
        X = 0,
        Y = 1,
        Z = 2
    }

    #endregion

    #region Helpers

    public class ExecutionTimer : IDisposable
    {
        DateTime start;
        string title;

        List<Checkpoint> checkpoints = new List<Checkpoint>();

        public ExecutionTimer(string title = "Execution")
        {
            start = DateTime.Now;
            this.title = title;
        }

        public void Flash(string name = null)
        {
            Checkpoint f = new Checkpoint();
            f.time = (DateTime.Now - start).TotalMilliseconds;
            f.name = name;
            checkpoints.Add(f);
        }

        public void Dispose()
        {
            string message = null;
            if (checkpoints.Count > 0)
            {
                message += title + ":\n";
                message += "Total: " + (DateTime.Now - start).TotalMilliseconds + " ms.";
                double lastTime = 0;
                for (int i = 0; i < checkpoints.Count; i++)
                {
                    message += "\n" + (checkpoints[i].name == null ? "Checkpoint" + i : checkpoints[i].name) +
                               ": " + (checkpoints[i].time - lastTime) + " ms.";
                    lastTime = checkpoints[i].time;
                }

            }
            else
                message += title + ": " + (DateTime.Now - start).TotalMilliseconds + " ms.";

             //DragonU3DSDK.DebugUtil.Log(message);
        }

        struct Checkpoint
        {
            public double time;
            public string name;
        }
    }

    #endregion
}