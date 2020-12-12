namespace OhMyFramework.Utils{
	public class RunOSType{
		public static bool IsAndroid{
			get{
			bool value = false;
#if UNITY_ANDROID
			value = true;
#endif
			return value;
			}
		}
		public static bool IsEditor{
			get{
			bool value = false;
#if UNITY_EDITOR
			value = true;
#endif
			return value;
			}
		}
		public static bool IsIOS{
			get{
			bool value = false;
#if UNITY_IOS
			value = true;
#endif
			return value;
			}
		}
	}
}