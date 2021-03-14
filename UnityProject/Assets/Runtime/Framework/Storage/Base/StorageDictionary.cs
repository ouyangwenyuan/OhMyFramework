using System;
using System.Collections.Generic;
using UnityEngine;

namespace DragonU3DSDK.Storage
{
    [Serializable]
    public class StorageDictionary<TKey, TValue> : Dictionary<TKey, TValue>
    {
        public StorageDictionary(bool syncForce = false, bool syncForceRemote = false)
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

        public new void Add(TKey key, TValue value)
        {
            base.Add(key, value);
            OnStorageChanged();
        }

        public new void Clear()
        {
            DebugUtil.LogWarning("You are deleting some of the contents of the storage, please double check if you really want to do this");
            base.Clear();
            OnStorageChanged();
        }

        public new bool Remove(TKey key)
        {
            DebugUtil.LogWarning("You are deleting some of the contents of the storage, please double check if you really want to do this");
            OnStorageChanged();
            return base.Remove(key);
        }

        public new TValue this[TKey key]
        {
            get
            {
                return base[key];
            }
            set
            {
                Debug.Assert(value != null);
                if (!base.ContainsKey(key) || !value.Equals(base[key]))
                {
                    base[key] = value;
                    OnStorageChanged();
                }
            }
        }
    }
}