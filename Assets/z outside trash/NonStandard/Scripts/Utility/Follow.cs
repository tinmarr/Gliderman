using UnityEngine;

namespace NonStandard.Utility {
	public class Follow : MonoBehaviour {
		Transform self;
		public Transform whoToFollow;
		public bool followPosition = true;
		public bool followRotation = false;
		public Vector3 positionOffset;
		[Tooltip("meters per second, 0 for instant copy")]
		public float smoothPositionFollow = 0;
		[Tooltip("degrees per second, 0 for instant copy")]
		public float smoothRotationFollow = 360;
		private void Start() { self = transform; }
		public void DoFollow() {
			if (!whoToFollow) return;
			if (followPosition) {
				if (smoothPositionFollow == 0) {
					self.position = whoToFollow.position + positionOffset;
				} else {
					self.position = Vector3.MoveTowards(
						self.position, whoToFollow.position + positionOffset,
						smoothPositionFollow * Time.deltaTime);
				}
			}
			if (followRotation) {
				if (smoothRotationFollow == 0) {
					self.rotation = whoToFollow.rotation;
				} else {
					self.rotation = Quaternion.RotateTowards(
						self.rotation, whoToFollow.rotation,
						smoothRotationFollow * Time.deltaTime);
				}
			}
		}
		void FixedUpdate() { DoFollow(); }
	}
}