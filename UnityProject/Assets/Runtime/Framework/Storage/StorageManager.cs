using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using DragonU3DSDK.Util;
// using DragonU3DSDK.Network.API;
// using DragonU3DSDK.Network.API.Protocol;
using System.Collections.Generic;

//*************************************************//
/*
 * Example
 * 必须且只能初始化一次
 * StorageManager.Instance.Init(new List<StorageBase>());
 * 
 * StorageCook cook = StorageManager.Instance.GetStorage<StorageCook>();
 * cook.Coins["island_1"] = 200;
 */
//*************************************************//

namespace DragonU3DSDK.Storage
{
    public class StorageManager : Manager<StorageManager>
    {
        const string storageKey = "StorageData";
        const string localVersionKey = "StorageVersion";
        const string remoteVersionAckKey = "RemoteStorageVersion";
        const string remoteVersionLocalKey = "RemoteVersionLocal";
        const string runOnceKey = "RunOnce";

        /// <summary>
        /// 配置被重建时调用<br/>
        /// 为了避免大量的字典查询，可以缓存 storage 配置到模块中，但在某些情况下缓存的数据需要被重新获取<br/>
        /// 比如登陆或者切换了 FaceBook 或者 Apple 账号，存档冲突等情况<br/>
        /// See <see cref="FromJson(string)"/>
        /// </summary>
        public static System.Action onRebuild;

        static Dictionary<System.Type, string> gType2Name = new Dictionary<System.Type, string>();

        Dictionary<string, StorageBase> storageMap;
        public T GetStorage<T>() where T : StorageBase
        {
            System.Type storage_type = typeof(T);
            if (!gType2Name.TryGetValue(storage_type, out string name))
            {
                name = storage_type.Name;
                gType2Name[storage_type] = name;
            }

            return (T)storageMap[name];            
        }

        float LocalTickTime
        {
            get;
            set;
        }

        float RemoteTickTime
        {
            get;
            set;
        }

        // 本地存档版本
        public ulong LocalVersion
        {
            get
            {
                return _localVersion;
            }
            set
            {
                _localVersion = value;
                //DebugUtil.Log("[storage] changed local_version to {0}", value);
            }
        }

        // 服务器存档版本
        public ulong RemoteVersionACK
        {
            get
            {
                return _remoteVersionACK;
            }
            set
            {
                _remoteVersionACK = value;
                DebugUtil.Log("[storage] changed remote_verion_ack to {0}", value);
                PlayerPrefs.SetString(remoteVersionAckKey, System.Convert.ToBase64String(RijndaelManager.Instance.EncryptStringToBytes(RemoteVersionACK.ToString())));
            }
        }

        public ulong RemoteVersionSYN
        {
            get
            {
                return _remoteVersionSYN;
            }
            set
            {
                _remoteVersionSYN = value;
                DebugUtil.Log("[storage] changed remote_verion_syn to {0}", value);
                PlayerPrefs.SetString(remoteVersionLocalKey, System.Convert.ToBase64String(RijndaelManager.Instance.EncryptStringToBytes(RemoteVersionSYN.ToString())));
            }
        }

        bool InConflict = false;
        System.Action<bool> conflictResolvedCallback;

        ulong lastSavedLocalVersion;
        // 强制同步，用于存储敏感数据
        public bool SyncForce
        {
            get;
            set;
        }

        public bool SyncForceRemote
        {
            get;
            set;
        }

        // 本地同步间隔
        float localInterval = 1.0f;
        public float LocalInterval
        {
            get
            {
                return localInterval;
            }
            set
            {
                if (value > 0.0f)
                {
                    localInterval = value;
                }
            }
        }

        float refreshIntervalMax = 64.0f;
        float refreshInterval = 1.0f;
        float refreshTick = 0.0f;

        // 远端同步间隔
        float remoteInterval = 30.0f;
        private ulong _remoteVersionACK;
        private ulong _remoteVersionSYN;
        private ulong _localVersion;

        public float RemoteInterval
        {
            get
            {
                return remoteInterval;
            }
            set
            {
                if (value > 0.0f)
                {
                    remoteInterval = value;
                }
            }
        }

        bool syncLock
        {
            get;
            set;
        }

        public bool Inited
        {
            get;
            private set;
        }

        public void SaveToLocal()
        {
            var storageCommon = GetStorage<StorageCommon>();
            if (storageCommon != null)
            {
                _localVersion -= 1;
                storageCommon.UpdatedAt = DeviceHelper.CurrentTimeMillis();
            }
            //fixed:ArgumentNullException: Value cannot be null. Parameter name: obj
            JsonSerializerSettings setting = new JsonSerializerSettings();
            setting.NullValueHandling = NullValueHandling.Ignore;
            string jsonData = JsonConvert.SerializeObject(storageMap,setting);
            byte[] encryptData = RijndaelManager.Instance.EncryptStringToBytes(jsonData);
            PlayerPrefs.SetString(storageKey, System.Convert.ToBase64String(encryptData));
            PlayerPrefs.SetString(localVersionKey, System.Convert.ToBase64String(RijndaelManager.Instance.EncryptStringToBytes(LocalVersion.ToString())));
            PlayerPrefs.SetString(remoteVersionAckKey, System.Convert.ToBase64String(RijndaelManager.Instance.EncryptStringToBytes(RemoteVersionACK.ToString())));
            PlayerPrefs.SetString(remoteVersionLocalKey, System.Convert.ToBase64String(RijndaelManager.Instance.EncryptStringToBytes(RemoteVersionSYN.ToString())));

            //DebugUtil.Log(" save local version : " + LocalVersion);
            //DebugUtil.Log(" save remote version ack: " + RemoteVersionACK);
            //DebugUtil.Log(" save remote version local: " + RemoteVersionSYN);
            //DebugUtil.Log(" save local storage json : " + jsonData);
            lastSavedLocalVersion = LocalVersion;
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(storageMap);
        }

        public void FromJson(string jsonData)
        {
            JObject jObj = JObject.Parse(jsonData);
            foreach (var type in storageMap.Keys)
            {
                var token = jObj[type];
                if (token == null)
                {
                    continue;
                }

                var str = token.ToString();
                JsonSerializerSettings setting = new JsonSerializerSettings();

                setting.NullValueHandling = NullValueHandling.Ignore;
                JsonConvert.PopulateObject(str, storageMap[type], setting);
            }

            onRebuild?.Invoke();
        }

        void ReadFromLocal()
        {
            // 读取本地存档
            string jsonData = "{}";
            if (PlayerPrefs.HasKey(storageKey))
            {
                byte[] encryptData = System.Convert.FromBase64String(PlayerPrefs.GetString(storageKey));
                jsonData = RijndaelManager.Instance.DecryptStringFromBytes(encryptData);
                DebugUtil.Log(" read storage json from local : " + jsonData);
                FromJson(jsonData);
            }
            else
            {
                DebugUtil.Log("No local storage data can read! ");
            }

            // 读取本地存档版本
            if (PlayerPrefs.HasKey(localVersionKey))
            {
                string strVersion = RijndaelManager.Instance.DecryptStringFromBytes(System.Convert.FromBase64String(PlayerPrefs.GetString(localVersionKey)));
                LocalVersion = ulong.Parse(strVersion);
                DebugUtil.Log(" read local version : " + LocalVersion);
            }
            else
            {
                LocalVersion = 0;
                DebugUtil.Log("No local storage version can read! ");
            }
            lastSavedLocalVersion = LocalVersion;

            // 读取远端档版本
            if (PlayerPrefs.HasKey(remoteVersionAckKey))
            {
                string strVersion = RijndaelManager.Instance.DecryptStringFromBytes(System.Convert.FromBase64String(PlayerPrefs.GetString(remoteVersionAckKey)));
                RemoteVersionACK = ulong.Parse(strVersion);
                DebugUtil.Log(" read remote version : " + RemoteVersionACK);
            }
            else
            {
                RemoteVersionACK = 0;
                DebugUtil.Log("No remote storage version can read! ");
            }

            if (PlayerPrefs.HasKey(remoteVersionLocalKey))
            {
                string strVersion = RijndaelManager.Instance.DecryptStringFromBytes(System.Convert.FromBase64String(PlayerPrefs.GetString(remoteVersionLocalKey)));
                RemoteVersionSYN = ulong.Parse(strVersion);
                DebugUtil.Log(" read last upload version : " + RemoteVersionSYN);
            }
            else
            {
                RemoteVersionSYN = 0;
                DebugUtil.Log("No remote storage version can read! ");
            }
        }

        #region sync_with_server
        void UploadProfile()
        {
            // if (!Account.AccountManager.Instance.HasLogin)
            // {
            //     return;
            // }
            if (syncLock)
            {
                return;
            }
            syncLock = true;

            if (RemoteVersionSYN <= RemoteVersionACK)
            {
                RemoteVersionSYN = LocalVersion;
            }

            string jsonData = JsonConvert.SerializeObject(storageMap);
            // var cUpdateProfile = new CUpdateProfile
            // {
            //     Profile = new Profile
            //     {
            //         Json = jsonData,
            //         Version = LocalVersion,
            //     },
            //     OldVersion = RemoteVersionACK,
            // };
            // DebugUtil.Log("start upload profile : " + cUpdateProfile.Profile.Json);
            // APIManager.Instance.Send(cUpdateProfile, (SUpdateProfile sUpdateProfile) =>
            // {
            //     DebugUtil.Log("upload profile successs, version  = " + sUpdateProfile.Profile.Version);
            //     if (sUpdateProfile.Profile.Version > RemoteVersionACK)
            //     {
            //         RemoteVersionACK = sUpdateProfile.Profile.Version;
            //     }
            //     syncLock = false;
            // }, (ErrorCode err, string msg, SUpdateProfile sUpdateProfile) =>
            // {
            //     DebugUtil.Log("upload profile error " + err + "   " + msg);
            //     if (err == ErrorCode.ProfileNotExistsError)
            //     {
            //         CreateProfile((result) =>
            //         {
            //             DebugUtil.Log("create profile when upload not exists result: " + result.ToString());
            //         });
            //         return;
            //     }
                
            //     if (err == ErrorCode.ProfileVersionConflictionError)
            //     {
            //         OnServerProfileConfict(sUpdateProfile.Profile);
            //     }
            //     syncLock = false;
            // });
            SaveToLocal();
        }

        public void GetOrCreateProfile(System.Action<bool> cb)
        {
            // if (!Account.AccountManager.Instance.HasLogin)
            // {
            //     if (cb != null)
            //     {
            //         cb.Invoke(false);
            //     }
            //     return;
            // }

            // syncLock = true;
            // CGetProfile getProfile = new CGetProfile
            // {
            //     OldVersion = RemoteVersionACK
            // };
            // APIManager.Instance.Send(getProfile, (SGetProfile sGetProfile) =>
            // {
            //     syncLock = false;
            //     cb?.Invoke(true);
            //     EventManager.Instance.Trigger<SDKEvents.ProfileFetchedEvent>().Trigger();

            // }, (errno, errmsg, resp) =>
            // {
            //     if (errno == ErrorCode.ProfileNotExistsError)
            //     {
            //         CreateProfile(cb);
            //         return;
            //     }

            //     syncLock = false;
            //     if (errno == ErrorCode.ProfileVersionConflictionError)
            //     {
            //         var sGetProfile = (SGetProfile)resp;
            //         OnServerProfileConfict(sGetProfile.Profile, cb);
            //     }
            //     else
            //     {
            //         cb?.Invoke(false);
            //     }
            // });
        }

        void CreateProfile(System.Action<bool> cb)
        {
            // CCreateProfile cCreateProfile = new CCreateProfile
            // {
            //     Profile = new Profile
            //     {
            //         Json = ToJson(),
            //         Version = LocalVersion,
            //     }
            // };

            // APIManager.Instance.Send(cCreateProfile, (SCreateProfile resp) =>
            // {
            //     syncLock = false;
            //     DebugUtil.Log("create profile success");
            //     RemoteVersionACK = cCreateProfile.Profile.Version;
            //     cb?.Invoke(true);
            //     EventManager.Instance.Trigger<SDKEvents.ProfileCreatedEvent>().Trigger();
            // }, (errno, errmsg, resp) =>
            // {
            //     syncLock = false;
            //     DebugUtil.LogError("create profile failed errno = {0} errmsg = {1}", errno, errmsg);
            //     cb?.Invoke(false);
            // });
        }

        // void OnServerProfileConfict(Profile serverProfile, System.Action<bool> callback = null)
        // {
        //     DebugUtil.LogWarning("profile conflict: remote_version={0} remote_version_ack={1} remote_version_local={2}, local_version={3} force={4}", serverProfile.Version, RemoteVersionACK, RemoteVersionSYN, LocalVersion, serverProfile.Force);

        //     if (callback != null)
        //     {
        //         conflictResolvedCallback = callback;
        //     }

        //     if (serverProfile.Force)
        //     {
        //         ResolveProfileConfict(serverProfile, true);
        //     }
        //     /*else if (RemoteVersionAck == 0)
        //     {
        //         ResolveProfileConfict(serverProfile, true);
        //     }
        //     */
        //     else if (serverProfile.Version == RemoteVersionACK)
        //     {
        //         RemoteVersionSYN = RemoteVersionACK;
        //     }
        //     else if (serverProfile.Version == RemoteVersionSYN)
        //     {
        //         RemoteVersionACK = RemoteVersionSYN;
        //     }
        //     else
        //     {
        //         InConflict = true;
        //         EventManager.Instance.Trigger<SDKEvents.ProfileConflictEvent>().Data(serverProfile).Trigger();
        //         Network.BI.BIManager.Instance.SendCommonGameEvent(BiEventCommon.Types.CommonGameEventType.ProfileConflict, serverProfile.Version.ToString(), RemoteVersionACK.ToString(), RemoteVersionSYN.ToString(), LocalVersion.ToString(), serverProfile.Force.ToString());
        //     }
        // }

        // public void ResolveProfileConfict(Profile serverProfile, bool useServer)
        // {
        //     if (useServer)
        //     {
        //         Clear();
        //         FromJson(serverProfile.Json);
        //         RemoteVersionACK = serverProfile.Version;
        //         RemoteVersionSYN = serverProfile.Version;
        //         LocalVersion = serverProfile.Version;

        //         var storageCommon = GetStorage<StorageCommon>();
        //         var clear = false;
        //         if (storageCommon != null && storageCommon.PlayerId == 0)
        //         {
        //             clear = true;
        //         }
        //         EventManager.Instance.Trigger<SDKEvents.ProfileReplacedEvent>().Data(clear).Trigger();
        //     }
        //     else
        //     {
        //         RemoteVersionACK = serverProfile.Version;
        //         RemoteVersionSYN = RemoteVersionACK;
        //         if (LocalVersion <= RemoteVersionACK)
        //         {
        //             LocalVersion = RemoteVersionACK + 1;
        //         }
        //     }
        //     SaveToLocal();
        //     EventManager.Instance.Trigger<SDKEvents.ProfileResolvedEvent>().Data(useServer).Trigger();

        //     if (conflictResolvedCallback != null)
        //     {
        //         conflictResolvedCallback.Invoke(true);
        //         conflictResolvedCallback = null;
        //     }
        //     InConflict = false;
        // }
        #endregion

        void Update()
        {
            if (!Inited)
                return;

            LocalTickTime += Time.deltaTime;
            if (SyncForce || (LocalTickTime > localInterval && LocalVersion > lastSavedLocalVersion))
            {
                SyncForce = false;
                LocalTickTime = 0.0f;
                SaveToLocal();
            }

            RemoteTickTime += Time.deltaTime;
            if ((RemoteTickTime > remoteInterval || SyncForceRemote)
                && !syncLock
                && !InConflict
                && LocalVersion > RemoteVersionACK)
                // && APIManager.Instance.HasNetwork
                // && Account.AccountManager.Instance.HasLogin)
            {
                RemoteTickTime = 0.0f;
                SyncForceRemote = false;
                UploadProfile();
            }

            refreshTick += Time.deltaTime;
            if (refreshTick > refreshInterval)
            {
                refreshTick = 0.0f;
                if (refreshInterval < refreshIntervalMax)
                {
                    refreshInterval *= 2.0f;
                }
                UpdateStorageCommon();
            }
        }

        public void Init(List<StorageBase> storages)
        {
            if (!Inited)
            {
                storageMap = new Dictionary<string, StorageBase>();
                foreach (var storage in storages)
                {
                    var type = storage.GetType().Name;
                    storageMap[type] = storage;
                }
                Inited = true;
                ReadFromLocal();
            }
            else
            {
                Debug.Assert(false, "Error Init Storage !!!");
            }
        }

        void UpdateStorageCommon()
        {
            var storageCommon = GetStorage<StorageCommon>();
            if (storageCommon == null)
            {
                return;
            }

            // storageCommon.Platform = (int)DeviceHelper.GetPlatform();

            // update adjust infos
            // var adjustPlugin = Dlugin.SDK.GetInstance().adjustPlugin;
            // if (adjustPlugin != null)
            // {
            //     if (string.IsNullOrEmpty(storageCommon.AdjustId)) {
            //         string adjustId = adjustPlugin.GetTrackeeID();
            //         if (!string.IsNullOrEmpty(adjustId))
            //         {
            //             storageCommon.AdjustId = adjustId;
            //         }
            //     }
            //     if (string.IsNullOrEmpty(storageCommon.InviteCode)) {
            //         string inviteCode = adjustPlugin.GetInviteCode();
            //         if (!string.IsNullOrEmpty(inviteCode))
            //         {
            //             storageCommon.InviteCode = inviteCode;
            //         }
            //     }
            //     if (string.IsNullOrEmpty(storageCommon.Idfa))
            //     {
            //         string idfa = adjustPlugin.GetIdfa();
            //         if (!string.IsNullOrEmpty(idfa))
            //         {
            //             storageCommon.Idfa = idfa;
            //         }
            //     }
            //     if (storageCommon.PlayerId > 0)
            //     {
            //         adjustPlugin.SetSessionParameter("playerId", storageCommon.PlayerId.ToString());
            //     }
            //     if (!string.IsNullOrEmpty(storageCommon.FirebaseInstanceId))
            //     {
            //         adjustPlugin.SetPushToken(storageCommon.FirebaseInstanceId);
            //     }
            // }

            if (string.IsNullOrEmpty(storageCommon.DeviceType))
            {
                // storageCommon.DeviceType = DeviceHelper.GetDeviceType();
            }

            if (string.IsNullOrEmpty(storageCommon.DeviceModel))
            {
                storageCommon.DeviceModel = DeviceHelper.GetDeviceModel();
            }
            if (string.IsNullOrEmpty(storageCommon.DeviceMemory))
            {
                storageCommon.DeviceMemory = DeviceHelper.GetTotalMemory().ToString();
            }

            string nativeVersion = "";//DragonNativeBridge.GetVersionCode().ToString();
            if (!string.IsNullOrEmpty(nativeVersion))
            {
                // clear login status when native upgrade
                if (!string.IsNullOrEmpty(storageCommon.NaviveVersion) && nativeVersion != storageCommon.NaviveVersion)
                {
                    //Account.AccountManager.Instance.Clear();
                    EventManager.Instance.Trigger<SDKEvents.NativeVersionChanged>().Data(storageCommon.NaviveVersion, nativeVersion).Trigger();
                }
                storageCommon.NaviveVersion = nativeVersion;
            }
        }

        public bool RunOnce(System.Action cb)
        {
            if (PlayerPrefs.HasKey(runOnceKey))
            {
                return false;
            }

            cb();
            PlayerPrefs.SetString(runOnceKey, true.ToString());
            return true;
        }

        void Clear()
        {
            foreach (var key in storageMap.Keys)
            {
                storageMap[key].Clear();
            }
        }

        public void ClearAll()
        {
            // TODO
            PlayerPrefs.DeleteAll();
            Clear();
            // var serverProfile = new Profile
            // {
            //     Json = JsonConvert.SerializeObject(storageMap),
            //     Version = 0,
            //     Force = true
            // };
            // OnServerProfileConfict(serverProfile);
        }
    }
}