using System.Collections.Generic;
using UnityEngine;

namespace NonStandard.TouchGui
{
	public class TouchGuiSystem : MonoBehaviour
	{
		public Touch[] _currentTouches = null;
		private Vector3 mousePosition;
		public TouchCollider prefab_touchCollider;
		public Transform touchCanvas;

		/// <summary>
		/// all of the colliders for current touches
		/// </summary>
		private List<TouchCollider> touchColliders = new List<TouchCollider>();

		private static TouchGuiSystem s_instance;
		public static TouchGuiSystem Instance() {
			if (s_instance != null) return s_instance;
			s_instance = NonStandard.Inputs.AppInput.GetEventSystem().gameObject.AddComponent<TouchGuiSystem>();
			return s_instance;
		}
		public int countTouch;
		private Touch[] GetCurrentMouseTouches() {
			Touch[] mouseTouches = null;
			bool isDown = Input.GetMouseButtonDown(0);
			bool isButton = Input.GetMouseButton(0);
			if (isButton && (_currentTouches == null || _currentTouches.Length == 0)) {
				isDown = true;
			}
			if (isButton || isDown) {
				if (isDown) { mousePosition = Input.mousePosition; }
				bool isStationary = mousePosition == Input.mousePosition;
				mouseTouches = new Touch[] { new Touch { fingerId = 0, rawPosition = Input.mousePosition, position = Input.mousePosition,
				deltaPosition = Input.mousePosition - mousePosition,
				phase = (isDown) ? TouchPhase.Began : ((isStationary) ? TouchPhase.Stationary : TouchPhase.Moved),
			}};
				mousePosition = Input.mousePosition;
			}
			return mouseTouches;
		}

		public TouchCollider GetTouch(int fingerId) {
			for (int i = 0; i < touchColliders.Count; ++i) {
				TouchCollider tc = touchColliders[i];
				if (tc.touch.phase < TouchPhase.Ended && tc.touch.fingerId == fingerId) {
					return tc;
				}
			}
			return null;
		}

		private static float ManhattanDistance(Vector3 a, Vector3 b) {
			return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
		}
		private struct TouchColliderDist {
			public TouchCollider tc;
			public float dist;
		}
		public static int FindIndexOfSmallest<TYPE>(IList<TYPE> list, System.Func<TYPE,float> valueFunc) {
			if (list == null || list.Count <= 0) return -1;
			int bestIndex = 0;
			float bestValue = valueFunc.Invoke(list[0]);
			for(int i = 1; i < list.Count; ++i) {
				float v = valueFunc.Invoke(list[i]);
				if(v < bestValue) {
					bestValue = v;
					bestIndex = i;
				}
			}
			return bestIndex;
		}
		public void SortTouchCollidersToMatchTouchArray(Touch[] touches) {
			// figure out which touch collider is closest to which touch
			List<TouchColliderDist>[] sortPerTouch = new List<TouchColliderDist>[touches.Length];
			for (int i = 0; i < touches.Length; ++i) {
				Vector2 p = touches[i].position;
				List<TouchColliderDist> order = new List<TouchColliderDist>(touchColliders.Count);
				for (int c = 0; c < touchColliders.Count; ++c) {
					order.Add(new TouchColliderDist {
						tc = touchColliders[c],
						dist = ManhattanDistance(p, touchColliders[c].transform.position)
					});
				}
				order.Sort((a, b) => a.dist.CompareTo(b.dist));
				sortPerTouch[i] = order;
			}
			// find the best matches for each touch, and match those up with the touch, removing them as options for other touches
			int count = Mathf.Min(touches.Length, touchColliders.Count);
			for (int i = 0; i < count; ++i) {
				int bestIndex = FindIndexOfSmallest(sortPerTouch, sortList => sortList[0].dist);
				TouchCollider claimedTc = sortPerTouch[bestIndex][0].tc;
				System.Array.ForEach(sortPerTouch, sortList => sortList.RemoveAt(sortList.FindIndex(e => e.tc == claimedTc)));
				touchColliders[i] = claimedTc;
			}
			// if not all the touch colliders were assigned to a touch, take the remainder and put them back in the touch collider list in any order really (they're about to be deactivated)
			if (sortPerTouch[0].Count > 0) {
				List<TouchColliderDist> remainder = sortPerTouch[0];
				for (int i = touches.Length; i < touchColliders.Count; ++i) {
					int lastOne = remainder.Count - 1;
					touchColliders[i] = remainder[lastOne].tc;
					remainder.RemoveAt(lastOne);
				}
			}
		}
		public Touch[] GetCurrentTouches() {
			Touch[] touches;
			// prioritize actual touch events over mouse events
			if (Input.touchCount != 0) {
				touches = Input.touches;
				if(touches.Length != touchColliders.Count) {
					SortTouchCollidersToMatchTouchArray(touches);
				}
			} else {
				// don't generate mouse touch events more than once per update
				touches = GetCurrentMouseTouches();
			}
			return touches;
		}

		public void UpdateTouchCollisionModels(Touch[] touches, bool touchCollidersChangedState) {
			// if a is no longer valid, we mark it that way manually
			if (touchCollidersChangedState) {
				int index = touches != null ? touches.Length : 0;
				int len = Mathf.Min(index, touchColliders.Count);
				for(int i = 0; i < len; ++i) {
					touchColliders[i].MarkValid(true);
				}
				for (int i = index; i < touchColliders.Count; ++i) {
					touchColliders[i].MarkValid(false);
				}
			}
			// move the touch colliders to match given locations
			if (touches != null) {
				for (int i = 0; i < touches.Length; ++i) {
					UpdateTouch(i, touches[i]);
				}
			}
		}

		public TouchCollider CreateTouchCollider() {
			if (prefab_touchCollider == null) {
				GameObject touchObj = new GameObject("touch");
				touchObj.layer = LayerMask.NameToLayer("UI");
				touchObj.AddComponent<CircleCollider2D>();
				Rigidbody2D r2d = touchObj.AddComponent<Rigidbody2D>();
				r2d.bodyType = RigidbodyType2D.Kinematic;
				prefab_touchCollider = touchObj.AddComponent<TouchCollider>();
				prefab_touchCollider.MarkValid(false);
			}
			GameObject go = Instantiate(prefab_touchCollider.gameObject);
			return go.GetComponent<TouchCollider>();
		}
		public void UpdateTouch(int index, Touch t) {
			if (touchColliders.Count <= index) {
				TouchCollider tc = CreateTouchCollider();
				tc.name = "touch " + index;
				tc.transform.SetParent(touchCanvas);
				touchColliders.Add(tc);
			}
			TouchCollider touch = touchColliders[index];
			touch.touch = t;
			touch.MarkValid(true);
			touch.transform.position = t.position;
		}

		void Start() {
			if (touchCanvas == null) { touchCanvas = transform; }
		}

		void FixedUpdate() {
			_currentTouches = GetCurrentTouches();
			int lastTouchCount = countTouch;
			countTouch = _currentTouches == null ? 0 : _currentTouches.Length;
			UpdateTouchCollisionModels(_currentTouches, lastTouchCount != countTouch);
		}
	}
}