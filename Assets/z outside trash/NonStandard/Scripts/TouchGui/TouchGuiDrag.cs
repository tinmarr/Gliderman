using UnityEngine;

namespace NonStandard.TouchGui {
	public class TouchGuiDrag : TouchColliderSensitive {
		public bool Dragged { get; protected set; }
		protected bool surpressDragFollow = false;
		[HideInInspector] public RectTransform rectTransform;
		Vector2 delta;
		int fingerId;

		public override void Start() {
			base.Start();
			rectTransform = GetComponent<RectTransform>();
		}

		public override bool PressDown(TouchCollider tc) {
			if (Dragged) { Debug.Log("ignored touch before drag finished"); return false; }
			fingerId = triggeringCollider.touch.fingerId;
			Dragged = true;
			delta = (Vector2)rectTransform.position - triggeringCollider.touch.position;
			return base.PressDown(tc);
		}

		public void FollowDrag() {
			if (surpressDragFollow) return;
			TouchCollider tc = TouchGuiSystem.Instance().GetTouch(fingerId);
			FollowDragInternal(tc);
		}
		public void FollowDragInternal(TouchCollider tc) {
			if (!Dragged || surpressDragFollow || tc == null) { return; }
			rectTransform.position = tc.touch.position + delta;
		}

		public override bool Hold(TouchCollider tc) {
			FollowDragInternal(tc);
			return base.Hold(tc); ;
		}

		public override bool Release(TouchCollider tc) {
			FollowDragInternal(tc);
			if (tc == null || tc.touch.phase == TouchPhase.Ended || tc.touch.phase == TouchPhase.Canceled) {
				Dragged = false;
				return base.Release(tc);
			}
			return false;
		}
	}
}