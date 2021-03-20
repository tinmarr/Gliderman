using UnityEngine;
using UnityEngine.Events;

namespace NonStandard.Utility {
	public class ActivateOnStart : MonoBehaviour {
		public UnityEvent onStart;
		void Start() {
			onStart.Invoke();
		}
	}
}