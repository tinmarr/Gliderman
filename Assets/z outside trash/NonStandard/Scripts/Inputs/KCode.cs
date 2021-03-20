namespace NonStandard.Inputs {
  public static class KCodeExtension {
	public static KCode Normalized(this KCode k) {
		switch (k) {
		case KCode.AltGr: case KCode.LeftAlt: case KCode.RightAlt: return KCode.AnyAlt;
		case KCode.LeftShift: case KCode.RightShift: return KCode.AnyShift;
		case KCode.LeftApple: case KCode.RightApple: return KCode.LeftApple;
		case KCode.LeftWindows: case KCode.RightWindows: return KCode.LeftWindows;
		case KCode.LeftControl: case KCode.RightControl: return KCode.AnyControl;
		}
		return k;
	}
	public static string NormalName(this KCode k) {
      switch (k) {
      case KCode.AnyAlt:
      case KCode.AltGr:
      case KCode.LeftAlt:
      case KCode.RightAlt:
#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
      return "Option";
#else
      return "Alt";
#endif
      case KCode.AnyShift: case KCode.LeftShift: case KCode.RightShift: return "Shift";
      case KCode.LeftApple: case KCode.RightApple: return "Apple";
      case KCode.LeftWindows: case KCode.RightWindows: return "Windows";
      case KCode.AnyControl: case KCode.LeftControl: case KCode.RightControl: return "Ctrl";
      case KCode.Mouse0: return "Left Click";
      case KCode.Mouse1: return "Right Click";
      case KCode.Mouse2: return "Middle Click";
      case KCode.MouseXUp: return "Mouse Right";
      case KCode.MouseXDown: return "Mouse Left";
      case KCode.MouseYUp: return "Mouse Up";
      case KCode.MouseYDown: return "Mouse Down";
      }
      return k.ToString();
    }
	public static bool IsDown(this KCode kCode) {return AppInput.GetKeyDown(kCode);}
	public static bool IsUp(this KCode kCode) {return AppInput.GetKeyUp(kCode);}
	public static bool IsHeld(this KCode kCode) {return AppInput.GetKey(kCode);}

	public static KState GetState(this KCode kCode) {
      // prevent two-finger-right-click on touch screens, it messes with other right-click behaviour
      if(kCode == KCode.Mouse1 && UnityEngine.Input.touches != null && UnityEngine.Input.touches.Length >= 2)
        return KState.KeyReleased;
      return AppInput.GetKeyDown(kCode) ? KState.KeyDown :
		AppInput.GetKey(kCode) ? KState.KeyHeld :
		AppInput.GetKeyUp(kCode) ? KState.KeyUp :
		KState.KeyReleased;
	}
  }
  /// <summary>
  /// named after the Unity Input.Get___ methods (except KeyReleased)
  /// </summary>
  public enum KState { KeyReleased, KeyDown, KeyHeld, KeyUp }

	/// <summary>
	///   built to be an extension for the UnityEngine.KeyCode enumerator
	///   <para>Key codes returned by Event.keyCode. These map directly to a physical key on the keyboard.</para>
	/// </summary>
  public enum KCode
  {
    /// <summary>
    ///   <para>Not assigned (never returned as the result of a keystroke).</para>
    /// </summary>
    None = 0,

    // UNUSED = 1, // 0x00000001
    // UNUSED = 2, // 0x00000001
    // UNUSED = 3, // 0x00000001
    // UNUSED = 4, // 0x00000001
    // UNUSED = 5, // 0x00000001
    // UNUSED = 6, // 0x00000001
    // UNUSED = 7, // 0x00000001
    
    /// <summary>
    ///   <para>The backspace key.</para>
    /// </summary>
    Backspace = 8,
    /// <summary>
    ///   <para>The tab key.</para>
    /// </summary>
    Tab = 9,

    // UNUSED = 10, // 0x0000000A
    // UNUSED = 11, // 0x0000000B

    /// <summary>
    ///   <para>The Clear key.</para>
    /// </summary>
    Clear = 12, // 0x0000000C
    /// <summary>
    ///   <para>Return key.</para>
    /// </summary>
    Return = 13, // 0x0000000D

    // UNUSED = 14, // 0x0000000E
    // UNUSED = 15 // 0x0000000F
    // UNUSED = 16, // 0x00000010
    // UNUSED = 17, // 0x00000011
    // UNUSED = 18, // 0x00000012
    
    /// <summary>
    ///   <para>Pause on PC machines.</para>
    /// </summary>
    Pause = 19, // 0x00000013

    // UNUSED = 20, // 0x00000014
    // UNUSED = 21, // 0x00000015
    // UNUSED = 22, // 0x00000016
    // UNUSED = 23, // 0x00000017
    // UNUSED = 24, // 0x00000018
    // UNUSED = 25, // 0x00000019
    // UNUSED = 26, // 0x0000001A

    /// <summary>
    ///   <para>Escape key.</para>
    /// </summary>
    Escape = 27, // 0x0000001B
    
    MouseXUp = 28, // 0x0000001C
    MouseXDown = 29, // 0x0000001D
    MouseYUp = 30, // 0x0000001E
    MouseYDown = 31, // 0x0000001F
    
    /// <summary>
    ///   <para>Space key.</para>
    /// </summary>
    Space = 32, // 0x00000020
    /// <summary>
    ///   <para>Exclamation mark key '!'.</para>
    /// </summary>
    Exclaim = 33, // 0x00000021
    /// <summary>
    ///   <para>Double quote key '"'.</para>
    /// </summary>
    DoubleQuote = 34, // 0x00000022
    /// <summary>
    ///   <para>Hash key '#'.</para>
    /// </summary>
    Hash = 35, // 0x00000023
    /// <summary>
    ///   <para>Dollar sign key '$'.</para>
    /// </summary>
    Dollar = 36, // 0x00000024
    /// <summary>
    ///   <para>Percent '%' key.</para>
    /// </summary>
    Percent = 37, // 0x00000025
    /// <summary>
    ///   <para>Ampersand key '&amp;'.</para>
    /// </summary>
    Ampersand = 38, // 0x00000026
    /// <summary>
    ///   <para>Quote key '.</para>
    /// </summary>
    Quote = 39, // 0x00000027
    /// <summary>
    ///   <para>Left Parenthesis key '('.</para>
    /// </summary>
    LeftParen = 40, // 0x00000028
    /// <summary>
    ///   <para>Right Parenthesis key ')'.</para>
    /// </summary>
    RightParen = 41, // 0x00000029
    /// <summary>
    ///   <para>Asterisk key '*'.</para>
    /// </summary>
    Asterisk = 42, // 0x0000002A
    /// <summary>
    ///   <para>Plus key '+'.</para>
    /// </summary>
    Plus = 43, // 0x0000002B
    /// <summary>
    ///   <para>Comma ',' key.</para>
    /// </summary>
    Comma = 44, // 0x0000002C
    /// <summary>
    ///   <para>Minus '-' key.</para>
    /// </summary>
    Minus = 45, // 0x0000002D
    /// <summary>
    ///   <para>Period '.' key.</para>
    /// </summary>
    Period = 46, // 0x0000002E
    /// <summary>
    ///   <para>Slash '/' key.</para>
    /// </summary>
    Slash = 47, // 0x0000002F
    /// <summary>
    ///   <para>The '0' key on the top of the alphanumeric keyboard.</para>
    /// </summary>
    Alpha0 = 48, // 0x00000030
    /// <summary>
    ///   <para>The '1' key on the top of the alphanumeric keyboard.</para>
    /// </summary>
    Alpha1 = 49, // 0x00000031
    /// <summary>
    ///   <para>The '2' key on the top of the alphanumeric keyboard.</para>
    /// </summary>
    Alpha2 = 50, // 0x00000032
    /// <summary>
    ///   <para>The '3' key on the top of the alphanumeric keyboard.</para>
    /// </summary>
    Alpha3 = 51, // 0x00000033
    /// <summary>
    ///   <para>The '4' key on the top of the alphanumeric keyboard.</para>
    /// </summary>
    Alpha4 = 52, // 0x00000034
    /// <summary>
    ///   <para>The '5' key on the top of the alphanumeric keyboard.</para>
    /// </summary>
    Alpha5 = 53, // 0x00000035
    /// <summary>
    ///   <para>The '6' key on the top of the alphanumeric keyboard.</para>
    /// </summary>
    Alpha6 = 54, // 0x00000036
    /// <summary>
    ///   <para>The '7' key on the top of the alphanumeric keyboard.</para>
    /// </summary>
    Alpha7 = 55, // 0x00000037
    /// <summary>
    ///   <para>The '8' key on the top of the alphanumeric keyboard.</para>
    /// </summary>
    Alpha8 = 56, // 0x00000038
    /// <summary>
    ///   <para>The '9' key on the top of the alphanumeric keyboard.</para>
    /// </summary>
    Alpha9 = 57, // 0x00000039
    /// <summary>
    ///   <para>Colon ':' key.</para>
    /// </summary>
    Colon = 58, // 0x0000003A
    /// <summary>
    ///   <para>Semicolon ';' key.</para>
    /// </summary>
    Semicolon = 59, // 0x0000003B
    /// <summary>
    ///   <para>Less than '&lt;' key.</para>
    /// </summary>
    Less = 60, // 0x0000003C
    /// <summary>
    ///   <para>Equals '=' key.</para>
    /// </summary>
    Equals = 61, // 0x0000003D
    /// <summary>
    ///   <para>Greater than '&gt;' key.</para>
    /// </summary>
    Greater = 62, // 0x0000003E
    /// <summary>
    ///   <para>Question mark '?' key.</para>
    /// </summary>
    Question = 63, // 0x0000003F
    /// <summary>
    ///   <para>At key '@'.</para>
    /// </summary>
    At = 64, // 0x00000040
    
    // UNUSED = 65, // a 0x00000041
    // UNUSED = 66, // b 0x00000042
    // UNUSED = 67, // c 0x00000043
    // UNUSED = 68, // d 0x00000044
    // UNUSED = 69, // e 0x00000045
    // UNUSED = 70, // f 0x00000046
    // UNUSED = 71, // g 0x00000047
    // UNUSED = 72, // h 0x00000048
    // UNUSED = 73, // i 0x00000049
    // UNUSED = 74, // j 0x0000004A
    // UNUSED = 75, // k 0x0000004B
    // UNUSED = 76, // l 0x0000004C
    // UNUSED = 77, // m 0x0000004D
    // UNUSED = 78, // n 0x0000004E
    // UNUSED = 79, // o 0x0000004F
    // UNUSED = 80, // p 0x00000050
    // UNUSED = 81, // q 0x00000051
    // UNUSED = 82, // r 0x00000052
    // UNUSED = 83, // s 0x00000053
    // UNUSED = 84, // t 0x00000054
    // UNUSED = 85, // u 0x00000055
    // UNUSED = 86, // v 0x00000056
    // UNUSED = 87, // w 0x00000057
    // UNUSED = 88, // x 0x00000058
    // UNUSED = 89, // y 0x00000059
    // UNUSED = 90, // z 0x0000005A

    /// <summary>
    ///   <para>Left square bracket key '['.</para>
    /// </summary>
    LeftBracket = 91, // 0x0000005B
    /// <summary>
    ///   <para>Backslash key '\'.</para>
    /// </summary>
    Backslash = 92, // 0x0000005C
    /// <summary>
    ///   <para>Right square bracket key ']'.</para>
    /// </summary>
    RightBracket = 93, // 0x0000005D
    /// <summary>
    ///   <para>Caret key '^'.</para>
    /// </summary>
    Caret = 94, // 0x0000005E
    /// <summary>
    ///   <para>Underscore '_' key.</para>
    /// </summary>
    Underscore = 95, // 0x0000005F
    /// <summary>
    ///   <para>Back quote key '`'.</para>
    /// </summary>
    BackQuote = 96, // 0x00000060
    /// <summary>
    ///   <para>'a' key.</para>
    /// </summary>
    A = 97, // 0x00000061
    /// <summary>
    ///   <para>'b' key.</para>
    /// </summary>
    B = 98, // 0x00000062
    /// <summary>
    ///   <para>'c' key.</para>
    /// </summary>
    C = 99, // 0x00000063
    /// <summary>
    ///   <para>'d' key.</para>
    /// </summary>
    D = 100, // 0x00000064
    /// <summary>
    ///   <para>'e' key.</para>
    /// </summary>
    E = 101, // 0x00000065
    /// <summary>
    ///   <para>'f' key.</para>
    /// </summary>
    F = 102, // 0x00000066
    /// <summary>
    ///   <para>'g' key.</para>
    /// </summary>
    G = 103, // 0x00000067
    /// <summary>
    ///   <para>'h' key.</para>
    /// </summary>
    H = 104, // 0x00000068
    /// <summary>
    ///   <para>'i' key.</para>
    /// </summary>
    I = 105, // 0x00000069
    /// <summary>
    ///   <para>'j' key.</para>
    /// </summary>
    J = 106, // 0x0000006A
    /// <summary>
    ///   <para>'k' key.</para>
    /// </summary>
    K = 107, // 0x0000006B
    /// <summary>
    ///   <para>'l' key.</para>
    /// </summary>
    L = 108, // 0x0000006C
    /// <summary>
    ///   <para>'m' key.</para>
    /// </summary>
    M = 109, // 0x0000006D
    /// <summary>
    ///   <para>'n' key.</para>
    /// </summary>
    N = 110, // 0x0000006E
    /// <summary>
    ///   <para>'o' key.</para>
    /// </summary>
    O = 111, // 0x0000006F
    /// <summary>
    ///   <para>'p' key.</para>
    /// </summary>
    P = 112, // 0x00000070
    /// <summary>
    ///   <para>'q' key.</para>
    /// </summary>
    Q = 113, // 0x00000071
    /// <summary>
    ///   <para>'r' key.</para>
    /// </summary>
    R = 114, // 0x00000072
    /// <summary>
    ///   <para>'s' key.</para>
    /// </summary>
    S = 115, // 0x00000073
    /// <summary>
    ///   <para>'t' key.</para>
    /// </summary>
    T = 116, // 0x00000074
    /// <summary>
    ///   <para>'u' key.</para>
    /// </summary>
    U = 117, // 0x00000075
    /// <summary>
    ///   <para>'v' key.</para>
    /// </summary>
    V = 118, // 0x00000076
    /// <summary>
    ///   <para>'w' key.</para>
    /// </summary>
    W = 119, // 0x00000077
    /// <summary>
    ///   <para>'x' key.</para>
    /// </summary>
    X = 120, // 0x00000078
    /// <summary>
    ///   <para>'y' key.</para>
    /// </summary>
    Y = 121, // 0x00000079
    /// <summary>
    ///   <para>'z' key.</para>
    /// </summary>
    Z = 122, // 0x0000007A
    /// <summary>
    ///   <para>Left curly bracket key '{'.</para>
    /// </summary>
    LeftCurlyBracket = 123, // 0x0000007B
    /// <summary>
    ///   <para>Pipe '|' key.</para>
    /// </summary>
    Pipe = 124, // 0x0000007C
    /// <summary>
    ///   <para>Right curly bracket key '}'.</para>
    /// </summary>
    RightCurlyBracket = 125, // 0x0000007D
    /// <summary>
    ///   <para>Tilde '~' key.</para>
    /// </summary>
    Tilde = 126, // 0x0000007E
    /// <summary>
    ///   <para>The forward delete key.</para>
    /// </summary>
    Delete = 127, // 0x0000007F
    
    _UserDefined0 = 128, // 0x00000080
    _UserDefined1 = _UserDefined0+1, // 0x00000081
    _UserDefined2 = _UserDefined0+2, // 0x00000082
    _UserDefined3 = _UserDefined0+3, // 0x00000083
    _UserDefined4 = _UserDefined0+4, // 0x00000084
    _UserDefined5 = _UserDefined0+5, // 0x00000085
    _UserDefined6 = _UserDefined0+6, // 0x00000086
    _UserDefined7 = _UserDefined0+7, // 0x00000087
    _UserDefined8 = _UserDefined0+8, // 0x00000088
    _UserDefined9 = _UserDefined0+9, // 0x00000089
    _UserDefined10 = _UserDefined0+10, // 0x0000008A
    _UserDefined11 = _UserDefined0+11, // 0x0000008B
    _UserDefined12 = _UserDefined0+12, // 0x0000008C
    _UserDefined13 = _UserDefined0+13, // 0x0000008D
    _UserDefined14 = _UserDefined0+14, // 0x0000008E
    _UserDefined15 = _UserDefined0+15, // 0x0000008F

    // UNUSED = 144, // 0x00000090
    // UNUSED = 145, // 0x00000091
    // UNUSED = 146, // 0x00000092
    // UNUSED = 147, // 0x00000093
    // UNUSED = 148, // 0x00000094
    // UNUSED = 149, // 0x00000095
    // UNUSED = 150, // 0x00000096
    // UNUSED = 151, // 0x00000097
    // UNUSED = 152, // 0x00000098
    // UNUSED = 153, // 0x00000099
    // UNUSED = 154, // 0x0000009A
    // UNUSED = 155, // 0x0000009B
    // UNUSED = 156, // 0x0000009C
    // UNUSED = 157, // 0x0000009D
    // UNUSED = 158, // 0x0000009E
    // UNUSED = 159, // 0x0000009F
    // UNUSED = 160, // 0x000000A0

    /// <summary>
    ///   <para>Numeric keypad 0.</para>
    /// </summary>
    Keypad0 = 256, // 0x00000100
    /// <summary>
    ///   <para>Numeric keypad 1.</para>
    /// </summary>
    Keypad1 = 257, // 0x00000101
    /// <summary>
    ///   <para>Numeric keypad 2.</para>
    /// </summary>
    Keypad2 = 258, // 0x00000102
    /// <summary>
    ///   <para>Numeric keypad 3.</para>
    /// </summary>
    Keypad3 = 259, // 0x00000103
    /// <summary>
    ///   <para>Numeric keypad 4.</para>
    /// </summary>
    Keypad4 = 260, // 0x00000104
    /// <summary>
    ///   <para>Numeric keypad 5.</para>
    /// </summary>
    Keypad5 = 261, // 0x00000105
    /// <summary>
    ///   <para>Numeric keypad 6.</para>
    /// </summary>
    Keypad6 = 262, // 0x00000106
    /// <summary>
    ///   <para>Numeric keypad 7.</para>
    /// </summary>
    Keypad7 = 263, // 0x00000107
    /// <summary>
    ///   <para>Numeric keypad 8.</para>
    /// </summary>
    Keypad8 = 264, // 0x00000108
    /// <summary>
    ///   <para>Numeric keypad 9.</para>
    /// </summary>
    Keypad9 = 265, // 0x00000109
    /// <summary>
    ///   <para>Numeric keypad '.'.</para>
    /// </summary>
    KeypadPeriod = 266, // 0x0000010A
    /// <summary>
    ///   <para>Numeric keypad '/'.</para>
    /// </summary>
    KeypadDivide = 267, // 0x0000010B
    /// <summary>
    ///   <para>Numeric keypad '*'.</para>
    /// </summary>
    KeypadMultiply = 268, // 0x0000010C
    /// <summary>
    ///   <para>Numeric keypad '-'.</para>
    /// </summary>
    KeypadMinus = 269, // 0x0000010D
    /// <summary>
    ///   <para>Numeric keypad '+'.</para>
    /// </summary>
    KeypadPlus = 270, // 0x0000010E
    /// <summary>
    ///   <para>Numeric keypad Enter.</para>
    /// </summary>
    KeypadEnter = 271, // 0x0000010F
    /// <summary>
    ///   <para>Numeric keypad '='.</para>
    /// </summary>
    KeypadEquals = 272, // 0x00000110
    /// <summary>
    ///   <para>Up arrow key.</para>
    /// </summary>
    UpArrow = 273, // 0x00000111
    /// <summary>
    ///   <para>Down arrow key.</para>
    /// </summary>
    DownArrow = 274, // 0x00000112
    /// <summary>
    ///   <para>Right arrow key.</para>
    /// </summary>
    RightArrow = 275, // 0x00000113
    /// <summary>
    ///   <para>Left arrow key.</para>
    /// </summary>
    LeftArrow = 276, // 0x00000114
    /// <summary>
    ///   <para>Insert key key.</para>
    /// </summary>
    Insert = 277, // 0x00000115
    /// <summary>
    ///   <para>Home key.</para>
    /// </summary>
    Home = 278, // 0x00000116
    /// <summary>
    ///   <para>End key.</para>
    /// </summary>
    End = 279, // 0x00000117
    /// <summary>
    ///   <para>Page up.</para>
    /// </summary>
    PageUp = 280, // 0x00000118
    /// <summary>
    ///   <para>Page down.</para>
    /// </summary>
    PageDown = 281, // 0x00000119
    /// <summary>
    ///   <para>F1 function key.</para>
    /// </summary>
    F1 = 282, // 0x0000011A
    /// <summary>
    ///   <para>F2 function key.</para>
    /// </summary>
    F2 = 283, // 0x0000011B
    /// <summary>
    ///   <para>F3 function key.</para>
    /// </summary>
    F3 = 284, // 0x0000011C
    /// <summary>
    ///   <para>F4 function key.</para>
    /// </summary>
    F4 = 285, // 0x0000011D
    /// <summary>
    ///   <para>F5 function key.</para>
    /// </summary>
    F5 = 286, // 0x0000011E
    /// <summary>
    ///   <para>F6 function key.</para>
    /// </summary>
    F6 = 287, // 0x0000011F
    /// <summary>
    ///   <para>F7 function key.</para>
    /// </summary>
    F7 = 288, // 0x00000120
    /// <summary>
    ///   <para>F8 function key.</para>
    /// </summary>
    F8 = 289, // 0x00000121
    /// <summary>
    ///   <para>F9 function key.</para>
    /// </summary>
    F9 = 290, // 0x00000122
    /// <summary>
    ///   <para>F10 function key.</para>
    /// </summary>
    F10 = 291, // 0x00000123
    /// <summary>
    ///   <para>F11 function key.</para>
    /// </summary>
    F11 = 292, // 0x00000124
    /// <summary>
    ///   <para>F12 function key.</para>
    /// </summary>
    F12 = 293, // 0x00000125
    /// <summary>
    ///   <para>F13 function key.</para>
    /// </summary>
    F13 = 294, // 0x00000126
    /// <summary>
    ///   <para>F14 function key.</para>
    /// </summary>
    F14 = 295, // 0x00000127
    /// <summary>
    ///   <para>F15 function key.</para>
    /// </summary>
    F15 = 296, // 0x00000128

    /// <summary>
	/// any Shift key
	/// </summary>
    AnyShift = 297, // 0x00000129
	/// <summary>
	/// any Alt key
	/// </summary>
    AnyAlt = 298, // 0x0000012A
	/// <summary>
	/// any Option key
	/// </summary>
    AnyOption = 298, // 0x0000012A
	/// <summary>
	/// any Control key
	/// </summary>
    AnyControl = 299, // 0x0000012B

    /// <summary>
    ///   <para>Numlock key.</para>
    /// </summary>
    Numlock = 300, // 0x0000012C
    /// <summary>
    ///   <para>Capslock key.</para>
    /// </summary>
    CapsLock = 301, // 0x0000012D
    /// <summary>
    ///   <para>Scroll lock key.</para>
    /// </summary>
    ScrollLock = 302, // 0x0000012E
    /// <summary>
    ///   <para>Right shift key.</para>
    /// </summary>
    RightShift = 303, // 0x0000012F
    /// <summary>
    ///   <para>Left shift key.</para>
    /// </summary>
    LeftShift = 304, // 0x00000130
    /// <summary>
    ///   <para>Right Control key.</para>
    /// </summary>
    RightControl = 305, // 0x00000131
    /// <summary>
    ///   <para>Left Control key.</para>
    /// </summary>
    LeftControl = 306, // 0x00000132
    /// <summary>
    ///   <para>Right Alt key.</para>
    /// </summary>
    RightAlt = 307, // 0x00000133
    RightOption = 307, // 0x00000133
    /// <summary>
    ///   <para>Left Alt key.</para>
    /// </summary>
    LeftAlt = 308, // 0x00000134
    LeftOption = 308, // 0x00000134
    /// <summary>
    ///   <para>Right Command key.</para>
    /// </summary>
    RightApple = 309, // 0x00000135
    /// <summary>
    ///   <para>Right Command key.</para>
    /// </summary>
    RightCommand = 309, // 0x00000135
    /// <summary>
    ///   <para>Left Command key.</para>
    /// </summary>
    LeftApple = 310, // 0x00000136
    /// <summary>
    ///   <para>Left Command key.</para>
    /// </summary>
    LeftCommand = 310, // 0x00000136
    /// <summary>
    ///   <para>Left Windows key.</para>
    /// </summary>
    LeftWindows = 311, // 0x00000137
    /// <summary>
    ///   <para>Right Windows key.</para>
    /// </summary>
    RightWindows = 312, // 0x00000138
    /// <summary>
    ///   <para>Alt Gr key.</para>
    /// </summary>
    AltGr = 313, // 0x00000139

    // UNUSED = 314, // 0x0000013A

    /// <summary>
    ///   <para>Help key.</para>
    /// </summary>
    Help = 315, // 0x0000013B
    /// <summary>
    ///   <para>Print key.</para>
    /// </summary>
    Print = 316, // 0x0000013C
    /// <summary>
    ///   <para>Sys Req key.</para>
    /// </summary>
    SysReq = 317, // 0x0000013D
    /// <summary>
    ///   <para>Break key.</para>
    /// </summary>
    Break = 318, // 0x0000013E
    /// <summary>
    ///   <para>Menu key.</para>
    /// </summary>
    Menu = 319, // 0x0000013F
    
    // UNUSED320 = 320, // 0x00000140
    
    /// <summary>
    ///   <para>Pushing the scroll wheel forward</para>
    /// NEW
    /// </summary>
    MouseWheelUp = 321, // 0x00000141
    /// <summary>
    ///   <para>Pulling the scroll wheel backward</para>
    /// NEW
    /// </summary>
    MouseWheelDown = 322, // 0x00000142
    
    /// <summary>
    ///   <para>The Left (or primary) mouse button.</para>
    /// </summary>
    Mouse0 = 323, // 0x00000143
    /// <summary>
    ///   <para>Right mouse button (or secondary mouse button).</para>
    /// </summary>
    Mouse1 = 324, // 0x00000144
    /// <summary>
    ///   <para>Middle mouse button (or third button).</para>
    /// </summary>
    Mouse2 = 325, // 0x00000145
    /// <summary>
    ///   <para>Additional (fourth) mouse button.</para>
    /// </summary>
    Mouse3 = 326, // 0x00000146
    /// <summary>
    ///   <para>Additional (fifth) mouse button.</para>
    /// </summary>
    Mouse4 = 327, // 0x00000147
    /// <summary>
    ///   <para>Additional (or sixth) mouse button.</para>
    /// </summary>
    Mouse5 = 328, // 0x00000148
    /// <summary>
    ///   <para>Additional (or seventh) mouse button.</para>
    /// </summary>
    Mouse6 = 329, // 0x00000149
    /// <summary>
    ///   <para>Button 0 on any joystick.</para>
    /// </summary>
    JoystickButton0 = 330, // 0x0000014A
    /// <summary>
    ///   <para>Button 1 on any joystick.</para>
    /// </summary>
    JoystickButton1 = 331, // 0x0000014B
    /// <summary>
    ///   <para>Button 2 on any joystick.</para>
    /// </summary>
    JoystickButton2 = 332, // 0x0000014C
    /// <summary>
    ///   <para>Button 3 on any joystick.</para>
    /// </summary>
    JoystickButton3 = 333, // 0x0000014D
    /// <summary>
    ///   <para>Button 4 on any joystick.</para>
    /// </summary>
    JoystickButton4 = 334, // 0x0000014E
    /// <summary>
    ///   <para>Button 5 on any joystick.</para>
    /// </summary>
    JoystickButton5 = 335, // 0x0000014F
    /// <summary>
    ///   <para>Button 6 on any joystick.</para>
    /// </summary>
    JoystickButton6 = 336, // 0x00000150
    /// <summary>
    ///   <para>Button 7 on any joystick.</para>
    /// </summary>
    JoystickButton7 = 337, // 0x00000151
    /// <summary>
    ///   <para>Button 8 on any joystick.</para>
    /// </summary>
    JoystickButton8 = 338, // 0x00000152
    /// <summary>
    ///   <para>Button 9 on any joystick.</para>
    /// </summary>
    JoystickButton9 = 339, // 0x00000153
    /// <summary>
    ///   <para>Button 10 on any joystick.</para>
    /// </summary>
    JoystickButton10 = 340, // 0x00000154
    /// <summary>
    ///   <para>Button 11 on any joystick.</para>
    /// </summary>
    JoystickButton11 = 341, // 0x00000155
    /// <summary>
    ///   <para>Button 12 on any joystick.</para>
    /// </summary>
    JoystickButton12 = 342, // 0x00000156
    /// <summary>
    ///   <para>Button 13 on any joystick.</para>
    /// </summary>
    JoystickButton13 = 343, // 0x00000157
    /// <summary>
    ///   <para>Button 14 on any joystick.</para>
    /// </summary>
    JoystickButton14 = 344, // 0x00000158
    /// <summary>
    ///   <para>Button 15 on any joystick.</para>
    /// </summary>
    JoystickButton15 = 345, // 0x00000159
    /// <summary>
    ///   <para>Button 16 on any joystick.</para>
    /// </summary>
    JoystickButton16 = 346, // 0x0000015A
    /// <summary>
    ///   <para>Button 17 on any joystick.</para>
    /// </summary>
    JoystickButton17 = 347, // 0x0000015B
    /// <summary>
    ///   <para>Button 18 on any joystick.</para>
    /// </summary>
    JoystickButton18 = 348, // 0x0000015C
    /// <summary>
    ///   <para>Button 19 on any joystick.</para>
    /// </summary>
    JoystickButton19 = 349, // 0x0000015D
    /// <summary>
    ///   <para>Button 0 on first joystick.</para>
    /// </summary>
    Joystick1Button0 = 350, // 0x0000015E
    /// <summary>
    ///   <para>Button 1 on first joystick.</para>
    /// </summary>
    Joystick1Button1 = 351, // 0x0000015F
    /// <summary>
    ///   <para>Button 2 on first joystick.</para>
    /// </summary>
    Joystick1Button2 = 352, // 0x00000160
    /// <summary>
    ///   <para>Button 3 on first joystick.</para>
    /// </summary>
    Joystick1Button3 = 353, // 0x00000161
    /// <summary>
    ///   <para>Button 4 on first joystick.</para>
    /// </summary>
    Joystick1Button4 = 354, // 0x00000162
    /// <summary>
    ///   <para>Button 5 on first joystick.</para>
    /// </summary>
    Joystick1Button5 = 355, // 0x00000163
    /// <summary>
    ///   <para>Button 6 on first joystick.</para>
    /// </summary>
    Joystick1Button6 = 356, // 0x00000164
    /// <summary>
    ///   <para>Button 7 on first joystick.</para>
    /// </summary>
    Joystick1Button7 = 357, // 0x00000165
    /// <summary>
    ///   <para>Button 8 on first joystick.</para>
    /// </summary>
    Joystick1Button8 = 358, // 0x00000166
    /// <summary>
    ///   <para>Button 9 on first joystick.</para>
    /// </summary>
    Joystick1Button9 = 359, // 0x00000167
    /// <summary>
    ///   <para>Button 10 on first joystick.</para>
    /// </summary>
    Joystick1Button10 = 360, // 0x00000168
    /// <summary>
    ///   <para>Button 11 on first joystick.</para>
    /// </summary>
    Joystick1Button11 = 361, // 0x00000169
    /// <summary>
    ///   <para>Button 12 on first joystick.</para>
    /// </summary>
    Joystick1Button12 = 362, // 0x0000016A
    /// <summary>
    ///   <para>Button 13 on first joystick.</para>
    /// </summary>
    Joystick1Button13 = 363, // 0x0000016B
    /// <summary>
    ///   <para>Button 14 on first joystick.</para>
    /// </summary>
    Joystick1Button14 = 364, // 0x0000016C
    /// <summary>
    ///   <para>Button 15 on first joystick.</para>
    /// </summary>
    Joystick1Button15 = 365, // 0x0000016D
    /// <summary>
    ///   <para>Button 16 on first joystick.</para>
    /// </summary>
    Joystick1Button16 = 366, // 0x0000016E
    /// <summary>
    ///   <para>Button 17 on first joystick.</para>
    /// </summary>
    Joystick1Button17 = 367, // 0x0000016F
    /// <summary>
    ///   <para>Button 18 on first joystick.</para>
    /// </summary>
    Joystick1Button18 = 368, // 0x00000170
    /// <summary>
    ///   <para>Button 19 on first joystick.</para>
    /// </summary>
    Joystick1Button19 = 369, // 0x00000171
    /// <summary>
    ///   <para>Button 0 on second joystick.</para>
    /// </summary>
    Joystick2Button0 = 370, // 0x00000172
    /// <summary>
    ///   <para>Button 1 on second joystick.</para>
    /// </summary>
    Joystick2Button1 = 371, // 0x00000173
    /// <summary>
    ///   <para>Button 2 on second joystick.</para>
    /// </summary>
    Joystick2Button2 = 372, // 0x00000174
    /// <summary>
    ///   <para>Button 3 on second joystick.</para>
    /// </summary>
    Joystick2Button3 = 373, // 0x00000175
    /// <summary>
    ///   <para>Button 4 on second joystick.</para>
    /// </summary>
    Joystick2Button4 = 374, // 0x00000176
    /// <summary>
    ///   <para>Button 5 on second joystick.</para>
    /// </summary>
    Joystick2Button5 = 375, // 0x00000177
    /// <summary>
    ///   <para>Button 6 on second joystick.</para>
    /// </summary>
    Joystick2Button6 = 376, // 0x00000178
    /// <summary>
    ///   <para>Button 7 on second joystick.</para>
    /// </summary>
    Joystick2Button7 = 377, // 0x00000179
    /// <summary>
    ///   <para>Button 8 on second joystick.</para>
    /// </summary>
    Joystick2Button8 = 378, // 0x0000017A
    /// <summary>
    ///   <para>Button 9 on second joystick.</para>
    /// </summary>
    Joystick2Button9 = 379, // 0x0000017B
    /// <summary>
    ///   <para>Button 10 on second joystick.</para>
    /// </summary>
    Joystick2Button10 = 380, // 0x0000017C
    /// <summary>
    ///   <para>Button 11 on second joystick.</para>
    /// </summary>
    Joystick2Button11 = 381, // 0x0000017D
    /// <summary>
    ///   <para>Button 12 on second joystick.</para>
    /// </summary>
    Joystick2Button12 = 382, // 0x0000017E
    /// <summary>
    ///   <para>Button 13 on second joystick.</para>
    /// </summary>
    Joystick2Button13 = 383, // 0x0000017F
    /// <summary>
    ///   <para>Button 14 on second joystick.</para>
    /// </summary>
    Joystick2Button14 = 384, // 0x00000180
    /// <summary>
    ///   <para>Button 15 on second joystick.</para>
    /// </summary>
    Joystick2Button15 = 385, // 0x00000181
    /// <summary>
    ///   <para>Button 16 on second joystick.</para>
    /// </summary>
    Joystick2Button16 = 386, // 0x00000182
    /// <summary>
    ///   <para>Button 17 on second joystick.</para>
    /// </summary>
    Joystick2Button17 = 387, // 0x00000183
    /// <summary>
    ///   <para>Button 18 on second joystick.</para>
    /// </summary>
    Joystick2Button18 = 388, // 0x00000184
    /// <summary>
    ///   <para>Button 19 on second joystick.</para>
    /// </summary>
    Joystick2Button19 = 389, // 0x00000185
    /// <summary>
    ///   <para>Button 0 on third joystick.</para>
    /// </summary>
    Joystick3Button0 = 390, // 0x00000186
    /// <summary>
    ///   <para>Button 1 on third joystick.</para>
    /// </summary>
    Joystick3Button1 = 391, // 0x00000187
    /// <summary>
    ///   <para>Button 2 on third joystick.</para>
    /// </summary>
    Joystick3Button2 = 392, // 0x00000188
    /// <summary>
    ///   <para>Button 3 on third joystick.</para>
    /// </summary>
    Joystick3Button3 = 393, // 0x00000189
    /// <summary>
    ///   <para>Button 4 on third joystick.</para>
    /// </summary>
    Joystick3Button4 = 394, // 0x0000018A
    /// <summary>
    ///   <para>Button 5 on third joystick.</para>
    /// </summary>
    Joystick3Button5 = 395, // 0x0000018B
    /// <summary>
    ///   <para>Button 6 on third joystick.</para>
    /// </summary>
    Joystick3Button6 = 396, // 0x0000018C
    /// <summary>
    ///   <para>Button 7 on third joystick.</para>
    /// </summary>
    Joystick3Button7 = 397, // 0x0000018D
    /// <summary>
    ///   <para>Button 8 on third joystick.</para>
    /// </summary>
    Joystick3Button8 = 398, // 0x0000018E
    /// <summary>
    ///   <para>Button 9 on third joystick.</para>
    /// </summary>
    Joystick3Button9 = 399, // 0x0000018F
    /// <summary>
    ///   <para>Button 10 on third joystick.</para>
    /// </summary>
    Joystick3Button10 = 400, // 0x00000190
    /// <summary>
    ///   <para>Button 11 on third joystick.</para>
    /// </summary>
    Joystick3Button11 = 401, // 0x00000191
    /// <summary>
    ///   <para>Button 12 on third joystick.</para>
    /// </summary>
    Joystick3Button12 = 402, // 0x00000192
    /// <summary>
    ///   <para>Button 13 on third joystick.</para>
    /// </summary>
    Joystick3Button13 = 403, // 0x00000193
    /// <summary>
    ///   <para>Button 14 on third joystick.</para>
    /// </summary>
    Joystick3Button14 = 404, // 0x00000194
    /// <summary>
    ///   <para>Button 15 on third joystick.</para>
    /// </summary>
    Joystick3Button15 = 405, // 0x00000195
    /// <summary>
    ///   <para>Button 16 on third joystick.</para>
    /// </summary>
    Joystick3Button16 = 406, // 0x00000196
    /// <summary>
    ///   <para>Button 17 on third joystick.</para>
    /// </summary>
    Joystick3Button17 = 407, // 0x00000197
    /// <summary>
    ///   <para>Button 18 on third joystick.</para>
    /// </summary>
    Joystick3Button18 = 408, // 0x00000198
    /// <summary>
    ///   <para>Button 19 on third joystick.</para>
    /// </summary>
    Joystick3Button19 = 409, // 0x00000199
    /// <summary>
    ///   <para>Button 0 on forth joystick.</para>
    /// </summary>
    Joystick4Button0 = 410, // 0x0000019A
    /// <summary>
    ///   <para>Button 1 on forth joystick.</para>
    /// </summary>
    Joystick4Button1 = 411, // 0x0000019B
    /// <summary>
    ///   <para>Button 2 on forth joystick.</para>
    /// </summary>
    Joystick4Button2 = 412, // 0x0000019C
    /// <summary>
    ///   <para>Button 3 on forth joystick.</para>
    /// </summary>
    Joystick4Button3 = 413, // 0x0000019D
    /// <summary>
    ///   <para>Button 4 on forth joystick.</para>
    /// </summary>
    Joystick4Button4 = 414, // 0x0000019E
    /// <summary>
    ///   <para>Button 5 on forth joystick.</para>
    /// </summary>
    Joystick4Button5 = 415, // 0x0000019F
    /// <summary>
    ///   <para>Button 6 on forth joystick.</para>
    /// </summary>
    Joystick4Button6 = 416, // 0x000001A0
    /// <summary>
    ///   <para>Button 7 on forth joystick.</para>
    /// </summary>
    Joystick4Button7 = 417, // 0x000001A1
    /// <summary>
    ///   <para>Button 8 on forth joystick.</para>
    /// </summary>
    Joystick4Button8 = 418, // 0x000001A2
    /// <summary>
    ///   <para>Button 9 on forth joystick.</para>
    /// </summary>
    Joystick4Button9 = 419, // 0x000001A3
    /// <summary>
    ///   <para>Button 10 on forth joystick.</para>
    /// </summary>
    Joystick4Button10 = 420, // 0x000001A4
    /// <summary>
    ///   <para>Button 11 on forth joystick.</para>
    /// </summary>
    Joystick4Button11 = 421, // 0x000001A5
    /// <summary>
    ///   <para>Button 12 on forth joystick.</para>
    /// </summary>
    Joystick4Button12 = 422, // 0x000001A6
    /// <summary>
    ///   <para>Button 13 on forth joystick.</para>
    /// </summary>
    Joystick4Button13 = 423, // 0x000001A7
    /// <summary>
    ///   <para>Button 14 on forth joystick.</para>
    /// </summary>
    Joystick4Button14 = 424, // 0x000001A8
    /// <summary>
    ///   <para>Button 15 on forth joystick.</para>
    /// </summary>
    Joystick4Button15 = 425, // 0x000001A9
    /// <summary>
    ///   <para>Button 16 on forth joystick.</para>
    /// </summary>
    Joystick4Button16 = 426, // 0x000001AA
    /// <summary>
    ///   <para>Button 17 on forth joystick.</para>
    /// </summary>
    Joystick4Button17 = 427, // 0x000001AB
    /// <summary>
    ///   <para>Button 18 on forth joystick.</para>
    /// </summary>
    Joystick4Button18 = 428, // 0x000001AC
    /// <summary>
    ///   <para>Button 19 on forth joystick.</para>
    /// </summary>
    Joystick4Button19 = 429, // 0x000001AD
    /// <summary>
    ///   <para>Button 0 on fifth joystick.</para>
    /// </summary>
    Joystick5Button0 = 430, // 0x000001AE
    /// <summary>
    ///   <para>Button 1 on fifth joystick.</para>
    /// </summary>
    Joystick5Button1 = 431, // 0x000001AF
    /// <summary>
    ///   <para>Button 2 on fifth joystick.</para>
    /// </summary>
    Joystick5Button2 = 432, // 0x000001B0
    /// <summary>
    ///   <para>Button 3 on fifth joystick.</para>
    /// </summary>
    Joystick5Button3 = 433, // 0x000001B1
    /// <summary>
    ///   <para>Button 4 on fifth joystick.</para>
    /// </summary>
    Joystick5Button4 = 434, // 0x000001B2
    /// <summary>
    ///   <para>Button 5 on fifth joystick.</para>
    /// </summary>
    Joystick5Button5 = 435, // 0x000001B3
    /// <summary>
    ///   <para>Button 6 on fifth joystick.</para>
    /// </summary>
    Joystick5Button6 = 436, // 0x000001B4
    /// <summary>
    ///   <para>Button 7 on fifth joystick.</para>
    /// </summary>
    Joystick5Button7 = 437, // 0x000001B5
    /// <summary>
    ///   <para>Button 8 on fifth joystick.</para>
    /// </summary>
    Joystick5Button8 = 438, // 0x000001B6
    /// <summary>
    ///   <para>Button 9 on fifth joystick.</para>
    /// </summary>
    Joystick5Button9 = 439, // 0x000001B7
    /// <summary>
    ///   <para>Button 10 on fifth joystick.</para>
    /// </summary>
    Joystick5Button10 = 440, // 0x000001B8
    /// <summary>
    ///   <para>Button 11 on fifth joystick.</para>
    /// </summary>
    Joystick5Button11 = 441, // 0x000001B9
    /// <summary>
    ///   <para>Button 12 on fifth joystick.</para>
    /// </summary>
    Joystick5Button12 = 442, // 0x000001BA
    /// <summary>
    ///   <para>Button 13 on fifth joystick.</para>
    /// </summary>
    Joystick5Button13 = 443, // 0x000001BB
    /// <summary>
    ///   <para>Button 14 on fifth joystick.</para>
    /// </summary>
    Joystick5Button14 = 444, // 0x000001BC
    /// <summary>
    ///   <para>Button 15 on fifth joystick.</para>
    /// </summary>
    Joystick5Button15 = 445, // 0x000001BD
    /// <summary>
    ///   <para>Button 16 on fifth joystick.</para>
    /// </summary>
    Joystick5Button16 = 446, // 0x000001BE
    /// <summary>
    ///   <para>Button 17 on fifth joystick.</para>
    /// </summary>
    Joystick5Button17 = 447, // 0x000001BF
    /// <summary>
    ///   <para>Button 18 on fifth joystick.</para>
    /// </summary>
    Joystick5Button18 = 448, // 0x000001C0
    /// <summary>
    ///   <para>Button 19 on fifth joystick.</para>
    /// </summary>
    Joystick5Button19 = 449, // 0x000001C1
    /// <summary>
    ///   <para>Button 0 on sixth joystick.</para>
    /// </summary>
    Joystick6Button0 = 450, // 0x000001C2
    /// <summary>
    ///   <para>Button 1 on sixth joystick.</para>
    /// </summary>
    Joystick6Button1 = 451, // 0x000001C3
    /// <summary>
    ///   <para>Button 2 on sixth joystick.</para>
    /// </summary>
    Joystick6Button2 = 452, // 0x000001C4
    /// <summary>
    ///   <para>Button 3 on sixth joystick.</para>
    /// </summary>
    Joystick6Button3 = 453, // 0x000001C5
    /// <summary>
    ///   <para>Button 4 on sixth joystick.</para>
    /// </summary>
    Joystick6Button4 = 454, // 0x000001C6
    /// <summary>
    ///   <para>Button 5 on sixth joystick.</para>
    /// </summary>
    Joystick6Button5 = 455, // 0x000001C7
    /// <summary>
    ///   <para>Button 6 on sixth joystick.</para>
    /// </summary>
    Joystick6Button6 = 456, // 0x000001C8
    /// <summary>
    ///   <para>Button 7 on sixth joystick.</para>
    /// </summary>
    Joystick6Button7 = 457, // 0x000001C9
    /// <summary>
    ///   <para>Button 8 on sixth joystick.</para>
    /// </summary>
    Joystick6Button8 = 458, // 0x000001CA
    /// <summary>
    ///   <para>Button 9 on sixth joystick.</para>
    /// </summary>
    Joystick6Button9 = 459, // 0x000001CB
    /// <summary>
    ///   <para>Button 10 on sixth joystick.</para>
    /// </summary>
    Joystick6Button10 = 460, // 0x000001CC
    /// <summary>
    ///   <para>Button 11 on sixth joystick.</para>
    /// </summary>
    Joystick6Button11 = 461, // 0x000001CD
    /// <summary>
    ///   <para>Button 12 on sixth joystick.</para>
    /// </summary>
    Joystick6Button12 = 462, // 0x000001CE
    /// <summary>
    ///   <para>Button 13 on sixth joystick.</para>
    /// </summary>
    Joystick6Button13 = 463, // 0x000001CF
    /// <summary>
    ///   <para>Button 14 on sixth joystick.</para>
    /// </summary>
    Joystick6Button14 = 464, // 0x000001D0
    /// <summary>
    ///   <para>Button 15 on sixth joystick.</para>
    /// </summary>
    Joystick6Button15 = 465, // 0x000001D1
    /// <summary>
    ///   <para>Button 16 on sixth joystick.</para>
    /// </summary>
    Joystick6Button16 = 466, // 0x000001D2
    /// <summary>
    ///   <para>Button 17 on sixth joystick.</para>
    /// </summary>
    Joystick6Button17 = 467, // 0x000001D3
    /// <summary>
    ///   <para>Button 18 on sixth joystick.</para>
    /// </summary>
    Joystick6Button18 = 468, // 0x000001D4
    /// <summary>
    ///   <para>Button 19 on sixth joystick.</para>
    /// </summary>
    Joystick6Button19 = 469, // 0x000001D5
    /// <summary>
    ///   <para>Button 0 on seventh joystick.</para>
    /// </summary>
    Joystick7Button0 = 470, // 0x000001D6
    /// <summary>
    ///   <para>Button 1 on seventh joystick.</para>
    /// </summary>
    Joystick7Button1 = 471, // 0x000001D7
    /// <summary>
    ///   <para>Button 2 on seventh joystick.</para>
    /// </summary>
    Joystick7Button2 = 472, // 0x000001D8
    /// <summary>
    ///   <para>Button 3 on seventh joystick.</para>
    /// </summary>
    Joystick7Button3 = 473, // 0x000001D9
    /// <summary>
    ///   <para>Button 4 on seventh joystick.</para>
    /// </summary>
    Joystick7Button4 = 474, // 0x000001DA
    /// <summary>
    ///   <para>Button 5 on seventh joystick.</para>
    /// </summary>
    Joystick7Button5 = 475, // 0x000001DB
    /// <summary>
    ///   <para>Button 6 on seventh joystick.</para>
    /// </summary>
    Joystick7Button6 = 476, // 0x000001DC
    /// <summary>
    ///   <para>Button 7 on seventh joystick.</para>
    /// </summary>
    Joystick7Button7 = 477, // 0x000001DD
    /// <summary>
    ///   <para>Button 8 on seventh joystick.</para>
    /// </summary>
    Joystick7Button8 = 478, // 0x000001DE
    /// <summary>
    ///   <para>Button 9 on seventh joystick.</para>
    /// </summary>
    Joystick7Button9 = 479, // 0x000001DF
    /// <summary>
    ///   <para>Button 10 on seventh joystick.</para>
    /// </summary>
    Joystick7Button10 = 480, // 0x000001E0
    /// <summary>
    ///   <para>Button 11 on seventh joystick.</para>
    /// </summary>
    Joystick7Button11 = 481, // 0x000001E1
    /// <summary>
    ///   <para>Button 12 on seventh joystick.</para>
    /// </summary>
    Joystick7Button12 = 482, // 0x000001E2
    /// <summary>
    ///   <para>Button 13 on seventh joystick.</para>
    /// </summary>
    Joystick7Button13 = 483, // 0x000001E3
    /// <summary>
    ///   <para>Button 14 on seventh joystick.</para>
    /// </summary>
    Joystick7Button14 = 484, // 0x000001E4
    /// <summary>
    ///   <para>Button 15 on seventh joystick.</para>
    /// </summary>
    Joystick7Button15 = 485, // 0x000001E5
    /// <summary>
    ///   <para>Button 16 on seventh joystick.</para>
    /// </summary>
    Joystick7Button16 = 486, // 0x000001E6
    /// <summary>
    ///   <para>Button 17 on seventh joystick.</para>
    /// </summary>
    Joystick7Button17 = 487, // 0x000001E7
    /// <summary>
    ///   <para>Button 18 on seventh joystick.</para>
    /// </summary>
    Joystick7Button18 = 488, // 0x000001E8
    /// <summary>
    ///   <para>Button 19 on seventh joystick.</para>
    /// </summary>
    Joystick7Button19 = 489, // 0x000001E9
    /// <summary>
    ///   <para>Button 0 on eighth joystick.</para>
    /// </summary>
    Joystick8Button0 = 490, // 0x000001EA
    /// <summary>
    ///   <para>Button 1 on eighth joystick.</para>
    /// </summary>
    Joystick8Button1 = 491, // 0x000001EB
    /// <summary>
    ///   <para>Button 2 on eighth joystick.</para>
    /// </summary>
    Joystick8Button2 = 492, // 0x000001EC
    /// <summary>
    ///   <para>Button 3 on eighth joystick.</para>
    /// </summary>
    Joystick8Button3 = 493, // 0x000001ED
    /// <summary>
    ///   <para>Button 4 on eighth joystick.</para>
    /// </summary>
    Joystick8Button4 = 494, // 0x000001EE
    /// <summary>
    ///   <para>Button 5 on eighth joystick.</para>
    /// </summary>
    Joystick8Button5 = 495, // 0x000001EF
    /// <summary>
    ///   <para>Button 6 on eighth joystick.</para>
    /// </summary>
    Joystick8Button6 = 496, // 0x000001F0
    /// <summary>
    ///   <para>Button 7 on eighth joystick.</para>
    /// </summary>
    Joystick8Button7 = 497, // 0x000001F1
    /// <summary>
    ///   <para>Button 8 on eighth joystick.</para>
    /// </summary>
    Joystick8Button8 = 498, // 0x000001F2
    /// <summary>
    ///   <para>Button 9 on eighth joystick.</para>
    /// </summary>
    Joystick8Button9 = 499, // 0x000001F3
    /// <summary>
    ///   <para>Button 10 on eighth joystick.</para>
    /// </summary>
    Joystick8Button10 = 500, // 0x000001F4
    /// <summary>
    ///   <para>Button 11 on eighth joystick.</para>
    /// </summary>
    Joystick8Button11 = 501, // 0x000001F5
    /// <summary>
    ///   <para>Button 12 on eighth joystick.</para>
    /// </summary>
    Joystick8Button12 = 502, // 0x000001F6
    /// <summary>
    ///   <para>Button 13 on eighth joystick.</para>
    /// </summary>
    Joystick8Button13 = 503, // 0x000001F7
    /// <summary>
    ///   <para>Button 14 on eighth joystick.</para>
    /// </summary>
    Joystick8Button14 = 504, // 0x000001F8
    /// <summary>
    ///   <para>Button 15 on eighth joystick.</para>
    /// </summary>
    Joystick8Button15 = 505, // 0x000001F9
    /// <summary>
    ///   <para>Button 16 on eighth joystick.</para>
    /// </summary>
    Joystick8Button16 = 506, // 0x000001FA
    /// <summary>
    ///   <para>Button 17 on eighth joystick.</para>
    /// </summary>
    Joystick8Button17 = 507, // 0x000001FB
    /// <summary>
    ///   <para>Button 18 on eighth joystick.</para>
    /// </summary>
    Joystick8Button18 = 508, // 0x000001FC
    /// <summary>
    ///   <para>Button 19 on eighth joystick.</para>
    /// </summary>
    Joystick8Button19 = 509, // 0x000001FD
    LAST = 510,
  }
}
