using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor.Events;
#endif

namespace NonStandard.Inputs {
	[System.Serializable]
	public class AxBind : InputBind {
		public bool disable;
		private bool wasAllowedLastFrame;
		[System.Serializable]
		public class UnityEventFloat : UnityEvent<float> { }

		/// <summary>
		/// name of the axis, e.g.: Horizontal, Vertical
		/// </summary>
		public Axis[] axis = new Axis[] { new Axis("Horizontal") };


		[ContextMenuItem("DoActivateTrigger", "DoActivateTrigger")]
		public EventSet axisEvent = new EventSet();

		[System.Serializable]
		public class EventSet {

			[SerializeField, ContextMenuItem("DoAxisChange", "DoAxisChangeEmpty")] public UnityEventFloat onAxisChange;

			public Func<float, bool> actionAxisChange;

			public int CountAxisChangeEvents {
				get { return (onAxisChange != null ? onAxisChange.GetPersistentEventCount() : 0) + (actionAxisChange != null ? actionAxisChange.GetInvocationList().Length : 0); }
			}
			public void AddAxisChangeEvent(Func<float, bool> a) {
				if (!Application.isPlaying) { Debug.LogWarning("cannot serialize callbacks, only use this method at runtime!"); }
				if (actionAxisChange != null) { actionAxisChange += a; } else { actionAxisChange = a; }
			}
			public bool DoAxisChange(float value) {
				if(onAxisChange != null) onAxisChange.Invoke(value);
				return (actionAxisChange != null) ? actionAxisChange.Invoke(value) : false;
			}
			public bool DoAxisChangeEmpty() { return DoAxisChange(0); }
			public void RemoveAxisChange() { onAxisChange.RemoveAllListeners(); actionAxisChange = null; }

			public string GetDelegateText(UnityEventFloat ue, Func<float, bool> a) {
				StringBuilder text = new StringBuilder();
				if (ue != null) {
					for (int i = 0; i < ue.GetPersistentEventCount(); ++i) {
						if (text.Length > 0) { text.Append("\n"); }
						UnityEngine.Object obj = ue.GetPersistentTarget(i);
						string t = obj != null ? obj.name : "<???>";
						text.Append(t).Append(".").Append(KBind.EventSet.FilterMethodName(ue.GetPersistentMethodName(i)));
					}
				}
				if (a != null) {
					Delegate[] delegates = a.GetInvocationList();
					for (int i = 0; i < delegates.Length; ++i) {
						if (text.Length > 0) { text.Append("\n"); }
						text.Append(delegates[i].Target).Append(".").Append(delegates[i].Method.Name);
					}
				}
				return text.ToString();
			}

			public string CalculateDescription() {
				StringBuilder text = new StringBuilder();
				string desc = GetDelegateText(onAxisChange, actionAxisChange);
				text.Append(desc);
				return text.ToString();
			}
		}

		/// <summary>
		/// additional requirements for the input
		/// </summary>
		public Func<bool> additionalRequirement;

		public bool IsAllowed() { return !disable && (additionalRequirement == null || additionalRequirement.Invoke()); }

		/// <summary>
		/// describes a function to execute when a specific key-combination is pressed
		/// </summary>
		public AxBind(string axis, Func<float, bool> onAxisEvent, string name = null) : this(axis, name, onAxisEvent) { }

		/// <summary>
		/// describes functions to execute when a specific key is pressed/held/released
		/// </summary>
		public AxBind(string axis, string name = null, Func<float, bool> onAxisEvent = null, Func<bool> additionalRequirement = null)
			: this(new Axis(axis), name, onAxisEvent, additionalRequirement) {
		}

		/// <summary>
		/// describes functions to execute when a specific key-combination is pressed/held/released
		/// </summary>
		public AxBind(Axis axis, string name = null, Func<float, bool> onAxisEvent = null, Func<bool> additionalRequirement = null)
			: this(new[] { axis }, name, onAxisEvent, additionalRequirement) {
		}

		public AxBind(Axis axis, string name, object target, string setFloatMethodName, Func<bool> additionalRequirement = null)
			: this(new[] { axis }, name, null, additionalRequirement) {
			AddListener(target, setFloatMethodName);
		}

		public void AddListener(object target, string setFloatMethodName) {
			System.Reflection.MethodInfo targetinfo = UnityEvent.GetValidMethodInfo(target, setFloatMethodName, new Type[] { typeof(float) });
			if(targetinfo == null) {
				Debug.LogError("no method " + setFloatMethodName + " in " + target.ToString());
			}
			UnityAction<float> action = Delegate.CreateDelegate(typeof(UnityAction<float>), target, targetinfo, false) as UnityAction<float>;
			if (axisEvent.onAxisChange == null) {
				axisEvent.onAxisChange = new AxBind.UnityEventFloat();
			}
#if UNITY_EDITOR
			UnityEventTools.AddPersistentListener(axisEvent.onAxisChange, action);
#else
			axisEvent.onAxisChange.AddListener(f => { targetinfo.Invoke(target, new object[] { f }); });
#endif
		}

		/// <summary>
		/// describes functions to execute when any of the specified key-combinations are pressed/held/released
		/// </summary>
		public AxBind(Axis[] axis, string name = null, Func<float, bool> onAxisEvent = null, Func<bool> additionalRequirement = null) {
			this.axis = axis;
			Init();
			this.name = name;
			AddEvents(onAxisEvent);
			if (additionalRequirement != null) {
				this.additionalRequirement = additionalRequirement;
			}
		}

		public void Init() { Array.ForEach(axis, ax => {
			ax.Init();
#if UNITY_EDITOR
			if(ax.multiplier == 0) {
				Debug.LogWarning("AxisBind->"+name+"->"+ax.name+" has a zero multiplier. Was this intentional?");
			}
#endif
		}); }

		public void AddEvents(Func<float,bool> onAxisEvent = null) {
			if (onAxisEvent != null) { axisEvent.AddAxisChangeEvent(onAxisEvent); }
		}

		public void AddAxis(Axis[] axisToUse) {
			if (axis.Length == 0) { axis = axisToUse; } else {
				List<Axis> currentAxis = new List<Axis>(axis);
				currentAxis.AddRange(axisToUse);
				// remove duplicates
				for (int a = 0; a < currentAxis.Count; ++a) {
					for (int b = currentAxis.Count - 1; b > a; --b) {
						if (currentAxis[a].CompareTo(currentAxis[b]) == 0) {
							currentAxis.RemoveAt(b);
						}
					}
				}
				axis = currentAxis.ToArray();
			}
			Init();
		}

		public void AddAxis(Axis[] axisToAdd, string nameToUse, Func<float,bool> onAxis = null) {
			if (axisToAdd != null) { AddAxis(axisToAdd); }
			if (string.IsNullOrEmpty(name)) { name = nameToUse; }
			AddEvents(onAxis);
		}

		public void AddAxis(string axisName, string nameToUse, Func<float, bool> onAxis = null) {
			AddAxis(new Axis[] { new Axis(axisName) }, nameToUse, onAxis);
		}
		public string ShortDescribe(string betweenKeyPresses = "\n") {
			if (axis == null || axis.Length == 0) return "";
			string text = "";
			for (int i = 0; i < axis.Length; ++i) {
				if (i > 0) text += betweenKeyPresses;
				text += axis[i].ToString();
			}
			return text;
		}

		public override string ToString() { return ShortDescribe(" || ")+" \""+name+"\""; }

		/// <returns>if the action succeeded (which may remove other actions from queue, due to priority)</returns>
		public bool DoAxis(float value) { return axisEvent.DoAxisChange(value); }

		public Axis GetActiveAxis() {
			bool allowedChecked = false;
			for (int i = 0; i < axis.Length; ++i) {
				if (axis[i].IsValueChanged()) {
					if (!allowedChecked) { if (!IsAllowed()) { return null; } allowedChecked = true; }
					axis[i].MarkValueAsKnown();
					return axis[i];
				}
			}
			return null;
		}
		public bool IsActive() { return GetActiveAxis() != null; }

		public void DoActivateTrigger() {
			if (axisEvent.CountAxisChangeEvents > 0) { DoAxis(0); }
		}

		public void Update() {
			bool allowed = IsAllowed();
			if (!allowed) {
				if (wasAllowedLastFrame) {
					wasAllowedLastFrame = false;
					ClearNonStickyInput();
				}
				return;
			}
			wasAllowedLastFrame = allowed;
			for (int i = 0; i < axis.Length; ++i) {
				Axis ax = axis[i];
				if (ax.IsValueChanged()) {
					ax.MarkValueAsKnown();
					float value = ax.cachedValue * ax.multiplier;
					DoAxis(value);
					break;
				}
			}
		}

		private void ClearNonStickyInput() {
			for (int i = 0; i < axis.Length; ++i) {
				Axis ax = axis[i];
				if (!ax.stickyInput) { ax.cachedValue = 0; }
			}
		}
	}

	[System.Serializable]
	public class Axis : IComparable<Axis> {
		public string name;
		public float multiplier = 1;
		private float knownValue = -1;
		/// <summary>
		/// if true, use software dampened value, instead of instantaneous raw value
		/// </summary>
		public bool filteredValue = false;
		/// <summary>
		/// if true, value will remain unchanged when axis is disabled
		/// </summary>
		public bool stickyInput = false;
		[HideInInspector] public float cachedValue;

		public KCombination.Modifier[] modifiers;

		public Axis(string name, float multiplier = 1) { this.name = name; this.multiplier = multiplier; }

		public bool IsValueChanged() {
			bool isAllowed = modifiers == null || modifiers.Length == 0 || KCombination.IsSatisfiedHeld(modifiers);
			if (!isAllowed) {
				if (!stickyInput) { cachedValue = 0; }
			} else {
				cachedValue = filteredValue ? GetValue() : GetValueRaw();
			}
			return (cachedValue != knownValue);
		}

		public void MarkValueAsKnown() { knownValue = cachedValue; }

		public float GetValue() { return Input.GetAxis(name); }
		public float GetValueRaw() { return Input.GetAxisRaw(name); }

		public int CompareTo(Axis other) { return name.CompareTo(other.name); }

		public void Init() { }

		public override string ToString() { return KCombination.ToString(modifiers)+name; }
	}
}