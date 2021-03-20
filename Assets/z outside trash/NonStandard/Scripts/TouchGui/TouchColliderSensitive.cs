using System;
using UnityEngine;
using UnityEngine.UI;

namespace NonStandard.TouchGui
{
	public class TouchColliderSensitive : MonoBehaviour
	{
		public Image image;
		[HideInInspector, SerializeField] protected RawImage outlineImage;
		protected Color originalColor;
		protected Collider2D c2d;

		public Action onDragBegin;
		public Action onDrag;
		public Action onDragRelease;

		protected TouchCollider triggeringCollider;

		public Color activeColor = Color.white;
		/// <summary>
		/// if true, will attempt to create a RawImage object drawing a picture of the current collider
		/// </summary>
		public bool showOutlineCollider;
		/// <summary>
		/// if true, will trigger Press if a touch passes over. if false, will only trigger press if press starts here.
		/// </summary>
		public bool anyTouchTrigger = false;
		protected bool _pressed = false;

		void OnValidate() {
			if (outlineImage != null) {
				outlineImage.enabled = showOutlineCollider;
			}
			if (!GenerateOutlineIfNeeded()) {
				RenderOutlineIfPossible();
			}
			if (outlineImage != null) {
				outlineImage.color = activeColor;
			}
		}

		public bool GenerateOutlineIfNeeded() {
			if (outlineImage == null && showOutlineCollider) {
				GenerateOutlineImage();
				RenderOutlineIfPossible();
				return true;
			}
			return false;
		}

		public RawImage GenerateOutlineImage() {
			GameObject outlineObj = new GameObject("outline");
			Transform t = outlineObj.transform;
			t.SetParent(transform);
			t.localPosition = Vector3.zero;
			RawImage img = outlineObj.AddComponent<RawImage>();
			if (c2d == null) { c2d = GetComponent<Collider2D>(); }
			RectTransform rt = outlineObj.GetComponent<RectTransform>();
			rt.anchorMax = Vector2.one;
			rt.anchorMin = rt.offsetMin = rt.offsetMax = Vector2.zero;
			img.color = activeColor;
			img.uvRect = new Rect(0.5f, 0.5f, 1, 1);
			outlineImage = img;
			return img;
		}

		public bool DoesOverlap(Vector3 point) {
			return c2d.OverlapPoint(point);
		}

		public void SetShowPressed(bool show) {
			if (image != null) { image.color = show ? activeColor : originalColor; }
		}
		public void ShowPressed() { SetShowPressed(true); }
		public void HidePressed() { SetShowPressed(false); }

		/// <param name="tc"></param>
		/// <returns>false if the TouchCollider did not actually press</returns>
		public virtual bool PressDown(TouchCollider tc) {
			ShowPressed();
			if (onDragBegin != null) { onDragBegin.Invoke(); }
			return true;
		}
		/// <param name="tc"></param>
		/// <returns>false if the TouchCollider is not actually holding</returns>
		public virtual bool Hold(TouchCollider tc) {
			if (onDrag != null) { onDrag.Invoke(); }
			return true;
		}
		/// <param name="tc"></param>
		/// <returns>false if the TouchCollider did not actually release</returns>
		public virtual bool Release(TouchCollider tc) {
			HidePressed();
			if (onDragRelease != null) { onDragRelease.Invoke(); }
			return true;
		}

		public bool Pressed {
			protected set {
				if (value == true) {
					if (!_pressed) {
						if (anyTouchTrigger || triggeringCollider.touch.phase == TouchPhase.Began) {
							//Debug.Log("Pressed");
							if(!PressDown(triggeringCollider)) return;
						} else {
							return;
						}
					} else {
						//Debug.Log("Held");
						if(!Hold(triggeringCollider)) return;
					}
				} else if (value == false && _pressed == true) {
					//Debug.Log("Released");
					if(!Release(triggeringCollider)) return;
				}
				_pressed = value;
			}
			get { return _pressed; }
		}

		private TouchCollider Notify(Collider2D c) {
			triggeringCollider = c.GetComponent<TouchCollider>();
			if (triggeringCollider == null) {
				Debug.LogError(c.gameObject + " is touching "+this);
			} else {
				triggeringCollider.touched = this;
			}
			return triggeringCollider;
		}

		private void OnTriggerEnter2D(Collider2D other) {
			if (!enabled) return;
			if (Notify(other)) {
				Pressed = true; // calls DoPress
			}
		}
		private void OnTriggerExit2D(Collider2D other) {
			if (!enabled) return;
			if (Notify(other)) {
				Pressed = false; // calls DoRelease
			}
		}
		private void OnTriggerStay2D(Collider2D other) {
			if (!enabled) return;
			if (Notify(other)) {
				Pressed = true; // calls DoHold
			}
		}

		public void RenderOutlineIfPossible() {
			if (outlineImage == null) { outlineImage = GetComponent<RawImage>(); }
			if (outlineImage == null || !outlineImage.enabled) return;
			if (outlineImage.color.a == 0) { Color c = outlineImage.color; c.a = 1; outlineImage.color = c; }
			RectTransform rt = GetComponent<RectTransform>();
			Rect r = rt.rect;
			Texture2D img = new Texture2D((int)r.width, (int)r.height);
			img.SetPixels(0, 0, (int)r.width, (int)r.height, new Color[(int)(r.width * r.height)]); // set pixels to the default color, which is clear
			if (c2d == null) { c2d = GetComponent<Collider2D>(); }
			PolygonCollider2D polygon = c2d as PolygonCollider2D;
			CircleCollider2D circle = c2d as CircleCollider2D;
			BoxCollider2D box = c2d as BoxCollider2D;
			if (polygon != null) {
				Vector2 off = polygon.offset;
				for (int i = 0; i < polygon.points.Length; ++i) {
					int nextI = (i + 1) % polygon.points.Length;
					Vector2 p0 = polygon.points[i] + off;
					Vector2 p1 = polygon.points[nextI] + off;
					NonStandard.Lines.DrawLine(img, (int)p0.x, (int)p0.y, (int)p1.x, (int)p1.y, Color.white);
				}
			}
			if (circle != null) {
				const int circlePoints = 128;
				float angle = Mathf.PI * 2;
				float anglePerSection = angle / circlePoints;
				float angleCursor = 0;
				float rad = circle.radius - 1;
				Vector2 off = circle.offset;
				for (int i = 0; i < circlePoints; ++i) {
					Vector2 p0 = new Vector2(Mathf.Cos(angleCursor), Mathf.Sin(angleCursor)) * rad + off;
					angleCursor += anglePerSection;
					Vector2 p1 = new Vector2(Mathf.Cos(angleCursor), Mathf.Sin(angleCursor)) * rad + off;
					NonStandard.Lines.DrawLine(img, (int)p0.x, (int)p0.y, (int)p1.x, (int)p1.y, Color.white);
				}
			}
			if (box != null) {
				float w = box.size.x, h = box.size.y, w2 = w / 2, h2 = h / 2;
				Vector2[] corners = new Vector2[] {
				new Vector2(-w2, -h2), new Vector2(-w2, h2-1), new Vector2(w2-1, h2-1), new Vector2(w2-1, -h2)
			};
				Vector2 off = box.offset;
				for (int i = 0; i < corners.Length; ++i) {
					int nextI = (i + 1) % corners.Length;
					Vector2 p0 = corners[i] + off;
					Vector2 p1 = corners[nextI] + off;
					NonStandard.Lines.DrawLine(img, (int)p0.x, (int)p0.y, (int)p1.x, (int)p1.y, Color.white);
				}
			}
			img.Apply();
			outlineImage.texture = img;
		}

		// Use this for initialization
		public virtual void Start() {
			gameObject.layer = LayerMask.NameToLayer("UI");
			TouchGuiSystem.Instance();
			c2d = GetComponent<Collider2D>();
			//if(buttonImage == null) { buttonImage = GetComponent<Image>(); }
			if (c2d == null) { Debug.LogWarning("missing Collider2D, is this intentional?"); } else if (!c2d.isTrigger) { Debug.LogWarning(name + " collider is not a trigger, is this intentional?"); }
			if (image != null) {
				originalColor = image.color;
			}
			GenerateOutlineIfNeeded();
		}

	}
}