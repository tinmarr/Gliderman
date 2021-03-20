using System.Collections.Generic;
using UnityEngine;

namespace NonStandard.TouchGui {
	public class TouchGuiExclusiveInput : MonoBehaviour {
		private static Dictionary<string, List<TouchGuiExclusiveInput>> exclusiveTouchGroups =
			new Dictionary<string, List<TouchGuiExclusiveInput>>();

		public string groupName;
		[HideInInspector] public TouchColliderSensitive target;

		public static bool AddToGroup(string group, TouchGuiExclusiveInput listener) {
			List<TouchGuiExclusiveInput> listing;
			if (!exclusiveTouchGroups.TryGetValue(group, out listing)) {
				listing = new List<TouchGuiExclusiveInput>();
				exclusiveTouchGroups[group] = listing;
			}
			if (listing.IndexOf(listener) < 0) {
				listing.Add(listener);
				return true;
			}
			return false;
		}

		public void SetTouchColliderEnabled(bool enabled) {
			target.enabled = enabled;
			//target.gameObject.SetActive(enabled);
		}

		public static void ActivateAll(string group, bool activate) {
			List<TouchGuiExclusiveInput> listing;
			if (exclusiveTouchGroups.TryGetValue(group, out listing)) {
				for (int i = 0; i < listing.Count; ++i) {
					TouchGuiExclusiveInput e = listing[i];
					e.SetTouchColliderEnabled(activate);
				}
			}
		}

		public void ExcludeOthers() {
			ActivateAll(groupName, false);
			SetTouchColliderEnabled(true);
		}
		public void ReactivateAll() {
			ActivateAll(groupName, true);
		}

		void Start() {
			AddToGroup(groupName, this);
			target = GetComponent<TouchColliderSensitive>();
			target.onDragBegin += ExcludeOthers;
			target.onDragRelease += ReactivateAll;
		}
	}
}