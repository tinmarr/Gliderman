using System.Collections.Generic;
#if UNITY_5_3_OR_NEWER
using System.Collections;
using System.Text;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using System.Linq;
#endif
#endif

/* // example code:
NonStandard.Clock.setTimeout (() => {
	NonStandard.Clock.Log("This will print 3 seconds after setTimeout was called!");
}, 3000);
// Code above will work like you expect if Chrono.Update() is being called regularly, which will happen if Unity has at least one Timer or MainTimer instance.
*/

// author: mvaganov@hotmail.com
// license: Copyfree, public domain. This is free code! Great artists, steal this code!
// latest version at: https://pastebin.com/raw/h61nAC3E -- (2020/11/26)
namespace NonStandard
{
#if UNITY_5_3_OR_NEWER
	public class Timer : MonoBehaviour
	{
		[Tooltip("When to trigger"), ContextMenuItem("Create Main Timer", "CraeteMainTimer")]
		public float seconds = 1;
		[Tooltip("Transform to teleport to\nSceneAsset to load a new scene\nAudioClip to play audio\nGameObject to SetActivate(true)")]
		public ObjectPtr whatToActivate = new ObjectPtr();
		[Tooltip("restart a timer after triggering")]
		public bool repeat = false;
		[Tooltip("attempt to deactivate instead of activate")]
		public bool deactivate = false;

		private void DoTimer() {
			if (repeat) {
				Clock.setTimeout(DoTimer, (long)(seconds * 1000));
			}
			Clock.ScheduledTask todo = Clock.setTimeout(whatToActivate.Data, (long)(seconds * 1000));
			todo.who = this;
			todo.activate = !deactivate;
		}
		private void Awake() { MainClock.Instance(); }
		void Start() { if (whatToActivate.Data != null) { DoTimer(); } }
	}

	public static class TransformExtention {
		public static string HierarchyPath(this Transform t) {
			StringBuilder sb = new StringBuilder();
			sb.Append(t.name);
			t = t.parent;
			while (t != null) {
				sb.Insert(0, t.name + "/");
				t = t.parent;
			}
			return sb.ToString();
		}
	}

	/// <summary>
	/// a class that enables the timer queue to be observed in the editor at runtime
	/// </summary>
	public class MainClock : MonoBehaviour {
		public Clock clock;
		public PauseEvents pauseEvents;
		[System.Serializable] public class PauseEvents {
			[Tooltip("do this when time is paused")] public UnityEngine.Events.UnityEvent onPause;
			[Tooltip("do this when time is unpaused")] public UnityEngine.Events.UnityEvent onUnpause;
		}

		public void Awake() {
			if (clock != Clock.Instance && clock != null) { Clock.Instance.Absorb(clock); }
			clock = Clock.Instance;
			clock.linkedToMainThread = System.Threading.Thread.CurrentThread;
			clock.onPause += SelfPause;
			clock.onUnpause += SelfUnpause;
			clock.Start();
		}
		private void SelfPause() { pauseEvents.onPause.Invoke(); }
		private void SelfUnpause() { pauseEvents.onUnpause.Invoke(); }

		void Update() { clock.Update(); }
		void OnApplicationPause(bool paused) { clock.OnApplicationPause(paused); }
		void OnDisable() { clock.OnDisable(); }
		void OnEnable() { clock.OnEnable(); }
		private void OnApplicationQuit() { clock.OnApplicationQuit(); }
#if UNITY_EDITOR
		public void OnValidate() { if (clock == null) { clock = Clock.Instance; } clock.OnValidate(); }
#endif
		internal static MainClock s_instance;
		public static MainClock Instance() {
			if (s_instance != null) return s_instance;
			s_instance = FindObjectOfType<MainClock>();
			if (s_instance == null) { // if it doesn't exist
				GameObject g = new GameObject("<" + typeof(Clock).Name + ">");
				s_instance = g.AddComponent<MainClock>(); // create one
			}
			return s_instance;
		}
	}
#endif

	[System.Serializable] public class Clock {
		// https://docs.microsoft.com/en-us/dotnet/api/system.timers.timer?redirectedfrom=MSDN&view=netframework-4.8
		// explicitly NOT using System.Timers.Timer because it's multi-threaded, and many Unity methods, notably the time keeping ones, must be called from the main thread.

		/// The singleton
		private static Clock s_instance = null;
		/// [Tooltip("keeps track of how long each update takes. If a timer-update takes longer than this, stop executing events and do them later. Less than 0 for no limit, 0 for one event per update.")]
		public int maxMillisecondsPerUpdate = 100;
		private long maxTicksPerUpdate;
		public long updateCount { get; private set; }
		public static long UpdateCount { get { return Instance.updateCount; } }
		/// queue of things to do using game time. use this for in-game events that can be paused or slowed down with time dialation.
		public List<ScheduledTask> queue = new List<ScheduledTask>();
		/// queue of things to do using real-time game time. use this for UI events, or things that use the real-world as a reference, that should always work at the same rate
		public List<ScheduledTask> queueRealtime = new List<ScheduledTask>();
		/// While this is zero, use system time. As soon as time becomes perturbed, by pause or time scale, this now keeps track of game-time. To reset time back to realtime, use SynchToRealtime()
		private long alternativeTicks = 0;
		private long realtimeOffset;
		/// [Tooltip("stop advancing time & executing the queue?")]
		public bool paused = false;
		private bool pausedLastFrame = false;
#if UNITY_5_3_OR_NEWER
		public bool pausePhysicsWhenPaused = true;
#endif
		/// <summary>
		/// identifies the main thread
		/// </summary>
		internal System.Threading.Thread linkedToMainThread = null;
		/// The timer counts in milliseconds, Unity measures in fractions of a second. This value reconciles fractional milliseconds.
		private float leftOverTime = 0;
		/// if actions are interrupted, probably by a deadline, this keeps track of what was being done
		private List<ScheduledTask> _currentlyDoing = new List<ScheduledTask>();
		private int currentlyDoneThingIndex = 0;

		public System.Action onPause, onUnpause;

		[System.Serializable]
		public class ScheduledTask {
			public string description;
			/// Unix Epoch Time Milliseconds
			public long when;
			/// could be a delegate, or an executable object
			public object what;
			/// a parameter for 'who' wants this particular 'what' to be done, like a context.
			public object who;
			/// whether or not to DO or UN-do
			public bool activate = true;
			/// what could be a delegate, or an executable object, as executed by a Trigger
			public ScheduledTask(long when, object what, string description = null, object who = null) {
				if (description == null) {
					if (what != null && typeof(System.Action).IsAssignableFrom(what.GetType())) {
						System.Action a = what as System.Action;
						description = a.Method.Name;
					} else if (what != null) {
						description = what.ToString();
					}
				}
				this.description = description; this.when = when; this.what = what; this.who = who;
			}
			/// comparer, used to sort into a list
			public class Comparer : IComparer<ScheduledTask> {
				public int Compare(ScheduledTask x, ScheduledTask y) { return x.when.CompareTo(y.when); }
			}
			public static Comparer compare = new Comparer();
		}

		public static void Log(string text, bool error = false) {
#if UNITY_5_3_OR_NEWER
			if (error) Debug.LogError(text);
			else Debug.Log(text);
#else
			if(error) System.Console.Error.WriteLine(text);
			else System.Console.WriteLine(text);
#endif
		}
		private Clock() {
#if UNITY_5_3_OR_NEWER
			MainClock.Instance();
#endif
		}

		public static Clock Instance { get { return (s_instance != null) ? s_instance : s_instance = new Clock(); } }

		/// Unix Time: milliseconds since Jan 1 1970
		public static long NowRealtime { get { return System.DateTime.Now.Ticks / System.TimeSpan.TicksPerMillisecond; } }
		/// very fast time keeping at millisecond resolution. no guarantees about the range of values, just that time is kept.
		public static long NowRealTicks { get { return System.Environment.TickCount; } }

		/// game time right now (modified by pausing or Time.timeScale)
		public long now { get { return (alternativeTicks == 0) ? NowRealtime : alternativeTicks; } }
		public long nowTicks { get { return (alternativeTicks == 0) ? NowRealTicks : alternativeTicks; } }

		/// game time right now (modified by pausing or Time.timeScale)
		public static long Now { get { return Instance.now; } }
		/// time right now, modified by pausing or Time.timeScale
		public static long NowTicks { get { return Instance.nowTicks; } }

		private static bool _quitting = false;
		public static bool IsQuitting { get { return _quitting; } internal set { _quitting = value; } }
		public void OnApplicationQuit() { _quitting = true; } // called by Unity

		/// clears the difference between game time and real time
		public void SyncToRealtime() { alternativeTicks = 0; }

		/// <summary>
		/// if more than one Clock is created accidentally, this allows them to merge
		/// </summary>
		/// <param name="otherTimerQueue"></param>
		public void Absorb(Clock otherTimerQueue) {
			for (int i = 0; i < otherTimerQueue.queue.Count; ++i) {
				ScheduledTask todo = otherTimerQueue.queue[i];
				queue.Insert(BestIndexFor(todo.when, queue), todo);
			}
			otherTimerQueue.queue.Clear();
			for (int i = 0; i < otherTimerQueue.queue.Count; ++i) {
				ScheduledTask todo = otherTimerQueue.queueRealtime[i];
				queueRealtime.Insert(BestIndexFor(todo.when, queueRealtime), todo);
			}
			otherTimerQueue.queueRealtime.Clear();
		}

		/// <summary>
		/// calculates the best index for a given task in a given queue
		/// </summary>
		/// <param name="soon">when the task needs to execute</param>
		/// <param name="a_queue">the queue the task needs to go into</param>
		/// <returns></returns>
		private int BestIndexFor(long soon, List<ScheduledTask> a_queue) {
			int index;
			if (a_queue.Count < 8) { // for small lists (which happen A LOT), linear search is fine.
				for (index = 0; index < a_queue.Count; ++index) {
					if (a_queue[index].when > soon) break;
				}
			} else {
				ScheduledTask toInsert = new ScheduledTask(soon, null);
				index = a_queue.BinarySearch(toInsert, ScheduledTask.compare);
				if (index < 0) { index = ~index; }
			}
			return index;
		}

		/// <summary>as the JavaScript function</summary>
		/// <param name="action">Action. an object to trigger, expected to be a delegate or System.Action</param>
		/// <param name="delayMilliseconds">Delay milliseconds.</param>
		public ScheduledTask SetTimeout(System.Action action, long delayMilliseconds) {
			return SetTimeout((object)action, delayMilliseconds);
		}
		/// <summary>as the JavaScript function</summary>
		/// <param name="action">Action. an object to trigger, expected to be a delegate or System.Action</param>
		/// <param name="delayMilliseconds">Delay milliseconds.</param>
		public ScheduledTask SetTimeout(object action, long delayMilliseconds) {
			long soon = nowTicks + delayMilliseconds;// * System.TimeSpan.TicksPerMillisecond;
			ScheduledTask todo = new ScheduledTask(soon, action);
			queue.Insert(BestIndexFor(soon, queue), todo);
			LinkedToMainThreadCheck();
			return todo;
		}

		public List<ScheduledTask> UnsetTimeout(object action) {
			List<ScheduledTask> unset = new List<ScheduledTask>();
			for (int i = queue.Count - 1; i >= 0; --i) {
				if (queue[i].what == action) {
					unset.Add(queue[i]);
					queue.RemoveAt(i);
				}
			}
			return unset;
		}

		/// <summary>as the JavaScript function</summary>
		/// <param name="action">Action. an object to trigger, expected to be a delegate or System.Action</param>
		/// <param name="delayMilliseconds">Delay milliseconds.</param>
		public ScheduledTask SetTimeoutRealtime(object action, long delayMilliseconds) {
			long soon = NowRealTicks + delayMilliseconds;// * System.TimeSpan.TicksPerMillisecond;
			ScheduledTask todo = new ScheduledTask(soon, action);
			queueRealtime.Insert(BestIndexFor(soon, queueRealtime), todo);
			LinkedToMainThreadCheck();
			return todo;
		}

		public void LinkedToMainThreadCheck() {
#if !UNITY_5_3_OR_NEWER
			if (linkedToMainThread == null) {
				Log("Please call Clock.Instance.Update() regularly to use Clock system", true);
			}
#endif
		}

		/// <param name="action">Action. what to do</param>
		/// <param name="delayMilliseconds">Delay milliseconds. in how-many-milliseconds to do it</param>
		public static ScheduledTask setTimeout(object action, long delayMilliseconds) {
			return Instance.SetTimeout(action, delayMilliseconds);
		}

		public static List<ScheduledTask> unsetTimeout(System.Action action) {
			return Instance.UnsetTimeout(action);
		}

		/// <param name="action">Action. what to do</param>
		/// <param name="delayMilliseconds">Delay milliseconds. in how-many-milliseconds to do it</param>
		public static ScheduledTask setTimeoutRealtime(object action, long delayMilliseconds) {
			return Instance.SetTimeoutRealtime(action, delayMilliseconds);
		}

		/// Allows implicit conversion of lambda expressions and delegates
		/// <param name="action">Action. what to do</param>
		/// <param name="delayMilliseconds">Delay milliseconds. in how-many-milliseconds to do it</param>
		public static ScheduledTask setTimeout(System.Action action, long delayMilliseconds) {
			return Instance.SetTimeout(action, delayMilliseconds);
		}

		/// <summary>Allows implicit conversion of lambda expressions and delegates</summary>
		/// <param name="action">Action. what to do</param>
		/// <param name="delayMilliseconds">Delay milliseconds. in how-many-milliseconds to do it</param>
		public static ScheduledTask setTimeoutRealtime(System.Action action, long delayMilliseconds) {
			return Instance.SetTimeoutRealtime(action, delayMilliseconds);
		}

		//void OnApplicationPause(bool paused) { if(alternativeTime == 0) { alternativeTime = now; } }
		public void Pause() { paused = true; }
		public void Unpause() { paused = false; }
		public void OnApplicationPause(bool paused) { if (alternativeTicks == 0) { alternativeTicks = nowTicks; } }
		public void OnDisable() { OnApplicationPause(true); }
		public void OnEnable() { OnApplicationPause(false); }

		/// used to handle pause/unpause behavior
		public delegate void BooleanAction(bool b);
		public static void EquateUnityEditorPauseWithApplicationPause(BooleanAction b) {
#if UNITY_EDITOR
			// This method is run whenever the playmode state is changed.
			UnityEditor.EditorApplication.pauseStateChanged += (UnityEditor.PauseState ps) => {
				b(ps == UnityEditor.PauseState.Paused);
			};
#endif
		}

		public void Init() {
			EquateUnityEditorPauseWithApplicationPause(OnApplicationPause);
			if (s_instance != null && s_instance != this) { throw new System.Exception("There should only be one " + GetType()); }
			s_instance = this;
		}

		void RefreshTiming() { maxTicksPerUpdate = maxMillisecondsPerUpdate; }

#if UNITY_EDITOR
		public void OnValidate() { RefreshTiming(); }
#endif

		public void Start() { Init(); RefreshTiming(); }

#if UNITY_5_3_OR_NEWER
		private float __oldTimeScale;
#endif
		public void Update() {
			if(linkedToMainThread == null) { linkedToMainThread = System.Threading.Thread.CurrentThread; }
			updateCount++;
			// handle pause behavior
			if (paused) {
				if (!pausedLastFrame) {
					alternativeTicks = nowTicks;
					if (onPause != null) { onPause.Invoke(); }
					pausedLastFrame = true;
#if UNITY_5_3_OR_NEWER
					if (pausePhysicsWhenPaused) {
						__oldTimeScale = Time.timeScale;
						Time.timeScale = 0;
					}
#endif
				}
			} else if (pausedLastFrame) {
				if (onUnpause != null) { onUnpause.Invoke(); }
				pausedLastFrame = false;
#if UNITY_5_3_OR_NEWER
				if (pausePhysicsWhenPaused) {
					Time.timeScale = __oldTimeScale;
				}
#endif
			}
			// pump the timer queues, both realtime (takes priority) and game-time
			long now_t, nowForReals = NowRealTicks;
			long deadline = nowForReals + maxTicksPerUpdate;
			int thingsDone = 0;
			if (queueRealtime.Count > 0) {
				thingsDone = DoWhatIsNeededNow(queueRealtime, nowForReals, deadline);
			}
			if (queue.Count > 0 && !paused) {
				if (alternativeTicks == 0) {
					now_t = nowForReals;
#if UNITY_5_3_OR_NEWER
					if (Time.timeScale != 1) { alternativeTicks = now_t; }
#endif
				} else {
					float deltaTimeTicks =
#if UNITY_5_3_OR_NEWER
					(Time.deltaTime * 1000);
#else
					(__lastUpdate != 0) ? (NowRealTicks - __lastUpdate) : 0;
#endif
					long deltaTimeTicksLong = (long)(deltaTimeTicks + leftOverTime);
					alternativeTicks += deltaTimeTicksLong;
					leftOverTime = deltaTimeTicks - deltaTimeTicksLong;
					now_t = alternativeTicks;
				}
				if (thingsDone == 0 || now_t < deadline) {
					thingsDone += DoWhatIsNeededNow(queue, now_t, deadline);
				}
			}
#if !UNITY_5_3_OR_NEWER
			__lastUpdate = NowRealTicks;
#endif
		}
#if !UNITY_5_3_OR_NEWER
		private long __lastUpdate = 0;
#endif

		int DoWhatIsNeededNow(List<ScheduledTask> a_queue, long now_t, long deadline) {
			bool tryToDoMore;
			int thingsDone = 0;
			do {
				tryToDoMore = false;
				if (a_queue.Count > 0 && a_queue[0].when <= now_t) {
					if (_currentlyDoing.Count == 0) {
						// the things to do in the queue might add to the queue, so to prevent infinite looping...
						// separate out the elements to do right now
						for (int i = 0; i < a_queue.Count; ++i) {
							if (a_queue[i].when > now_t) { break; }
							_currentlyDoing.Add(a_queue[i]);
						}
						// if there's nothing to do, get out of this potential loop
						if (_currentlyDoing.Count == 0) { break; }
						a_queue.RemoveRange(0, _currentlyDoing.Count);
						tryToDoMore = false;
					}
					// do what is scheduled to do right now
					while (currentlyDoneThingIndex < _currentlyDoing.Count) {
						ScheduledTask todo = _currentlyDoing[currentlyDoneThingIndex++];
						// if DoActivate adds to the queue, it won't get executed this cycle
						NonStandard.ActivateAnything.DoActivate(todo.what, this, todo.who, todo.activate);
						++thingsDone;
						// if it took too long to do that thing, stop and hold the rest of the things to do till later.
						if (maxTicksPerUpdate >= 0 && NowRealTicks > deadline) {
							break;
						}
					}
					if (currentlyDoneThingIndex >= _currentlyDoing.Count) {
						_currentlyDoing.Clear();
						currentlyDoneThingIndex = 0;
						tryToDoMore = NowRealTicks < deadline && a_queue.Count > 0;
					}
				}
			} while (tryToDoMore);
			return thingsDone;
		}
	}

	/// This class serves as a store for static Utility functions related to 'activating things'.
	public static class ActivateAnything
	{
		/// <summary>'Activates' something, in a ways that is possibly specific to circumstances.</summary>
		/// <param name="whatToActivate">what needs to be activated. In "The straw that broke the camel's back", this is the "camel's back", which we can assume is breaking (or unbreaking?) during this function.</param>
		/// <param name="causedActivate">what triggered the activation. In "The straw that broke the camel's back", this is "the straw". Depending on the straw, breaking may happen differently.</param>
		/// <param name="doingActivate">what is doing the activation. In "The straw that broke the camel's back", this is "the camel".</param>
		/// <param name="activate">whether to activate or deactivate. In "The straw that broke the camel's back", this is whether to break or unbreak the-camel's-back.</param>
		/// <param name="delayInSeconds">Delay in seconds.</param>
		public static void DoActivate(
			object whatToActivate, object causedActivate, object doingActivate, bool activate,
			float delayInSeconds
		) {
			if (delayInSeconds <= 0) {
				DoActivate(whatToActivate, causedActivate, doingActivate, activate);
			} else {
				Clock.setTimeout(() => {
					DoActivate(whatToActivate, causedActivate, doingActivate, activate);
				}, (long)(delayInSeconds * 1000));
			}
		}

		/// <summary>'Activates' something, in a ways that is possibly specific to circumstances.</summary>
		/// <param name="whatToActivate">what needs to be activated. In "The straw that broke the camel's back", this is the "camel's back", which we can assume is breaking (or unbreaking?) during this function.</param>
		/// <param name="causedActivate">what triggered the activation. In "The straw that broke the camel's back", this is "the (1?) straw". Depending on the straw, or straw count, breaking may happen differently.</param>
		/// <param name="doingActivate">what is doing the activation. In "The straw that broke the camel's back", this is "the camel", or possibly, "the straw".</param>
		/// <param name="activate">whether to activate or deactivate. In "The straw that broke the camel's back", this is whether to break or unbreak the-camel's-back.</param>
		public static void DoActivate(
			object whatToActivate, object causedActivate, object doingActivate, bool activate
		) {
			if (whatToActivate == null) { Clock.Log("Don't know how to activate null"); return; }
			if (whatToActivate is IReference) {
				whatToActivate = ((IReference)whatToActivate).Dereference();
			}
			System.Type type = whatToActivate.GetType();
			if (typeof(System.Action).IsAssignableFrom(type)) {
				System.Action a = whatToActivate as System.Action;
				a.Invoke();
#if UNITY_5_3_OR_NEWER
			} else if (typeof(UnityEngine.Events.UnityEvent).IsAssignableFrom(type)) {
				UnityEngine.Events.UnityEvent a = whatToActivate as UnityEngine.Events.UnityEvent;
				a.Invoke();
			} else if (type == typeof(Transform)) {
				Transform targetLocation = ConvertToTransform(whatToActivate);
				Transform toMove = ConvertToTransform(causedActivate);
				if (toMove != null) {
					toMove.position = targetLocation.position;
				}
			} else if (type == typeof(AudioClip) || type == typeof(AudioSource)) {
				AudioSource asource = null;
				if (type == typeof(AudioSource)) {
					asource = whatToActivate as AudioSource;
				}
				if (asource == null) {
					GameObject go = ConvertToGameObject(doingActivate);
					if (go != null) {
						asource = go.AddComponent<AudioSource>();
					} else {
						throw new System.Exception("can't create audio without a game object to put it on.");
					}
				}
				if (type == typeof(AudioClip)) {
					asource.clip = whatToActivate as AudioClip;
				}
				if (activate) {
					asource.Play();
				} else {
					asource.Stop();
				}
			} else if (type == typeof(ParticleSystem)) {
				ParticleSystem ps = whatToActivate as ParticleSystem;
				if (activate) {
					Transform t = ps.transform;
					GameObject go = ConvertToGameObject(doingActivate);
					t.position = go.transform.position;
					t.rotation = go.transform.rotation;
					ParticleSystem.ShapeModule sm = ps.shape;
					if (sm.shapeType == ParticleSystemShapeType.Mesh) {
						sm.mesh = go.GetComponent<MeshFilter>().mesh;
						sm.scale = go.transform.lossyScale;
					} else if (sm.shapeType == ParticleSystemShapeType.MeshRenderer) {
						sm.meshRenderer = go.GetComponent<MeshRenderer>();
					}
					ps.Play();
				} else {
					ps.Stop();
				}
			} else if (type == typeof(GameObject)) {
				GameObject go = (whatToActivate as GameObject);
				if (go == null) {
					Debug.LogWarning("GameObject destroyed?");
				} else {
					go.SetActive(activate);
				}
			} else if (type == typeof(UnityEngine.Color)) {
				GameObject go = ConvertToGameObject(doingActivate);
				RememberedOriginalColor r = go.GetComponent<RememberedOriginalColor>();
				if (activate) {
					Color c = (Color)whatToActivate;
					if (r == null) {
						r = go.AddComponent<RememberedOriginalColor>();
						r.oldColor = go.GetComponent<Renderer>().material.color;
					}
					go.GetComponent<Renderer>().material.color = c;
				} else {
					if (r != null) {
						go.GetComponent<Renderer>().material.color = r.oldColor;
						UnityEngine.Object.Destroy(r);
					}
				}
			} else if (type == typeof(UnityEngine.Material)) {
				GameObject go = ConvertToGameObject(doingActivate);
				RememberedOriginalMaterial r = go.GetComponent<RememberedOriginalMaterial>();
				if (activate) {
					Material m = whatToActivate as Material;
					if (r == null) {
						r = go.AddComponent<RememberedOriginalMaterial>();
						r.oldMaterial = go.GetComponent<Renderer>().material;
					}
					go.GetComponent<Renderer>().material = m;
				} else {
					if (r != null) {
						go.GetComponent<Renderer>().material = r.oldMaterial;
						UnityEngine.Object.Destroy(r);
					}
				}
			} else if (typeof(IEnumerable).IsAssignableFrom(type)) {
				IEnumerable ienum = whatToActivate as IEnumerable;
				IEnumerator iter = ienum.GetEnumerator();
				while (iter.MoveNext()) {
					DoActivate(iter.Current, causedActivate, doingActivate, activate);
				}
			} else if (type == typeof(Animation)) {
				if (activate) {
					(whatToActivate as Animation).Play();
				} else {
					(whatToActivate as Animation).Stop();
				}
#endif
			} else {
				System.Reflection.MethodInfo[] m = type.GetMethods();
				bool invoked = false;
				for (int i = 0; i < m.Length; ++i) {
					System.Reflection.MethodInfo method = m[i];
					if ((activate && method.Name == "DoActivateTrigger")
					|| (!activate && method.Name == "DoDeactivateTrigger")) {
						switch (method.GetParameters().Length) {
						case 0: method.Invoke(whatToActivate, new object[] { }); invoked = true; break;
						case 1: method.Invoke(whatToActivate, new object[] { causedActivate }); invoked = true; break;
						case 2: method.Invoke(whatToActivate, new object[] { causedActivate, doingActivate }); invoked = true; break;
						}
						break;
					}
				}
				if (!invoked) {
					Clock.Log("Don't know how to " + ((activate) ? "DoActivateTrigger" : "DoDeactivateTrigger") + " a \'" + type + "\' (" + whatToActivate + ") with \'" + doingActivate + "\', triggered by \'" + causedActivate + "\'", true);
				}
			}
		}
#if UNITY_5_3_OR_NEWER
		public class RememberedOriginalMaterial : MonoBehaviour { public Material oldMaterial; }
		public class RememberedOriginalColor : MonoBehaviour { public Color oldColor; }

		public static GameObject ConvertToGameObject(object obj) {
			if (obj is GameObject) { return obj as GameObject; }
			if (obj is Component) { return (obj as Component).gameObject; }
			if (obj is Collision) { return (obj as Collision).gameObject; }
			if (obj is Collision2D) { return (obj as Collision2D).gameObject; }
			return null;
		}

		public static Transform ConvertToTransform(object obj) {
			if (obj is Transform) { return obj as Transform; }
			GameObject go = ConvertToGameObject(obj);
			if (go != null) { return go.transform; }
			return null;
		}
#endif
	}

	public interface IReference { object Dereference(); }

#if UNITY_5_3_OR_NEWER
	[System.Serializable]
	public struct ObjectPtr : IReference
	{
		public Object data;
		public Object Data { get { return data; } set { data = value; } }
		public object Dereference() {
			return data;
		}
		public TYPE GetAs<TYPE>() where TYPE : class { return data as TYPE; }
	}
#endif

#if UNITY_EDITOR
	// used to get lists of classes for the ObjectPtr property
	public static class Reflection
	{
		public static System.Type[] GetTypesInNamespace(string nameSpace, bool includeComponentTypes = false, System.Reflection.Assembly assembly = null) {
			if (assembly == null) {
				assembly = System.Reflection.Assembly.GetExecutingAssembly();
			}
			System.Type[] types = assembly.GetTypes().Where(t =>
				System.String.Equals(t.Namespace, nameSpace, System.StringComparison.Ordinal)
				&& (includeComponentTypes || !t.ToString().Contains('+'))).ToArray();
			return types;
		}
		public static string CleanFront(string str, string trimMe) {
			if (str.StartsWith(trimMe)) { return str.Substring(trimMe.Length); }
			return str;
		}
		public static List<string> TypeNamesCleaned(System.Type[] validTypes, string namespaceToClean) {
			List<string> list = new List<string>();
			for (int i = 0; i < validTypes.Length; ++i) {
				string typename = validTypes[i].ToString();
				typename = CleanFront(typename, namespaceToClean + ".");
				list.Add(typename);
			}
			return list;
		}

		public static T EditorGUI_EnumPopup<T>(Rect _position, T value) {
			System.Type t = typeof(T);
			if (t.IsEnum) {
				string[] names = System.Enum.GetNames(t);
				string thisone = value.ToString();
				int index = System.Array.IndexOf(names, thisone);
				index = EditorGUI.Popup(_position, index, names);
				value = (T)System.Enum.Parse(t, names[index]);
			}
			return value;
		}
	}
#endif
}

#if UNITY_EDITOR
// used to create scriptable objects with the ObjectPtr property
public static class ScriptableObjectUtility
{
	/// This makes it easy to create, name and place unique new ScriptableObject asset files.
	public static T CreateAsset<T>() where T : ScriptableObject { return CreateAsset(typeof(T)) as T; }
	public static ScriptableObject CreateAsset(System.Type t, string filename = "", string path = "") {
		ScriptableObject asset = ScriptableObject.CreateInstance(t);
		string whereItWasSaved = SaveScriptableObjectAsAsset(asset, filename, path);
		asset = Resources.Load(whereItWasSaved, t) as ScriptableObject;
		return asset;
	}

	public static string SaveScriptableObjectAsAsset(ScriptableObject asset, string filename = "", string path = "") {
		System.Type t = asset.GetType();
		if (path == "") {
			path = AssetDatabase.GetAssetPath(Selection.activeObject);
			if (path == "") {
				path = UnityEngine.SceneManagement.SceneManager.GetActiveScene().path;//"Assets";
				Debug.Log(path);
				int idx = path.LastIndexOf("/");
				if (idx < 0) {
					path = "Assets";
				} else {
					path = path.Substring(0, idx);
					if (filename == "") {
						string typename = t.ToString();
						int idx2 = typename.LastIndexOf(".");
						if (idx > 0) { typename = typename.Substring(idx2); }
						filename = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name + typename + ".asset";
					}
					Debug.Log(path + " //// " + filename);
				}
				//				Debug.Log(UnityEngine.SceneManagement.SceneManager.GetActiveScene().path);
			} else if (System.IO.Path.GetExtension(path) != "") {
				path = path.Replace(System.IO.Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
			}
		}
		if (filename.Length == 0) { filename = "New " + t.ToString() + ".asset"; }
		string fullpath = path + "/" + filename;
		string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(fullpath);
		AssetDatabase.CreateAsset(asset, assetPathAndName);
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
		EditorUtility.FocusProjectWindow();
		Selection.activeObject = asset;
		Debug.Log("saved " + fullpath);
		return fullpath;
	}
}

// enables the ObjectPtr property, not fully utilized by Timer, but certainly utilized by NonStandardAssets
[CustomPropertyDrawer(typeof(NonStandard.ObjectPtr))]
public class PropertyDrawer_ObjectPtr : PropertyDrawer
{
	delegate Object SelectNextObjectFunction();
	public static bool showLabel = true;
	public int choice = 0;
	string[] choices_name = { };
	SelectNextObjectFunction[] choices_selectFunc = { };
	Object choicesAreFor;
	System.Type[] possibleResponses;
	string[] cached_typeCreationList_names;
	SelectNextObjectFunction[] cached_TypeCreationList_function;

	public static string setToNull = "set to null", delete = "delete";
	public static float defaultOptionWidth = 16, defaultLabelWidth = 48, unitHeight = 16;
	/// <summary>The namespaces to get default selectable classes from</summary>
	protected virtual string[] GetNamespacesForNewComponentOptions() { return null; }

	public override float GetPropertyHeight(SerializedProperty _property, GUIContent label) {
		return StandardCalcPropertyHeight();
	}

	public static float StandardCalcPropertyHeight() {
		// SerializedProperty asset = _property.FindPropertyRelative("data");
		return unitHeight;//base.GetPropertyHeight (asset, label);
	}

	/// <summary>
	/// When the ObjectPtr points to nothing, this method generates the objects that can be created by default
	/// </summary>
	/// <param name="self">Self.</param>
	/// <param name="names">Names.</param>
	/// <param name="functions">Functions.</param>
	private void GenerateTypeCreationList(Component self, out string[] names, out SelectNextObjectFunction[] functions) {
		List<string> list = new List<string>();
		List<SelectNextObjectFunction> list_of_data = new List<SelectNextObjectFunction>();
		string[] theList = GetNamespacesForNewComponentOptions();
		if (theList != null) {
			for (int i = 0; i < theList.Length; ++i) {
				string namespaceName = theList[i];
				possibleResponses = NonStandard.Reflection.GetTypesInNamespace(namespaceName);
				list.AddRange(NonStandard.Reflection.TypeNamesCleaned(possibleResponses, namespaceName));
				for (int t = 0; t < possibleResponses.Length; t++) {
					System.Type nextT = possibleResponses[t];
					list_of_data.Add(() => {
						return CreateSelectedClass(nextT, self);
					});
				}
			}
		}
		list.Insert(0, (theList != null) ? "<-- select Object or create..." : "<-- select Object");
		list_of_data.Insert(0, null);
		names = list.ToArray();
		functions = list_of_data.ToArray();
	}

	private void CleanTypename(ref string typename) {
		int lastDot = typename.LastIndexOf('.');
		if (lastDot >= 0) { typename = typename.Substring(lastDot + 1); }
	}

	private void GenerateChoicesForSelectedObject(Component self, out string[] names, out SelectNextObjectFunction[] functions) {
		List<string> components = new List<string>();
		List<SelectNextObjectFunction> nextSelectionFunc = new List<SelectNextObjectFunction>();
		string typename = choicesAreFor.GetType().ToString();
		CleanTypename(ref typename);
		components.Add(typename);
		nextSelectionFunc.Add(null);
		GameObject go = choicesAreFor as GameObject;
		bool addSetToNull = true;
		Object addDelete = null;
		if (go != null) {
			Component[] c = go.GetComponents<Component>();
			for (int i = 0; i < c.Length; i++) {
				Component comp = c[i];
				if (comp != self) {
					typename = comp.GetType().ToString();
					CleanTypename(ref typename);
					components.Add(typename);
					nextSelectionFunc.Add(() => { return comp; });
				}
			}
			addSetToNull = true;
		} else if (choicesAreFor is Component) {
			components.Add(".gameObject");
			GameObject gob = (choicesAreFor as Component).gameObject;
			nextSelectionFunc.Add(() => { return gob; });
			addSetToNull = true;
			addDelete = choicesAreFor;
		}
		if (addSetToNull) {
			components.Add(setToNull);
			nextSelectionFunc.Add(() => {
				choice = 0; return null;
			});
		}
		if (addDelete != null) {
			components.Add(delete);
			nextSelectionFunc.Add(() => {
				//Object.DestroyImmediate(addDelete); 
				itemsToCleanup.Add(addDelete);
				choice = 0; return null;
			});
		}
		names = components.ToArray();
		functions = nextSelectionFunc.ToArray();
	}

	[ExecuteInEditMode]
	private class IndirectCleaner : MonoBehaviour
	{
		public List<Object> itemsToCleanup;
		private void Update() {
			for (int i = itemsToCleanup.Count - 1; i >= 0; --i) {
				Object o = itemsToCleanup[i];
				if (o != null) { Object.DestroyImmediate(o); }
			}
			itemsToCleanup.Clear();
			DestroyImmediate(this); // cleaning lady disposes of herself too
		}
	}
	private List<Object> itemsToCleanup = new List<Object>();
	private void RequestCleanup(Component self) {
		// if any items need to be deleted, don't do it now! the UI is in the middle of being drawn!
		// create a separate process that will do it for you
		IndirectCleaner cleaner = self.gameObject.AddComponent<IndirectCleaner>();
		cleaner.itemsToCleanup = this.itemsToCleanup;
	}

	/// <summary>called right after an object is assigned</summary>
	public virtual Object FilterImmidiate(Object obj, Component self) {
		return obj;
	}

	/// <summary>called right after a new component is created to be assigned</summary>
	protected virtual Object FilterNewComponent(System.Type nextT, Component self, Component newlyCreatedComponent) {
		return newlyCreatedComponent;
	}

	/// <summary>called just before UI is finished. This is the last chance to adjust the new setting.</summary>
	public virtual Object FilterFinal(Object newObjToReference, Object prevObj, Component self) {
		return newObjToReference;
	}

	private Object CreateSelectedClass(System.Type nextT, Component self) {
		Object obj = null;
		if (self != null && self.gameObject != null) {
			GameObject go = self.gameObject;
			if (nextT.IsSubclassOf(typeof(ScriptableObject))) {
				obj = ScriptableObjectUtility.CreateAsset(nextT);
			} else {
				Component newComponent = go.AddComponent(nextT);
				obj = FilterNewComponent(nextT, self, newComponent);
			}
		}
		return obj;
	}

	public static SerializedProperty ObjectPtrAsset(SerializedProperty _property) {
		SerializedProperty asset = _property.FindPropertyRelative("data");
		//asset = asset.FindPropertyRelative("data");
		return asset;
	}

	public override void OnGUI(Rect _position, SerializedProperty _property, GUIContent _label) {
		EditorGUI.BeginProperty(_position, GUIContent.none, _property);
		SerializedProperty asset = ObjectPtrAsset(_property);
		int oldIndent = EditorGUI.indentLevel;
		EditorGUI.indentLevel = 0;
		if (PropertyDrawer_ObjectPtr.showLabel) {
			_position = EditorGUI.PrefixLabel(_position, GUIUtility.GetControlID(FocusType.Passive), _label);
		}
		Component self = _property.serializedObject.targetObject as Component;
		if (asset != null) {
			Object prevObj = asset.objectReferenceValue;
			asset.objectReferenceValue = EditorGUIObjectReference(_position, asset.objectReferenceValue, self);
			asset.objectReferenceValue = FilterFinal(asset.objectReferenceValue, prevObj, self);
			//Contingentable cself = self as Contingentable;
			//if(prevObj != asset.objectReferenceValue && cself != null && cself.ContingencyRecursionCheck() != null) {
			//	Debug.LogWarning("Disallowing recursion of " + asset.objectReferenceValue);
			//	asset.objectReferenceValue = prevObj;
			//}
		}
		EditorGUI.indentLevel = oldIndent;
		EditorGUI.EndProperty();
		if (itemsToCleanup.Count != 0) { RequestCleanup(self); }
	}

	// TODO rename this to DoGUI
	public virtual Object EditorGUIObjectReference(Rect _position, Object obj, Component self) {
		int oldIndent = EditorGUI.indentLevel;
		EditorGUI.indentLevel = 0;
		obj = StandardEditorGUIObjectReference(_position, obj, self);
		EditorGUI.indentLevel = oldIndent;
		return obj;
	}

	public Object ShowObjectPtrChoicesPopup(Rect _position, Object obj, Component self, bool recalculateChoices) {
		// if the object needs to have it's alternate forms calculated
		if (recalculateChoices || choicesAreFor != obj || choices_name.Length == 0) {
			choicesAreFor = obj;
			// if these choices are for an actual object
			if (choicesAreFor != null) {
				GenerateChoicesForSelectedObject(self, out choices_name, out choices_selectFunc);
				choice = 0;
			} else {
				if (cached_typeCreationList_names == null) {
					GenerateTypeCreationList(self,
						out cached_typeCreationList_names, out cached_TypeCreationList_function);
				}
				choices_name = cached_typeCreationList_names;
				choices_selectFunc = cached_TypeCreationList_function;
			}
		}
		// give the alternate options for the object
		int lastChoice = choice;
		_position.x += _position.width;
		_position.width = defaultOptionWidth;
		choice = EditorGUI.Popup(_position, choice, choices_name);
		if (lastChoice != choice) {
			if (choices_selectFunc[choice] != null) {
				obj = choices_selectFunc[choice]();
			}
		}
		return obj;
	}

	public Object StandardEditorGUIObjectReference(Rect _position, Object obj, Component self) {
		float originalWidth = _position.width;
		_position.width = originalWidth - defaultOptionWidth;
		Object prevSelection = obj;
		obj = EditorGUI.ObjectField(_position, obj, typeof(Object), true);
		obj = FilterImmidiate(obj, self);
		obj = ShowObjectPtrChoicesPopup(_position, obj, self, obj != prevSelection);
		return obj;
	}

	public Object DoGUIEnumLabeledString<T>(Rect _position, Object obj, Component self,
		ref T enumValue, ref string textValue) {
		int oldindent = EditorGUI.indentLevel;
		EditorGUI.indentLevel = 0;
		Rect r = _position;
		float w = defaultOptionWidth, wl = defaultLabelWidth;
		r.width = wl;
		enumValue = NonStandard.Reflection.EditorGUI_EnumPopup<T>(r, enumValue);
		r.x += r.width;
		r.width = _position.width - w - wl;
		textValue = EditorGUI.TextField(r, textValue);
		obj = ShowObjectPtrChoicesPopup(r, obj, self, true);
		r.x += r.width;
		r.width = w;
		EditorGUI.indentLevel = oldindent;
		return obj;
	}
	public Object DoGUIEnumLabeledObject<T>(Rect _position, Object obj, Component self,
		ref T enumValue, ref Object objectValue) {
		int oldindent = EditorGUI.indentLevel;
		EditorGUI.indentLevel = 0;
		Rect r = _position;
		float w = defaultOptionWidth, wl = defaultLabelWidth;
		r.width = wl;
		enumValue = NonStandard.Reflection.EditorGUI_EnumPopup<T>(r, enumValue);
		r.x += r.width;
		r.width = _position.width - w - wl;
		objectValue = EditorGUI.ObjectField(r, objectValue, typeof(Object), true);
		obj = FilterImmidiate(obj, self);
		obj = ShowObjectPtrChoicesPopup(r, obj, self, true);
		r.x += r.width;
		r.width = w;
		EditorGUI.indentLevel = oldindent;
		return obj;
	}
}
#endif