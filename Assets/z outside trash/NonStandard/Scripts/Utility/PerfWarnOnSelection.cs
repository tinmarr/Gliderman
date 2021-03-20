using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NonStandard.Utility {
	public class PerfWarnOnSelection : MonoBehaviour {
#if UNITY_EDITOR
		public GameObject[] selectingSlowsRenderingInEditMode;
		bool gaveWarningAboutPerformanceWhileSelected = false;
		private int whichSelected;

		public int WhichSelected() {
			GameObject[] selections = Selection.gameObjects;
			for(int i = 0; i < selections.Length; ++i) {
				if (System.Array.IndexOf(selectingSlowsRenderingInEditMode, selections[i]) >= 0) return i;
			}
			return -1;
		}

		private void FixedUpdate() {
			if (EditorApplication.isPlaying && !gaveWarningAboutPerformanceWhileSelected && (whichSelected = WhichSelected()) >= 0) {
				Debug.LogWarning("UnityEditor Performance Warning: Selecting "+ selectingSlowsRenderingInEditMode [whichSelected].name +
					" during play mode may cause camera stutter, because graphics Update is synching to Unity Editor Inspector UI.");
				gaveWarningAboutPerformanceWhileSelected = true;
			}
		}
#endif
	}
}