using System;
using System.Collections.Specialized;
using DragonU3DSDK.Asset;

namespace DragonU3DSDK.SDKEvents
{
    // public class ProfileConflictEvent : Subject<ProfileConflictEvent>
    // {
    //     public Network.API.Protocol.Profile ServerProfile;
    //     public ProfileConflictEvent Data(Network.API.Protocol.Profile profile)
    //     {
    //         ServerProfile = profile;
    //         return this;
    //     }
    // }

    public class ConnectResServerSuccessEvent : Subject<ConnectResServerSuccessEvent>
    {
    }

    public class ConnectResServerFailEvent : Subject<ConnectResServerFailEvent>
    {
        public string Error;
        public ConnectResServerFailEvent Data(string error)
        {
            Error = error;
            return this;
        }
    }

    public class LoginEvent: Subject<LoginEvent>
    {
    }
    public class ProfileReplacedEvent: Subject<ProfileReplacedEvent>
    {
        public bool clear;
        public ProfileReplacedEvent Data(bool clear)
        {
            this.clear = clear;
            return this;
        }
    }
    public class ProfileCreatedEvent: Subject<ProfileCreatedEvent>
    {
    }

    public class ProfileFetchedEvent: Subject<ProfileFetchedEvent>
    {
    }

    public class ProfileResolvedEvent: Subject<ProfileResolvedEvent>
    {
        public bool server;
        public ProfileResolvedEvent Data(bool server)
        {
            this.server = server;
            return this;
        }
    }

    public class AccountLoginOtherDeviceEvent : Subject<AccountLoginOtherDeviceEvent>
    {
        public AccountLoginOtherDeviceEvent Data()
        {
            return this;
        }
    }

    public class AdsLoadFailEvent : Subject<AdsLoadFailEvent>
    {
        public AdsLoadFailEvent Data()
        {
            return this;
        }
    }

    public class AdsCloseEvent : Subject<AdsCloseEvent>
    {
        public AdsCloseEvent Data()
        {
            return this;
        }
    }

    public class CheckUpdateEvent: Subject<CheckUpdateEvent>
    {
        public enum CheckUpdateEventType
        {
            CHECK_UPDATE_EVENT_TYPE_START = 0,
            CHECK_UPDATE_EVENT_TYPE_FINISH = 1,
            CHECK_UPDATE_EVENT_TYPE_FAILURE = 2,
        };

        public CheckUpdateEventType checkUpdateEventType;
        public AppUpdateResult updateResult;
        public string errno;
        
        public CheckUpdateEvent Data(CheckUpdateEventType checkUpdateEventType, AppUpdateResult result, string errno = null)
        {
            this.checkUpdateEventType = checkUpdateEventType;
            this.updateResult = result;
            this.errno = errno;
            return this;
        }
        
        
    }

    public class AdjustIdUpdatedEvent: Subject<AdjustIdUpdatedEvent>
    {
    }

    public class DownloadFileEvent: Subject<DownloadFileEvent>
    {
        public string name;
        public string stage;
        public string data;
        public int bytesDownloaded;

        public DownloadFileEvent Data(string name, string stage, int bytesDownloaded, string data = "")
        {
            this.name = name;
            this.stage = stage;
            this.bytesDownloaded = bytesDownloaded;
            this.data = data;
            return this;
        }
    }

    public class DeepLinkEvent: Subject<DeepLinkEvent>
    {
        public string route;
        public string title;
        public string content;
        public NameValueCollection rawData;

        public DeepLinkEvent Data(NameValueCollection nvc)
        {
            if(nvc == null)
            {
                return this;
            }

            foreach(string key in nvc.AllKeys)
            {
                if (key.Equals("route"))
                {
                    this.route = nvc.Get(key);
                    continue;
                }

                if (key.Equals("title"))
                {
                    this.title = nvc.Get(key);
                    continue;
                }

                if (key.Equals("content"))
                {
                    this.content = nvc.Get(key);
                    continue;
                }
            }

            this.rawData = nvc;

            return this;
        }
    }

    public class NativeVersionChanged: Subject<NativeVersionChanged>
    {
        public string OldVersion;
        public string NewVersion;

        public NativeVersionChanged Data(string oldVersion, string newVersion)
        {
            OldVersion = oldVersion;
            NewVersion = newVersion;
            return this;
        }
    }

    public class SocketIOConnected: Subject<SocketIOConnected>
    {

    }

    public class SocketIODisconnected : Subject<SocketIODisconnected>
    {

    }
    public class SocketIOError : Subject<SocketIOError>
    {

    }

    public class FirebaseInstanceIdReceived : Subject<FirebaseInstanceIdReceived>
    {
        public string FirebaseInstanceId;
        public FirebaseInstanceIdReceived Data(string firebaseInstanceId)
        {
            FirebaseInstanceId = firebaseInstanceId;
            return this;
        }
    }
    public class IAPInitialized : Subject<IAPInitialized>
    {
    }

    public class ReauthFacebookDataAccessPopEvent : Subject<ReauthFacebookDataAccessPopEvent>
    {
    }

    public class ReauthFacebookDataAccessSuccessEvent : Subject<ReauthFacebookDataAccessSuccessEvent>
    {
    }

    public class ReauthFacebookDataAccessFailureEvent : Subject<ReauthFacebookDataAccessFailureEvent>
    {
    }

    public class ReauthFacebookAccessTokenComfirmEvent : Subject<ReauthFacebookAccessTokenComfirmEvent>
    {
    }

    public class ReauthFacebookAccessTokenPopEvent : Subject<ReauthFacebookAccessTokenPopEvent>
    {
    }

    public class ReauthFacebookAccessTokenSuccessEvent : Subject<ReauthFacebookAccessTokenSuccessEvent>
    {
    }

    public class ReauthFacebookAccessTokenFailureEvent : Subject<ReauthFacebookAccessTokenFailureEvent>
    {
    }

    public class DiskFullEvent : Subject<DiskFullEvent>
    {
    }

    public class AssetCheckClearEvent : Subject<AssetCheckClearEvent>
    {
    }
}
