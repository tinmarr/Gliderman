using UnityEngine;

namespace NonStandard.TouchGui {
	public class TouchCollider : MonoBehaviour {
		/// <summary>
		/// the Unity Touch structure
		/// </summary>
		public Touch touch;
		/// <summary>
		/// what TouchColliderSensitive object was touched
		/// </summary>
		public TouchColliderSensitive touched;
		public static string Label(Touch t) {
			return "fid: " + t.fingerId + ", t#: " + t.tapCount + ", phase: " + t.phase + "\n" +
				"dt:" + t.deltaTime + ", dp:" + t.deltaPosition + ", rawP: " + t.rawPosition + ", P:" + t.position + "\n" +
				"alA: " + t.altitudeAngle + ", azA:" + t.azimuthAngle + "\n" +
				"press: " + t.pressure + "/" + t.maximumPossiblePressure + ", rad:" + t.radius + ", radV:" + t.radiusVariance + "\n" +
				"type: " + t.type;
		}

		private static Vector3 hiddenLocation = new Vector3(-10000, -10000);

		public void MarkValid(bool valid) {
			if (valid) {
				gameObject.SetActive(true);
			} else {
				transform.position = hiddenLocation;
				touch.phase = TouchPhase.Ended;
				NonStandard.Clock.setTimeoutRealtime(Deactivate, 0);
			}
		}
		private void Deactivate() {
			if (touch.phase != TouchPhase.Ended) return;
			touch.phase = TouchPhase.Canceled;
			if (touched != null) {
				touched.Release(this);
			}
			touched = null;
			gameObject.SetActive(false);
		}
	}
}