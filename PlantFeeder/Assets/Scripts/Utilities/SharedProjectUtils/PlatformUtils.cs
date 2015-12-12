using UnityEngine;
using System.Collections;

public static class PlatformUtils
{
	#if UNITY_WEBPLAYER
		public static string platformName = "_MOUSE";
	#elif UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
		public static string platformName = "_WIN";
	#elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX 
		public static string platformName = "_OSX";
	#endif
}
