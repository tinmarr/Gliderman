// http://codegiraffe.com/unity/NonStandardPlayer.unitypackage
using NonStandard.Inputs;
using UnityEngine;
using UnityEngine.Events;

namespace NonStandard.Character {
	[RequireComponent(typeof(Rigidbody))]
	public class CharacterMove : MonoBehaviour
	{
		[HideInInspector] public Rigidbody rb;
		[ContextMenuItem("Create default user controls", "CreateDefaultUserControls")]
		public Transform head;

		void Start()
		{
			rb = GetComponent<Rigidbody>();
			rb.freezeRotation = true;
		}
		/// <summary>
		/// how many seconds to hold down the jump button. if a non-zero value, a jump impulse will be applied
		/// </summary>
		public float Jump { get; set; }
		private float lastJump = -1;
		public float StrafeRightMovement { get { return move.strafeRightMovement; } set { move.strafeRightMovement = value; } }
		public float MoveForwardMovement { get { return move.moveForwardMovement; } set { move.moveForwardMovement = value; } }

		[System.Serializable]
		public struct AutoMove {
			public Vector3 targetPosition;
			public System.Action whatToDoWhenTargetIsReached;
			public bool enabled;
			public bool jumpAtObstacle;
			public bool arrived;
			public static bool GetClickedLocation(Camera camera, out Vector3 targetPosition) {
				Ray ray = camera.ScreenPointToRay(UnityEngine.Input.mousePosition);
				RaycastHit rh = new RaycastHit();
				if (Physics.Raycast(ray, out rh)) {
					targetPosition = rh.point;
					return true;
				}
				targetPosition = Vector3.zero;
				return false;
			}
			public Vector3 CalculateMoveDirection(Vector3 position, float speed, Vector3 upNormal, ref bool arrived) {
				if (arrived) return Vector3.zero;
				Vector3 delta = targetPosition - position;
				if (upNormal != Vector3.zero) {
					delta = Vector3.ProjectOnPlane(delta, upNormal);
				}
				float dist = delta.magnitude;
				if (dist <= Time.deltaTime * speed) {
					arrived = true;
					return Vector3.zero;
				}
				return delta / dist; // normalized vector indicating direciton
			}
			public void SetAutoMovePosition(Vector3 position) {
				targetPosition = position;
				enabled = true;
				arrived = false;
			}
			public void DisableAutoMove() { enabled = false; }
		}

		public void SetAutoMovePosition(Vector3 position, System.Action whatToDoWhenTargetIsReached = null) {
			move.automaticMovement.SetAutoMovePosition(position);
			move.automaticMovement.whatToDoWhenTargetIsReached = whatToDoWhenTargetIsReached;
		}
		public void DisableAutoMove() { move.automaticMovement.DisableAutoMove(); }

		[System.Serializable]
		public struct CharacterMoveControls {
			public float speed;
			[Tooltip("anything steeper than this cannot be moved on")]
			public float maxStableAngle;
			public AutoMove automaticMovement;

			public bool canMoveInAir;
			public bool lookForwardMoving;
			[HideInInspector] public bool isStableOnGround;
			[HideInInspector] public float strafeRightMovement;
			[HideInInspector] public float moveForwardMovement;
			[HideInInspector] public float turnClockwise;

			[HideInInspector] public Vector3 moveDirection;
			[HideInInspector] public Vector3 groundNormal;
			[HideInInspector] public Vector3 oppositionDirection;
			[HideInInspector] public Vector3 lastVelocity;
			[HideInInspector] public Vector3 lastOppositionDirection;

			[Tooltip("Set this to enable click-to-move")]
			public Transform orientationTransform;

			Vector3 ConvertIntentionToRealDirection(Vector3 intention, Transform playerTransform, out float speed) {
				if (orientationTransform) {
					Vector3 originalIntention = intention;
					intention = orientationTransform.TransformDirection(intention);
					intention = Vector3.ProjectOnPlane(intention, Vector3.up);
					if(intention == Vector3.zero) {
						intention = Quaternion.AngleAxis(-45, Vector3.right) * originalIntention;
						intention = orientationTransform.TransformDirection(intention);
						intention = Vector3.ProjectOnPlane(intention, Vector3.up);
					}
				} else {
					intention = playerTransform.transform.TransformDirection(intention);
				}
				speed = intention.magnitude;
				intention /= speed;
				return intention;
			}
			public Vector3 AccountForBlocks(Vector3 moveVelocity) {
				if (oppositionDirection != Vector3.zero) {
					float opposition = -Vector3.Dot(moveDirection, oppositionDirection);
					if (opposition > 0) {
						moveVelocity += opposition * oppositionDirection;
					}
				}
				return moveVelocity;
			}

			public void ApplyMoveFromInput(CharacterMove cm) {
				Vector3 moveVelocity = Vector3.zero;
				Transform t = cm.transform;
				Vector3 oldDirection = moveDirection;
				moveDirection = new Vector3(strafeRightMovement, 0, moveForwardMovement);
				float intendedSpeed = 1;
				if (moveDirection != Vector3.zero) {
					moveDirection = ConvertIntentionToRealDirection(moveDirection, t, out intendedSpeed);
					if (intendedSpeed > 1) { intendedSpeed = 1; }
					// else { Debug.Log(intendedSpeed); }
				}
				if (automaticMovement.enabled) {
					if (moveDirection == Vector3.zero) {
						if (!automaticMovement.arrived) {
							moveDirection = automaticMovement.CalculateMoveDirection(t.position, speed * intendedSpeed, Vector3.up, ref automaticMovement.arrived);
							if (automaticMovement.arrived) { cm.callbacks.arrived.Invoke(automaticMovement.targetPosition); }
						}
					} else {
						automaticMovement.arrived = true; // if the player is providing input, stop calculating automatic movement
					}
				}
				if (moveDirection != Vector3.zero) {
					moveVelocity = AccountForBlocks(moveDirection);
					// apply the direction-adjusted movement to the velocity
					moveVelocity *= (speed * intendedSpeed);
				}
				if(moveDirection != oldDirection) { cm.callbacks.moveDirectionChanged.Invoke(moveDirection); }
				float gravity = cm.rb.velocity.y; // get current gravity
				moveVelocity.y = gravity; // apply to new velocity
				if(lookForwardMoving && moveDirection != Vector3.zero && orientationTransform != null)
				{
					cm.transform.rotation = Quaternion.LookRotation(moveDirection, Vector3.up);
					if(cm.head != null) { cm.head.localRotation = Quaternion.identity; } // turn head straight while walking
				}
				cm.rb.velocity = moveVelocity;
				lastVelocity = moveVelocity;
				if(oppositionDirection == Vector3.zero && lastOppositionDirection != Vector3.zero)
				{
					cm.callbacks.collisionStopped.Invoke(); // done colliding
					lastOppositionDirection = Vector3.zero;
				}
				oppositionDirection = Vector3.zero;
			}

			public void FixedUpdate(CharacterMove c) {
				if (isStableOnGround || canMoveInAir) {
					ApplyMoveFromInput(c);
				}
			}
			
			/// <summary>
			/// 
			/// </summary>
			/// <param name="cm"></param>
			/// <param name="collision"></param>
			/// <returns>the index of collision that could cause stability</returns>
			public int CollisionStabilityCheck(CharacterMove cm, Collision collision) {
				float biggestOpposition = -Vector3.Dot(moveDirection, oppositionDirection);
				int stableIndex = -1, wallCollisions = -1;
				Vector3 standingNormal = Vector3.zero;
				// identify that the character is on the ground if it's colliding with something that is angled like ground
				for (int i = 0; i < collision.contacts.Length; ++i) {
					Vector3 surfaceNormal = collision.contacts[i].normal;
					float a = Vector3.Angle(Vector3.up, surfaceNormal);
					if (a <= maxStableAngle) {
						isStableOnGround = true;
						stableIndex = i;
						standingNormal = surfaceNormal;
					} else {
						float opposition = -Vector3.Dot(moveDirection, surfaceNormal);
						if(opposition > biggestOpposition) {
							biggestOpposition = opposition;
							wallCollisions = i;
							oppositionDirection = surfaceNormal;
						}
						if(automaticMovement.jumpAtObstacle){
							cm.jump.PressJump = 1;
						}
					}
				}
				if(wallCollisions != -1) {
					if (lastOppositionDirection != oppositionDirection) {
						cm.callbacks.collisionStart.Invoke(oppositionDirection);
					}
					lastOppositionDirection = oppositionDirection;
				}
				return stableIndex;
			}
		}
	
		public CharacterMoveControls move = new CharacterMoveControls {
			speed = 5,
			maxStableAngle = 60,
			lookForwardMoving = true,
			automaticMovement = new AutoMove { }
		};

		public float GetJumpProgress() {
			return move.isStableOnGround ? 1 : (1 - ((float)jump.jumpsSoFar / jump.maxJumps));
		}

		void FixedUpdate() {
			if (Jump != lastJump) {
				jump.PressJump = Jump;
				lastJump = Jump;
			}
			move.FixedUpdate(this);
			jump.FixedUpdate(this);
			if (!move.isStableOnGround && !jump.impulseActive && move.groundNormal != Vector3.zero) {
				move.groundNormal = Vector3.zero;
				callbacks.fall.Invoke();
			}
			move.isStableOnGround = false; // invalidate stability *after* jump state is calculated
		}

		public bool IsStableOnGround() {
			return move.isStableOnGround;
		}

		private void OnCollisionStay(Collision collision) {
			if (collision.impulse != Vector3.zero && move.moveDirection != Vector3.zero && Vector3.Dot(collision.impulse.normalized, move.moveDirection) < -.75f) {
				rb.velocity = move.lastVelocity; // on a real collision, very much intentionally against a wall, maintain velocity
			}
			int contactThatMakesStability = move.CollisionStabilityCheck(this, collision);
			if(contactThatMakesStability >= 0) {
				Vector3 standingNormal = collision.contacts[contactThatMakesStability].normal;
				if (standingNormal != move.groundNormal) {
					callbacks.stand.Invoke(standingNormal);
				}
				move.groundNormal = standingNormal;
			}
		}

		private void OnCollisionEnter(Collision collision) {
			if (collision.impulse != Vector3.zero && move.CollisionStabilityCheck(this, collision) < 0) {
				rb.velocity = move.lastVelocity; // on a real collision, where the player is unstable, maintain velocity
			}
		}

		public Jumping jump = new Jumping();

		[System.Serializable]
		public class Jumping {
			public float minJumpHeight = 0.25f, maxJumpHeight = 2;
			[Tooltip("How long the jump button must be pressed to jump the maximum height")]
			public float fullJumpPressDuration = 0.25f;
			[Tooltip("for double-jumping, put a 2 here. To eliminate jumping, put a 0 here.")]
			public int maxJumps = 1;
			/// <summary>Whether or not the jumper wants to press jump (specifically, how many seconds of jump)
			/// <code>jump.PressJump = Input.GetButton("Jump") ? 1 : 0;</code></summary>
			[HideInInspector] public float PressJump;
			[HideInInspector] public bool impulseActive;
			protected float currentJumpVelocity, heightReached, heightReachedTotal, timeHeld, targetHeight;
			/// to modify inputHeld, set PressJump to a positive value.
			private bool inputHeld;
			private bool peaked;
			public bool Peaked { get { return peaked; } }

			[Tooltip("if false, double jumps won't 'restart' a jump, just add jump velocity")]
			private bool jumpStartResetsVerticalMotion = true;
			[HideInInspector] public int jumpsSoFar;// { get; protected set; }
			/// <returns>if this instance is trying to jump</returns>
			public bool IsJumping { get { return inputHeld; } set { inputHeld = value; } }
			/// <summary>pretends to hold the jump button for the specified duration</summary>
			public void FixedUpdate(CharacterMove p) {
				if (inputHeld = (PressJump > 0)) { PressJump -= Time.deltaTime; }
				if (impulseActive && !inputHeld) { impulseActive = false; peaked = true; }
				if (!inputHeld) { return; }
				bool isStableOnGround = p.IsStableOnGround();
				// check stable footing for the jump
				if (isStableOnGround && !impulseActive) {
					jumpsSoFar = 0;
					heightReached = 0;
					currentJumpVelocity = 0;
					timeHeld = 0;
					peaked = true; // used for multi-jumping state
				}
				// calculate the jump
				float gForce = -Physics.gravity.y * p.rb.mass;
				Vector3 jump_force = Vector3.zero, jumpDirection = Vector3.up;//-p.gravity.dir;
				// if the user wants to jump, and is allowed to jump again
				if (!impulseActive && (jumpsSoFar < maxJumps) && peaked) {
					heightReached = 0;
					timeHeld = 0;
					jumpsSoFar++;
					targetHeight = minJumpHeight * p.rb.mass;
					float velocityRequiredToJump = Mathf.Sqrt(targetHeight * 2 * gForce);
					// cancel out current jump/fall forces
					if (jumpStartResetsVerticalMotion) {
						float motionInVerticalDirection = Vector3.Dot(jumpDirection, p.rb.velocity);
						jump_force -= (motionInVerticalDirection * jumpDirection) / Time.deltaTime;
					}
					// apply proper jump force
					currentJumpVelocity = velocityRequiredToJump;
					peaked = false;
					jump_force += (jumpDirection * currentJumpVelocity) / Time.deltaTime;
					impulseActive = true;
					p.move.groundNormal = jumpDirection; // animation callback code might be waiting for this
					p.callbacks.jumped.Invoke(jumpDirection);
				} else
					// if a jump is happening      
					if (currentJumpVelocity > 0)
				{
					// handle jump height: the longer you hold jump, the higher you jump
					if (inputHeld) {
						timeHeld += Time.deltaTime;
						if (timeHeld >= fullJumpPressDuration) {
							targetHeight = maxJumpHeight;
							timeHeld = fullJumpPressDuration;
						} else {
							targetHeight = minJumpHeight + ((maxJumpHeight - minJumpHeight) * timeHeld / fullJumpPressDuration);
							targetHeight *= p.rb.mass;
						}
						if (heightReached < targetHeight) {
							float requiredJumpVelocity = Mathf.Sqrt((targetHeight - heightReached) * 2 * gForce);
							float forceNeeded = requiredJumpVelocity - currentJumpVelocity;
							jump_force += (jumpDirection * forceNeeded) / Time.deltaTime;
							currentJumpVelocity = requiredJumpVelocity;
						}
					} else { peaked = true; }
				} else {
					impulseActive = false;
				}
				if (currentJumpVelocity > 0) {
					float moved = currentJumpVelocity * Time.deltaTime;
					heightReached += moved;
					heightReachedTotal += moved;
					currentJumpVelocity -= gForce * Time.deltaTime;
				} else if (!peaked && !isStableOnGround) {
					peaked = true;
					impulseActive = false;
				}
				p.rb.AddForce(jump_force);
			}
		}

		[Tooltip("hooks that allow code execution when character state changes (useful for animation)")]
		public Callbacks callbacks = new Callbacks();

		[System.Serializable] public class UnityEvent_Vector3 : UnityEvent<Vector3> { }

		[System.Serializable]
		public class Callbacks
		{
			[Tooltip("when player changes direction, passes the new direction")]
			public UnityEvent_Vector3 moveDirectionChanged;
			[Tooltip("when player changes their standing angle, passes the new ground normal")]
			public UnityEvent_Vector3 stand;
			[Tooltip("when player jumps, passes the direction of the jump")]
			public UnityEvent_Vector3 jumped;
			[Tooltip("when player starts to fall")]
			public UnityEvent fall;
			[Tooltip("when player collides with a wall, passes the wall's normal")]
			public UnityEvent_Vector3 collisionStart;
			[Tooltip("when player is no longer colliding with a wall")]
			public UnityEvent collisionStopped;
			[Tooltip("when auto-moving player reaches their goal, passes absolute location of the goal")]
			public UnityEvent_Vector3 arrived;
		}
		public void CreateDefaultUserControls() {
			GameObject userInput = new GameObject(name+" input");
			Transform t = userInput.transform;
			t.SetParent(transform);
			t.localPosition = Vector3.zero;
			CharacterCamera camera = GetComponentInChildren<CharacterCamera>();
			if(camera == null) { camera = GetComponentInParent<CharacterCamera>(); }
			UserInput mouseLook = userInput.AddComponent<UserInput>();
			if(camera != null) {
				mouseLook.axisBinds.Add(new AxBind(new Axis("Mouse X", 5), "mouselook X",
					camera, "set_HorizontalRotateInput"));
				mouseLook.axisBinds.Add(new AxBind(new Axis("Mouse Y", 5), "mouselook Y",
					camera, "set_VerticalRotateInput"));
			}
			mouseLook.enabled = false;
			UserInput userMoves = userInput.AddComponent<UserInput>();
			KBind rightClick = new KBind(KCode.Mouse1, "use mouselook",
				pressFunc: KBind.Func(mouseLook, "set_enabled", true),
				releaseFunc: KBind.Func(mouseLook, "set_enabled", false));
			rightClick.keyEvent.AddPress(camera, "SetMouseCursorLock", true);
			rightClick.keyEvent.AddRelease(camera, "SetMouseCursorLock", false);
			userMoves.keyBinds.Add(rightClick);
			userMoves.keyBinds.Add(new KBind(KCode.PageUp, "zoom in",
				pressFunc: KBind.Func(camera, "set_ZoomInput", -5f),
				releaseFunc: KBind.Func(camera, "set_ZoomInput", 0f)));
			userMoves.keyBinds.Add(new KBind(KCode.PageDown, "zoom out",
				pressFunc: KBind.Func(camera, "set_ZoomInput", 5f),
				releaseFunc: KBind.Func(camera, "set_ZoomInput", 0f)));
			userMoves.keyBinds.Add(new KBind(KCode.Space, "jump",
				pressFunc: KBind.Func(this, "set_Jump", 1f),
				releaseFunc: KBind.Func(this, "set_Jump", 0f)));
			userMoves.axisBinds.Add(new AxBind(new Axis("Horizontal"), "strafe right/left",
				this, "set_StrafeRightMovement"));
			userMoves.axisBinds.Add(new AxBind(new Axis("Vertical"), "move forward/backward",
				this, "set_MoveForwardMovement"));
			userMoves.axisBinds.Add(new AxBind(new Axis("Mouse ScrollWheel", -4), "zoom in/out",
				camera, "AddToTargetDistance"));
		}
	}
}