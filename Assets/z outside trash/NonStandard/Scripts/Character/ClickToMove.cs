using UnityEngine;

namespace NonStandard.Character {
	public class ClickToMove : MonoBehaviour {
	public CharacterMove characterToMove;
	public KeyCode key = KeyCode.Mouse0;
	public Camera _camera;
	public LayerMask validToMove = -1;

#if UNITY_EDITOR
	/// called when created by Unity Editor
	void Reset() {
		if (characterToMove == null) { characterToMove = transform.GetComponentInParent<CharacterMove>(); }
		if (characterToMove == null) { characterToMove = FindObjectOfType<CharacterMove>(); }
		if (_camera == null) { _camera = GetComponent<Camera>(); }
		if (_camera == null) { _camera = Camera.main; }
		if (_camera == null) { _camera = FindObjectOfType<Camera>(); ; }
	}
#endif
	private void Start() { }

	void Update()
	{
		if (UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject == null && Input.GetKey(key))
		{
			Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
			RaycastHit rh;
			Physics.Raycast(ray, out rh, float.PositiveInfinity, validToMove);
			if(rh.collider != null) {
				characterToMove.SetAutoMovePosition(rh.point, ()=> { characterToMove.DisableAutoMove(); });
			}
		}
	}
}
}