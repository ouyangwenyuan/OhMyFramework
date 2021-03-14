using Newtonsoft.Json;
using System;

namespace DragonU3DSDK.Storage
{
    [System.Serializable]
    public class MemoryInt : StorageBase
    {
        [JsonProperty]
        decimal _vc0;
        [JsonProperty]
        int _vc1;

        public MemoryInt()
        {
            SetValue(0);
        }

        public MemoryInt(int startValue)
        {
            SetValue(startValue);
        }

        public void SetValue(int value)
        {
            if (GetValue() == value)
            {
                return;
            }
            if (value <= 0)
            {
                _vc0 = 0.0m;
                _vc1 = 0;
            }
            else
            {
                _vc1 = (int)Math.Floor(UnityEngine.Random.Range(0.0f, 1.0f) * value);
                _vc0 = (value - _vc1) / 8.0m;
            }
            StorageManager.Instance.LocalVersion++;
            StorageManager.Instance.SyncForce = true;
        }

        public int GetValue()
        {
            return (int)Math.Round(8.0m * _vc0 + _vc1);
        }
        // ---------------------------------//


        public static MemoryInt operator -(MemoryInt value)
        {
            value.SetValue(-value.GetValue());
            return value;
        }

        public static MemoryInt operator ++(MemoryInt value)
        {
            value.SetValue(value.GetValue() + 1);
            return value;
        }

        public static MemoryInt operator --(MemoryInt value)
        {
            value.SetValue(value.GetValue() - 1);
            return value;
        }

        public static MemoryInt operator + (MemoryInt left, MemoryInt right)
        {
            MemoryInt newInt = new MemoryInt();
            newInt.SetValue(left.GetValue() + right.GetValue());
            return newInt;
        }

        public static MemoryInt operator - (MemoryInt left, MemoryInt right)
        {
            MemoryInt newInt = new MemoryInt();
            newInt.SetValue(left.GetValue() - right.GetValue());
            return newInt;
        }

        public static MemoryInt operator * (MemoryInt left, MemoryInt right)
        {
            MemoryInt newInt = new MemoryInt();
            newInt.SetValue(left.GetValue() * right.GetValue());
            return newInt;
        }

        public static MemoryInt operator / (MemoryInt left, MemoryInt right)
        {
            MemoryInt newInt = new MemoryInt();
            newInt.SetValue(left.GetValue() / right.GetValue());
            return newInt;
        }

        public static MemoryInt operator +(MemoryInt left, int right)
        {
            MemoryInt newInt = new MemoryInt();
            newInt.SetValue(left.GetValue() + right);
            return newInt;
        }

        public static MemoryInt operator -(MemoryInt left, int right)
        {
            MemoryInt newInt = new MemoryInt();
            newInt.SetValue(left.GetValue() - right);
            return newInt;
        }

        public static MemoryInt operator *(MemoryInt left, int right)
        {
            MemoryInt newInt = new MemoryInt();
            newInt.SetValue(left.GetValue() * right);
            return newInt;
        }

        public static MemoryInt operator /(MemoryInt left, int right)
        {
            MemoryInt newInt = new MemoryInt();
            newInt.SetValue(left.GetValue() / right);
            return newInt;
        }

        public static bool operator == (MemoryInt left, int right)
        {
            return left.GetValue() == right;
        }

        public static bool operator !=(MemoryInt left, int right)
        {
            return left.GetValue() != right;
        }

        public static bool operator ==(MemoryInt left, MemoryInt right)
        {
            return left.GetValue() == right.GetValue();
        }

        public static bool operator !=(MemoryInt left, MemoryInt right)
        {
            return left.GetValue() != right.GetValue();
        }

        public override bool Equals(object obj)
        {
            return  Equals(obj as MemoryInt, this);
        }

        bool Equals(MemoryInt left, MemoryInt right)
        {
            return left.GetValue() == right.GetValue();
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

}


