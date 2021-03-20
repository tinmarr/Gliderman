using UnityEngine;

namespace NonStandard.Character
{
	public class SuperCyanGlue : MonoBehaviour
	{
		public Animator animator;
		public CharacterMove character;
		private void Start() {
			if (animator == null) { animator = GetComponent<Animator>(); }
			if (animator == null) { animator = GetComponentInParent<Animator>(); }
			if (character == null) { character = GetComponent<CharacterMove>(); }
			if (character == null) { character = GetComponentInParent<CharacterMove>(); }
			if (character == null) {
				Utility.Follow f = GetComponent<Utility.Follow>();
				if (f) { character = f.whoToFollow.GetComponent<CharacterMove>(); }
			}
			character.callbacks.jumped.AddListener(Jump);
			character.callbacks.stand.AddListener(Stand);
			character.callbacks.fall.AddListener(Fall);
			character.callbacks.arrived.AddListener(Wave);
		}

		bool shouldTriggerJumpAnimation = false;
		public void Jump(Vector3 dir) {
			animator.SetTrigger("Land");
			animator.SetBool("Grounded", true);
			if (character.IsStableOnGround()) {
				shouldTriggerJumpAnimation = true;
			}
		}

		public void Stand(Vector3 upDir) {
			animator.SetTrigger("Land");
			animator.SetBool("Grounded", true);
		}

		public void Fall() {
			animator.SetBool("Grounded", false);
		}

		public void Wave(Vector3 location) {
			animator.SetTrigger("Wave");
		}

		public void FixedUpdate() {
			float speed = character.rb.velocity.magnitude;
			animator.SetFloat("MoveSpeed", speed);
			if (shouldTriggerJumpAnimation && !animator.IsInTransition(0)) {
				animator.SetTrigger("Jump");
				animator.SetBool("Grounded", false);
				shouldTriggerJumpAnimation = false;
			}
		}
	}
}