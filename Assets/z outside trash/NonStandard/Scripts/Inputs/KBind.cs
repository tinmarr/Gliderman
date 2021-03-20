using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.Events;
using System.Text;
#if UNITY_EDITOR
using UnityEditor.Events;
#endif

namespace NonStandard.Inputs {
	[Serializable]
	public class KBind : InputBind, IComparable<KBind> {
		/// <summary>
		/// smaller number is greater priority.
		/// </summary>
		public int priority = 1000;
		public bool disable;
		/// <summary>
		/// if true, can still be triggered after the key event is consumed
		/// </summary>
		public bool eventAlwaysTriggerable;

		public KCombination[] keyCombinations = new KCombination[1];

		[Serializable]
		public class EventSet {
			[SerializeField, ContextMenuItem("DoPress", "DoPress")] protected UnityEvent onPress;
			[SerializeField, ContextMenuItem("DoHold", "DoHold")] protected UnityEvent onHold;
			[SerializeField, ContextMenuItem("DoRelease", "DoRelease")] protected UnityEvent onRelease;

			/// <summary>
			/// return true if the action worked, false if it should be ignored
			/// </summary>
			private Func<bool> actionPress, actionHold, actionRelease;

			public int CountPress { get { return (onPress != null ? onPress.GetPersistentEventCount() : 0) + (actionPress != null ? actionPress.GetInvocationList().Length : 0); } }
			public int CountHold { get { return (onHold != null? onHold.GetPersistentEventCount() : 0) + (actionHold!=null? actionHold.GetInvocationList().Length : 0); } }
			public int CountRelease { get { return (onRelease != null? onRelease.GetPersistentEventCount() : 0) + (actionRelease != null? actionRelease.GetInvocationList().Length : 0); } }

			public void AddPress(Func<bool> a) { if (actionPress != null) { actionPress += a; } else { actionPress = a; } }
			public void AddHold(Func<bool> a) { if (actionHold != null) { actionHold += a; } else { actionHold = a; } }
			public void AddRelease(Func<bool> a) { if (actionRelease != null) { actionRelease += a; } else { actionRelease = a; } }
			public void AddPress(object target, string setMethodName, object value) { AddPress(new InvokeSet(target, setMethodName, value)); }
			public void AddHold(object target, string setMethodName, object value) { AddHold(new InvokeSet(target, setMethodName, value)); }
			public void AddRelease(object target, string setMethodName, object value) { AddRelease(new InvokeSet(target, setMethodName, value)); }
			public void AddPress(InvokeSet a) { if (onPress == null) { onPress = new UnityEvent(); } a.Bind(onPress); }
			public void AddHold(InvokeSet a) { if (onHold == null) { onHold = new UnityEvent(); } a.Bind(onHold); }
			public void AddRelease(InvokeSet a) { if (onRelease == null) { onRelease = new UnityEvent(); } a.Bind(onRelease); }
			public bool DoPress() { if(onPress != null)onPress.Invoke(); return (actionPress!=null?actionPress.Invoke() : false); }
			public bool DoHold() { if(onHold!=null) onHold.Invoke(); return (actionHold!= null?actionHold.Invoke() : false);  }
			public bool DoRelease() { if(onRelease!=null)onRelease.Invoke(); return (actionRelease!=null?actionRelease.Invoke() : false);  }
			public void RemovePresses() { onPress.RemoveAllListeners(); actionPress = null;}
			public void RemoveHolds() { onHold.RemoveAllListeners(); actionHold = null;}
			public void RemoveReleases() { onRelease.RemoveAllListeners(); actionRelease = null;}

			public void AddEvents(Func<bool> onPressEvent = null, Func<bool> onHoldEvent = null, Func<bool> onReleaseEvent = null,
				InvokeSet pressFunc = null, InvokeSet holdFunc = null, InvokeSet releaseFunc = null) {
				if (onPressEvent != null) { AddPress(onPressEvent); }
				if (onHoldEvent != null) { AddHold(onHoldEvent); }
				if (onReleaseEvent != null) { AddRelease(onReleaseEvent); }
				if (pressFunc != null) { AddPress(pressFunc); }
				if (holdFunc != null) { AddHold(holdFunc); }
				if (releaseFunc != null) { AddRelease(releaseFunc); }
			}
			internal static string FilterMethodName(string methodName) {
				if (methodName.StartsWith("set_") || methodName.StartsWith("get_")) { return methodName.Substring(4); }
				return methodName;
			}
        
			public string GetDelegateText(UnityEvent ue, Func<bool> a) {
				StringBuilder text = new StringBuilder();
				if (ue != null) {
					for (int i = 0; i < ue.GetPersistentEventCount(); ++i) {
						if (text.Length > 0) { text.Append("\n"); }
						UnityEngine.Object obj = ue.GetPersistentTarget(i);
						string t = obj!=null?obj.name : "<???>";
						text.Append(t).Append(".").Append(FilterMethodName(ue.GetPersistentMethodName(i)));
					}
				}
				if(a != null) {
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
				string holdText = GetDelegateText(onPress, actionPress);
				string pressText = GetDelegateText(onHold, actionHold);
				string releaseText = GetDelegateText(onRelease, actionRelease);
				text.Append(holdText);
				if (!string.IsNullOrEmpty(pressText)) {
					if (text.Length > 0) { text.Append("\n\n"); }
					text.Append("Press:\n").Append(pressText);
				}
				if (!string.IsNullOrEmpty(releaseText)) {
					if (text.Length > 0) { text.Append("\n\n"); }
					text.Append("Release:\n").Append(releaseText);
				}
				return text.ToString();
			}
		}

		[ContextMenuItem("DoActivateTrigger", "DoActivateTrigger")]
		public EventSet keyEvent = new EventSet();

		/// <summary>
		/// additional requirements for the input
		/// </summary>
		public Func<bool> additionalRequirement;

		public bool IsAllowed() { return additionalRequirement == null || additionalRequirement.Invoke(); }

		/// <summary>
		/// describes a function to execute when a specific key-combination is pressed
		/// </summary>
		public KBind(KCode key, Func<bool> onPressEvent, string name = null):this(key, name, onPressEvent) { }

		public static InvokeSet Func(object target, string setMethodName, object value = null) {
			return new InvokeSet(target, setMethodName, value);
		}

		public class InvokeSet {
			public object target;
			public string setMethodName;
			public object value;
			public InvokeSet(object target, string setMethodName, object value = null) {
				this.target = target; this.setMethodName = setMethodName; this.value = value;
			}
			public UnityAction<T> GetAction<T>(object target, string setMethodName) {
				System.Reflection.MethodInfo targetinfo = UnityEvent.GetValidMethodInfo(target, setMethodName, new Type[] { typeof(T) });
				if (targetinfo == null) { Debug.LogError("no method " + setMethodName + "("+typeof(T).Name+") in " + target.ToString()); }
				return Delegate.CreateDelegate(typeof(UnityAction<T>), target, targetinfo, false) as UnityAction<T>;
			}
			public void Bind(UnityEvent @event) {
#if UNITY_EDITOR
				if (value == null) {
					System.Reflection.MethodInfo targetinfo = UnityEvent.GetValidMethodInfo(target, setMethodName, new Type[0]);
					if (targetinfo == null) { Debug.LogError("no method " + setMethodName + "() in " + target.ToString()); }
					UnityAction action = Delegate.CreateDelegate(typeof(UnityAction), target, targetinfo, false) as UnityAction;
					UnityEventTools.AddVoidPersistentListener(@event, action);
				} else if(value is int) {
					UnityEventTools.AddIntPersistentListener(@event, GetAction<int>(target, setMethodName), (int)value);
				} else if (value is float) {
					UnityEventTools.AddFloatPersistentListener(@event, GetAction<float>(target, setMethodName), (float)value);
				} else if (value is string) {
					UnityEventTools.AddStringPersistentListener(@event, GetAction<string>(target, setMethodName), (string)value);
				} else if (value is bool) {
					UnityEventTools.AddBoolPersistentListener(@event, GetAction<bool>(target, setMethodName), (bool)value);
				} else if (value is GameObject) { Bind<GameObject>(@event);
				} else if (value is Transform) { Bind<Transform>(@event);
				} else {
					Debug.LogError("unable to assign " + value.GetType());
				}
#else
				System.Reflection.MethodInfo targetinfo = UnityEvent.GetValidMethodInfo(target, setMethodName, new Type[0]);
				@event.AddListener(() => targetinfo.Invoke(target, new object[] { value }));
#endif
			}
#if UNITY_EDITOR
			public void Bind<T>(UnityEvent @event) where T : UnityEngine.Object {
				if (value is T) {
					UnityEventTools.AddObjectPersistentListener(@event, GetAction<T>(target, setMethodName), (T)value);
				} else {
					Debug.LogError("unable to assign " + value.GetType());
				}
			}
#endif
		}

		/// <summary>
		/// describes functions to execute when a specific key is pressed/held/released
		/// </summary>
		public KBind(KCode key, string name = null, Func<bool> onPressEvent = null, Func<bool> onHoldEvent = null,
			Func<bool> onReleaseEvent = null, Func<bool> additionalRequirement = null, 
			bool eventAlwaysTriggerable = false, InvokeSet pressFunc=null, InvokeSet holdFunc=null, InvokeSet releaseFunc=null)
			: this(new KCombination(key), name, onPressEvent, onHoldEvent, onReleaseEvent, additionalRequirement, eventAlwaysTriggerable,pressFunc,holdFunc,releaseFunc) {
		}

		/// <summary>
		/// describes functions to execute when a specific key-combination is pressed/held/released
		/// </summary>
		public KBind(KCombination kCombo, string name = null, Func<bool> onPressEvent = null, Func<bool> onHoldEvent = null,
			Func<bool> onReleaseEvent = null, Func<bool> additionalRequirement = null, 
			bool eventAlwaysTriggerable = false, InvokeSet pressFunc = null, InvokeSet holdFunc = null, InvokeSet releaseFunc = null)
			: this(new[] {kCombo}, name, onPressEvent, onHoldEvent, onReleaseEvent, additionalRequirement, eventAlwaysTriggerable,pressFunc,holdFunc,releaseFunc) {
		}
    
		/// <summary>
		/// describes functions to execute when any of the specified key-combinations are pressed/held/released
		/// </summary>
		public KBind(KCombination[] kCombos, string name = null, Func<bool> onPressEvent = null, Func<bool> onHoldEvent = null,
			Func<bool> onReleaseEvent = null, Func<bool> additionalRequirement = null, 
			bool eventAlwaysTriggerable = false, InvokeSet pressFunc = null, InvokeSet holdFunc = null, InvokeSet releaseFunc = null) {
			keyCombinations = kCombos;
			Init();
			Array.Sort(keyCombinations); Array.Reverse(keyCombinations); // put least complex key bind first, backwards from usual processing
			this.name = name;
			keyEvent.AddEvents(onPressEvent, onHoldEvent, onReleaseEvent,pressFunc,holdFunc,releaseFunc);
			if (additionalRequirement != null) {
				this.additionalRequirement = additionalRequirement;
			}
			this.eventAlwaysTriggerable = eventAlwaysTriggerable;
		}

		public void Init() { Array.ForEach(keyCombinations, k => k.Init()); }

		public void AddComplexKeyPresses(KCombination[] keysToUse) {
			if (keyCombinations.Length == 0 || keyCombinations[0].key == KCode.None) { keyCombinations = keysToUse; } else {
				List<KCombination> currentKeys = new List<KCombination>(keyCombinations);
				currentKeys.AddRange(keysToUse);
				// remove duplicates
				for (int a = 0; a < currentKeys.Count; ++a) {
					for (int b = currentKeys.Count - 1; b > a; --b) {
						if (currentKeys[a].CompareTo(currentKeys[b]) == 0) {
							currentKeys.RemoveAt(b);
						}
					}
				}
				keyCombinations = currentKeys.ToArray();
			}
			Init();
			Array.Sort(keyCombinations); Array.Reverse(keyCombinations); // put least complex key bind first (reverse of usual processing)
		}
    
		public void AddKeyCombinations(KCombination[] keyCombo, string nameToUse, Func<bool> onPress = null, Func<bool> onHold = null, Func<bool> onRelease = null,
			InvokeSet setPress = null, InvokeSet setHold = null, InvokeSet setRelease = null) {
			if (keyCombo != null) { AddComplexKeyPresses(keyCombo); }
			if (string.IsNullOrEmpty(name)) { name = nameToUse; }
			keyEvent.AddEvents(onPress, onHold, onRelease, setPress, setHold, setRelease);
		}

		public void AddKeyBinding(KCode keyToUse, string nameToUse, Func<bool> onPress = null, Func<bool> onHold = null, Func<bool> onRelease = null) {
			AddKeyCombinations(new KCombination[]{new KCombination(keyToUse)}, nameToUse, onPress, onHold, onRelease);
		}
		public string ShortDescribe(string betweenKeyPresses = "\n") {
			if (keyCombinations == null || keyCombinations.Length == 0) return "";
			string text = "";
			for (int i = 0; i < keyCombinations.Length; ++i) {
				if (i > 0) text += betweenKeyPresses;
				text += keyCombinations[i].ToString();
			}
			return text;
		}

		public int CompareTo(KBind other) {
			if (other == null) return -1;
			// the simpler key binding, more likely to be pressed, should go first
			for (int i = 0; i < keyCombinations.Length; ++i) {
				if (other.keyCombinations.Length <= i) return 1;
				int cmp = keyCombinations[i].CompareTo(other.keyCombinations[i]);
				if (cmp == 0) { cmp = priority.CompareTo(other.priority); }
				if (cmp != 0) return cmp;
			}
			return 1;
		}

		public override string ToString() { return ShortDescribe(" || ") + " \""+name+"\""; }

		/// <returns>if the action succeeded (which may remove other actions from queue, due to priority)</returns>
		public bool DoPress() { return keyEvent.DoPress(); }

		/// <returns>if the action succeeded (which may remove other actions from queue, due to priority)</returns>
		public bool DoHold() { return keyEvent.DoHold(); }

		/// <returns>if the action succeeded (which may remove other actions from queue, due to priority)</returns>
		public bool DoRelease() { return keyEvent.DoRelease(); }

		public KCombination GetDown() {
			if (disable) return null;
			bool allowedChecked = false;
			for (int i = 0; i < keyCombinations.Length; ++i) {
				if (keyCombinations[i].IsSatisfiedDown()) {
					if (!allowedChecked) { if (!IsAllowed()) { return null; } allowedChecked = true; }
					return keyCombinations[i];
				}
			}
			return null;
		}
		public bool IsDown() { return GetDown() != null; }

		public KCombination GetHeld() {
			if (disable) return null;
			bool allowedChecked = false;
			for (int i = 0; i < keyCombinations.Length; ++i) {
				if (keyCombinations[i].IsSatisfiedHeld()) {
					if (!allowedChecked) { if (!IsAllowed()) { return null; } allowedChecked = true; }
					return keyCombinations[i];
				}
			}
			return null;
		}
		public bool IsHeld() { return GetHeld() != null; }

		public KCombination GetUp() {
			if (disable) return null;
			bool allowedChecked = false;
			for (int i = 0; i < keyCombinations.Length; ++i) {
				if (keyCombinations[i].IsSatisfiedUp()) {
					if (!allowedChecked) { if (!IsAllowed()) { return null; } allowedChecked = true; }
					return keyCombinations[i];
				}
			}
			return null;
		}
		public bool IsUp() { return GetUp() != null; }

		public void DoActivateTrigger() {
			if (keyEvent.CountPress > 0) { DoPress(); }
			else if (keyEvent.CountHold > 0) { DoHold(); }
			else if (keyEvent.CountRelease > 0) { DoRelease(); }
		}
	}

	[Serializable]
	public class KCombination : IComparable<KCombination> {
		/// <summary>
		/// the key that triggers this complex keypress
		/// </summary>
		public KCode key;
		public Modifier[] modifiers;

		public KCombination() { }

		public bool IsNone() { return key == KCode.None && (modifiers == null || modifiers.Length == 0); }
    
		public KCombination(KCode key) {
			this.key = key;
			modifiers = null;
		}

		public void Init() {
			key = key.Normalized();
			if (modifiers == null) return;
			for (int i = 0; i < modifiers.Length; ++i) { modifiers[i] = modifiers[i].Init(); }
		}

		public KCombination(KCode key, KCode modifier) : this(key) {
			AddModifier(modifier);
		}

		public KCombination(KCode key, params KCode[] modifiers) : this(key) {
			Array.ForEach(modifiers, m => AddModifier(m));
		}

		public int GetComplexity() { return modifiers != null ? modifiers.Length : 0; }
    
		public bool AddModifier(KCode kCode) {
			Modifier mod = new Modifier(kCode);
			if (HasModifier(mod)) { return false; }
			List<Modifier> mods = new List<Modifier>();
			if(modifiers != null) {mods.AddRange(modifiers);}
			mods.Add(mod);
			mods.Sort();
			modifiers = mods.ToArray();
			return true;
		}    

		public bool HasModifiers(Modifier[] mods) {
			if (modifiers == null || modifiers.Length != mods.Length) return false;
			for (int i = 0; i < mods.Length; ++i) {
				if (!HasModifier(mods[i])) return false;
			}
			return true;
		}

		public bool HasModifier(Modifier m) {
			if (modifiers == null || modifiers.Length == 0) return false;
			for (int i = 0; i < modifiers.Length; ++i) {
				if (modifiers[i].Equals(m)) return true;
			}
			return false;
		}
    
		[Serializable]
		public struct Modifier : IComparable<Modifier> {
			public KCode key;
			/// <param name="key"></param>
			public Modifier(KCode key) { this.key = key; }
        
			public override string ToString() { return key.NormalName(); }

			public int CompareTo(Modifier other) { return key.CompareTo(other.key); }

			public override bool Equals(object obj) {
				if (obj == null) return false;
				Modifier mod = (Modifier) obj;
				return mod.key == key;
			}

			public bool Equals(Modifier other) { return key == other.key; }

			public override int GetHashCode() { return (int) key; }

			public Modifier Init() { key = key.Normalized(); return this; }
		}
    
		public int CompareTo(KCombination other) {
			int comp = key.CompareTo(other.key);
			if (comp != 0) return comp;
			if (modifiers != null && other.modifiers != null) {
				for (int i = 0; i < modifiers.Length && i < other.modifiers.Length; ++i) {
					if (i >= other.modifiers.Length) return -1;
					if (i >= modifiers.Length) return 1;
					comp = modifiers[i].key.CompareTo(other.modifiers[i].key);
					if (comp != 0) return comp;
				}
			} else {
				int selfScore = modifiers != null?modifiers.Length : 0;
				int otherScore = other.modifiers != null? other.modifiers.Length : 0;
				return -selfScore.CompareTo(otherScore); // the more complex ComplexKeyPress should be first
			}
			return 0;
		}

		public override string ToString() {
			return ToString(modifiers)+ key.NormalName();
		}

		public static string ToString(Modifier[] modifiers) {
			StringBuilder text = new StringBuilder();
			if (modifiers != null) {
				for (int i = 0; i < modifiers.Length; ++i) {
					text.Append(modifiers[i]).Append("+");
				}
			}
			return text.ToString();
		}

		public static KCombination FromString(string s) {
			string[] keys = s.Split('+');
			int numMods = keys.Length - 1;
			KCombination kp = new KCombination();
			if (numMods > 0) {
				kp.modifiers = new Modifier[numMods];
				for (int i = 0; i < numMods; ++i) {
					string k = keys[i].Trim();
					kp.modifiers[i].key = k.ToEnum<KCode>();
				}
				kp.key = keys[keys.Length-1].Trim().ToEnum<KCode>();
			}
			return kp;
		}

		public static bool IsSatisfiedDown(Modifier[] modifiers, ref bool aKeyWasJustPressed) {
			for (int i = 0; i < modifiers.Length; ++i) {
				KState ks = modifiers[i].key.GetState();
				if (ks == KState.KeyReleased) {
					return false;
				}
				if (ks == KState.KeyDown) {
					aKeyWasJustPressed = true;
				}
			}
			return true;
		}
		public static bool IsSatisfiedHeld(Modifier[] modifiers) {
			for (int i = 0; i < modifiers.Length; ++i) {
				if (!modifiers[i].key.IsHeld()) { return false; }
			}
			return true;
		}
		public static bool IsSatisfiedUp(Modifier[] modifiers, ref bool anyKeyIsBeingReleased) {
			for (int i = 0; i < modifiers.Length; ++i) {
				KState ks = modifiers[i].key.GetState();
				if (ks == KState.KeyReleased) {
					return false;
				}
				if (ks == KState.KeyUp) {
					anyKeyIsBeingReleased = true;
				}
			}
			return true;
		}

		public bool IsSatisfiedDown() {
			KState ks = key.GetState();
			bool anyKeyIsBeingPressed = ks == KState.KeyDown;
			if (ks == KState.KeyReleased) return false;
			if(modifiers != null) {
				if(!IsSatisfiedDown(modifiers, ref anyKeyIsBeingPressed)) {
					return false;
				}
			}
			return anyKeyIsBeingPressed;
		}
		public bool IsSatisfiedHeld() {
			if (!key.IsHeld()) return false;
			if (modifiers != null) {
				if (!IsSatisfiedHeld(modifiers)) return false;
			}
			return true;
		}
		public bool IsSatisfiedUp() {
			KState ks = key.GetState();
			bool anyKeyIsBeingReleased = ks == KState.KeyUp;
			if (ks == KState.KeyReleased) return false;
			if (modifiers != null) {
				if(!IsSatisfiedUp(modifiers, ref anyKeyIsBeingReleased)) {
					return false;
				}
			}
			return anyKeyIsBeingReleased;
		}
	}

	public static class StringEnumExtension {
		public static EnumType ToEnum<EnumType>(this string text) { return (EnumType)Enum.Parse(typeof(EnumType), text, true); }
	}
}
