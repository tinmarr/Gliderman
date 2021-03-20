using UnityEngine;

namespace NonStandard.Character
{
	public class CharacterMoveAnimationCallbacks : MonoBehaviour
	{
		public CharacterMove character;

		[Tooltip("hooks that allow code execution when character state changes (useful for animation)")]
		public CharacterMove.Callbacks callbacks = new CharacterMove.Callbacks();

		void Start()
		{
			if (character == null) { character = GetComponentInParent<CharacterMove>(); }
			if (character == null) { character = GetComponent<CharacterMove>(); }
			if (character == null) { Utility.Follow f = GetComponent<Utility.Follow>();
				if (f) { character = f.whoToFollow.GetComponent<CharacterMove>(); }
			}
			if (character != null) {
				character.callbacks.moveDirectionChanged.AddListener(callbacks.moveDirectionChanged.Invoke);
				character.callbacks.stand.AddListener(callbacks.stand.Invoke);
				character.callbacks.jumped.AddListener(callbacks.jumped.Invoke);
				character.callbacks.fall.AddListener(callbacks.fall.Invoke);
				character.callbacks.collisionStart.AddListener(callbacks.collisionStart.Invoke);
				character.callbacks.collisionStopped.AddListener(callbacks.collisionStopped.Invoke);
				character.callbacks.arrived.AddListener(callbacks.arrived.Invoke);
			}
		}

	}
}