using System;
using System.Collections.Generic;
using UnityEngine;

namespace DragonU3DSDK.Storage
{
    [Serializable]
    public class StorageList<TValue> : List<TValue>
    {
        public StorageList(bool syncForce = false, bool syncForceRemote = false)
        {
            SyncForce = syncForce;
            SyncForceRemote = syncForceRemote;

        }
        bool SyncForce { get; set; }
        bool SyncForceRemote { get; set; }

        void OnStorageChanged()
        {
            StorageManager.Instance.LocalVersion++;
            if (SyncForce)
            {
                StorageManager.Instance.SyncForce = true;
            }
            if (SyncForceRemote)
            {
                StorageManager.Instance.SyncForceRemote = true;
            }
        }

        public new void Add(TValue item)
        {
            base.Add(item);
            OnStorageChanged();
        }

        public new void AddRange(IEnumerable<TValue> collection)
        {
            base.AddRange(collection);
            OnStorageChanged();
        }

        public new void Clear()
        {
            DebugUtil.LogWarning("You are deleting some of the contents of the storage, please double check if you really want to do this");
            base.Clear();
            OnStorageChanged();
        }

        public new List<TOutput> ConvertAll<TOutput>(Converter<TValue, TOutput> converter)
        {
            return base.ConvertAll(converter);
        }

        public new void Insert(int index, TValue item)
        {
            base.Insert(index, item);
            OnStorageChanged();
        }

        public new void InsertRange(int index, IEnumerable<TValue> collection)
        {
            base.InsertRange(index, collection);
            OnStorageChanged();
        }

        public new bool Remove(TValue item)
        {
            DebugUtil.LogWarning("You are deleting some of the contents of the storage, please double check if you really want to do this");
            OnStorageChanged();
            return base.Remove(item);
        }

        public new int RemoveAll(Predicate<TValue> match)
        {
            DebugUtil.LogWarning("You are deleting some of the contents of the storage, please double check if you really want to do this");
            OnStorageChanged();
            return base.RemoveAll(match);
        }

        public new void RemoveAt(int index)
        {
            DebugUtil.LogWarning("You are deleting some of the contents of the storage, please double check if you really want to do this");
            base.RemoveAt(index);
            OnStorageChanged();
        }

        public new void RemoveRange(int index, int count)
        {
            DebugUtil.LogWarning("You are deleting some of the contents of the storage, please double check if you really want to do this");
            base.RemoveRange(index, count);
            OnStorageChanged();
        }

        public new void Reverse()
        {
            DebugUtil.LogWarning("You are changing some structures of the storage, please double check if you really want to do this");
            base.Reverse();
            OnStorageChanged();
        }

        public new void Sort(Comparison<TValue> comparison)
        {
            DebugUtil.LogWarning("You are changing some structures of the storage, please double check if you really want to do this");
            base.Sort(comparison);
            OnStorageChanged();
        }

        public new TValue this[int index]
        {
            get
            {
                return base[index];
            }
            set
            {
                Debug.Assert(value != null);
                if (!value.Equals(base[index]))
                {
                    base[index] = value;
                    OnStorageChanged();
                }
            }
        }
    }
}
