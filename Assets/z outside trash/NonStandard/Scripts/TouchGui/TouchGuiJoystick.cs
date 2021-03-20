using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using NonStandard.Inputs;

namespace NonStandard.TouchGui {
	public class TouchGuiJoystick : MonoBehaviour {
		[Tooltip("If true, make sure horizontal and vertical inputs never leave the boundary of a unit circle")]
		public bool normalizeAxisInput = false;
		protected bool inputNeedsNormalization = true;
		protected bool dragging = false;

		[Tooltip("Distance handle (knob) can be dragged from the center of the joystick. 2 is the most accurate value, but 4 'feels' better.")]
		public int handleDistance = 4;
		/// just a number that looks good
		protected static float stickUISpeed = 8;
		/// background of the joystick, this is the part of the joystick that recieves input
		protected Image bgImage;
		/// the "knob" part of the joystick, moves to provide feedback, does not receive input from the touch
		protected TouchGuiDrag joystickKnob;
		/// direction vector ouput from joystick, normalized if inputNeedsNormalization is true.
		[HideInInspector] public Vector2 outputVector;
		/// unormalized direction vector (has a magnitude), modified by Horizontal and Vertical properties
		[HideInInspector] public Vector2 inputVector;
		protected float currentStickDistance;
		protected Vector2 lastInputValues;

		public float Horizontal {
			get { return inputVector.x; }
			set {
				inputNeedsNormalization = normalizeAxisInput;
				inputVector.x = value;
			}
		}
		public float Vertical {
			get { return inputVector.y; }
			set {
				inputNeedsNormalization = normalizeAxisInput;
				inputVector.y = value;
			}
		}
	
		[System.Serializable]
		public class JoystickOutput {
			[System.Serializable]
			public class UnityEventFloat : UnityEngine.Events.UnityEvent<float> { }
			public UnityEventFloat onHorizontalChange, onVerticalChange;
			[Tooltip("Output of horizontal and vertical values will be in a range -1 to +1 multiplied by this value")]
			public Vector2 outputMultiplier;
			public void NotifyValue(float horizontal, float vertical) {
				onHorizontalChange.Invoke(horizontal * outputMultiplier.x);
				onVerticalChange.Invoke(vertical * outputMultiplier.y);
			}
		}
		public JoystickOutput joystickOutput = new JoystickOutput { outputMultiplier = Vector2.one };

		private void Start() {
			if(EventSystem.current == null) {
				AppInput.GetEventSystem();
			}
			if (GetComponent<Image>() == null) {
				Debug.LogError("There is no joystick image attached to this script.");
			}
			if (transform.GetChild(0).GetComponent<TouchGuiDrag>() == null) {
				Debug.LogError("There is no joystick handle image attached to this script.");
			}
			if (GetComponent<Image>() != null && transform.GetChild(0).GetComponent<TouchGuiDrag>() !=null) {
				bgImage = GetComponent<Image>();
				joystickKnob = transform.GetChild(0).GetComponent<TouchGuiDrag>();
				bgImage.rectTransform.SetAsLastSibling(); // ensures that this joystick will always render on top of other UI elements
				Vector2 idealPivot = new Vector2(0.5f, 0.5f);
				Vector2 offsetFromIdeal = (idealPivot - bgImage.rectTransform.pivot);
				offsetFromIdeal.Scale(bgImage.rectTransform.sizeDelta);
				bgImage.rectTransform.pivot = idealPivot;
				bgImage.rectTransform.anchoredPosition += offsetFromIdeal;
				joystickKnob.onDragBegin += () => dragging = true;
				joystickKnob.onDrag += OnDrag;
				joystickKnob.onDragRelease += () => {
					dragging = false;
					inputVector = Vector3.zero;
				};
			}
		}

		public void OnDrag() {
			Vector2 localPoint = joystickKnob.rectTransform.localPosition;
			localPoint.x = (localPoint.x / bgImage.rectTransform.sizeDelta.x);
			localPoint.y = (localPoint.y / bgImage.rectTransform.sizeDelta.y);
			inputVector = localPoint * handleDistance;
			inputNeedsNormalization = normalizeAxisInput;
		}

		void InputVectorCalculation() {
			outputVector = inputVector;
			//Debug.Log(currentStickDistance + " " +inputNeedsNormalization);
			if (inputNeedsNormalization) {
				currentStickDistance = inputVector.magnitude;
				if (currentStickDistance > 1) {
					outputVector /= currentStickDistance;
					inputNeedsNormalization = false;
				}
			}
		}

		public void Update() {
			if (EventSystem.current.currentSelectedGameObject != gameObject || joystickKnob.Dragged) {
				if (lastInputValues != inputVector) {
					InputVectorCalculation();
					joystickOutput.NotifyValue(outputVector.x, outputVector.y);
					lastInputValues = inputVector;
				}
				if (!dragging) {
					Vector3 currentPos = joystickKnob.rectTransform.localPosition;
					Vector3 targetPos = new Vector3(outputVector.x * bgImage.rectTransform.sizeDelta.x, outputVector.y * bgImage.rectTransform.sizeDelta.y);
					targetPos /= handleDistance;
					Vector3 delta = targetPos - currentPos;
					if(delta.sqrMagnitude < 1) {
						joystickKnob.rectTransform.anchoredPosition = targetPos;
					} else {
						joystickKnob.rectTransform.anchoredPosition = currentPos + delta * (Time.unscaledDeltaTime * stickUISpeed);
					}
				} else {
					joystickKnob.FollowDrag();
				}
			}
		}
	}
}