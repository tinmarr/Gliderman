using UnityEngine;

namespace NonStandard.Character {
	public class FaceMouse : MonoBehaviour
	{
		public Camera _camera;
		public Transform bodyTransform;
		public float FaceX { get; set; }
		public float FaceY { get; set; }
		public float lookSpeed = 360;

	#if UNITY_EDITOR
		/// called when created by Unity Editor
		void Reset() {
			if (_camera == null) { _camera = GetComponent<Camera>(); }
			if (_camera == null) { _camera = Camera.main; }
			if (_camera == null) { _camera = FindObjectOfType<Camera>(); }
		}
	#endif

		[HideInInspector] public RaycastHit raycastHit;
		[HideInInspector] public Ray ray;
		[HideInInspector] public Vector3 lookingAt;
		[Tooltip("FaceMouse even while moving. Allows head to spin backwards around body in certain circumstances...")]
		public bool followEvenWhileMoving;
		public bool allowCreepyLookAngles = false;
		/// <summary>
		/// if there are no look angle constraints, this is how this object should be looking
		/// </summary>
		private Quaternion calculatedIdealLookRotation;

		public void LookAtMouse(Vector3 screenCoordinate)
		{
			if (UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject != null) return; // ignore user interface
			ray = _camera.ScreenPointToRay(screenCoordinate);
			if (Physics.Raycast(ray, out raycastHit)) {
				lookingAt = raycastHit.point;
				//NS.Lines.Make("ray "+name).Arrow(transform.position, raycastHit.point, Color.red);
			} else {
				lookingAt = ray.origin;// + ray.direction;
			}
			Look();
		}

		/// sets lookingAt and adjusts direction of transform, does not adjust ray or raycastHit
		public void ForceLookAt(Vector3 position)
		{
			lookingAt = position;
			Look();
		}

		void Look()
		{
			Vector3 dir = (lookingAt - transform.position);
			dir.Normalize();
			if (dir != Vector3.zero) {
				Quaternion idealLook = Quaternion.LookRotation(dir);
				if (!allowCreepyLookAngles) {
					// prevent up-and-down angles from being too extreme
					Vector3 euler = idealLook.eulerAngles;
					if (euler.x < -180) { euler.x += 360; }
					if (euler.x > 180) { euler.x -= 360; }
					euler.x = Mathf.Clamp(euler.x, -45, 45);
					idealLook = Quaternion.Euler(euler);
				}
				calculatedIdealLookRotation = Quaternion.RotateTowards(transform.rotation, idealLook, Time.deltaTime * lookSpeed);
				transform.rotation = calculatedIdealLookRotation;
			}
		}

		public static float NormalizeAngle(float a) {
			if (a < -180) { a += 360; }
			if (a > 180) { a -= 360; }
			return a;
		}
		public static Vector3 NormalizeAngle(Vector3 euler) {
			for(int i = 0; i < 3; ++i) { euler[i] = NormalizeAngle(euler[i]); }
			return euler;
		}
		void TurnAccordingToJoystick(Vector3 joystickFace)
		{
			Vector3 f = (_camera != null) ? _camera.transform.forward : transform.forward;
			Vector3 up = Vector3.up;
			Vector3 right = Vector3.Cross(up, f);
			if (right == Vector3.zero) { right = Vector3.Cross(_camera.transform.up, f); } // prevent bug when looking directly up or down
			right.Normalize();
			if (_camera != null && Vector3.Dot(_camera.transform.up, Vector3.up) < 0) { right *= -1; } // handle upside-down turning
			Vector3 dirAlongHorizon = Vector3.Cross(right, up).normalized;
			Vector3 dir = joystickFace.x * right + joystickFace.y * dirAlongHorizon;
			lookingAt = transform.position + dir;
			transform.LookAt(lookingAt, up);
		}

		void LateUpdate()
		{
			Vector3 joystickFace = new Vector3(FaceX, FaceY);
			if (joystickFace != Vector3.zero) {
				TurnAccordingToJoystick(joystickFace);
			} else {
				if(bodyTransform == null || followEvenWhileMoving) {
					LookAtMouse(Input.mousePosition);
				} else {
					ForceLookAt(transform.position + bodyTransform.forward);
				}
			}

			if (!allowCreepyLookAngles && bodyTransform != null) {
				Vector3 localLookdir = bodyTransform.InverseTransformDirection(calculatedIdealLookRotation * Vector3.forward);
				if (localLookdir.z < 0) { // prevent the rotation if it would start going backwards
					localLookdir.z = 0;
					localLookdir.Normalize();
					Vector3 dir = bodyTransform.TransformDirection(localLookdir);
					transform.rotation = Quaternion.LookRotation(dir);
				}
			}
		}
	}
}