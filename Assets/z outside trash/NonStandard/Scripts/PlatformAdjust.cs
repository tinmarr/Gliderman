using UnityEngine;
using UnityEngine.Events;

namespace NonStandard {
	public class PlatformAdjust : MonoBehaviour {
#if UNITY_EDITOR
		public UnityEvent onUnityEditorStart;
#endif
#if UNITY_EDITOR || UNITY_WEBPLAYER
		public UnityEvent onWebplayerStart;
#endif
#if UNITY_EDITOR || UNITY_ANDROID || UNITY_IPHONE
		public UnityEvent onMobileStart;
#endif

		void Start() {
#if UNITY_EDITOR
			onUnityEditorStart.Invoke();
#endif
#if UNITY_WEBPLAYER
			onWebplayerStart.Invoke();
#endif
#if UNITY_ANDROID || UNITY_IPHONE
			onMobileStart.Invoke();
#endif
		}

		public void Quit() {
#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_WEBPLAYER
			Application.OpenURL(webplayerQuitURL);
#else
			Application.Quit();
#endif
		}
	}
}