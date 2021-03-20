using UnityEngine;

namespace NonStandard.Utility {
	public class LeaveParent : MonoBehaviour {
		void Start () {
			transform.SetParent(null);
		}
	}
}