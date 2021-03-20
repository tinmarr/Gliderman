using UnityEngine;
using System.Collections.Generic;

// author: mvaganov@hotmail.com
// license: Copyfree, public domain. This is free code! Great artists, steal this code!
// latest version at: https://pastebin.com/raw/8m69iTut -- last updated (2020/12/12)
namespace NonStandard
{
	/// <summary>static functions for Unity's LineRenderer. Creates visualizations for 3D Vector math.
	/// This library isn't optimized for performance, it's built to make math less invisible, even at compiled runtime.
	/// </summary>
	public class Lines : MonoBehaviour
	{
		[Tooltip("Used to draw lines. Ideally a white Sprites/Default shader.")]
		public Material lineMaterial;
		public bool autoParentLinesToGlobalObject = true;

		/// <summary>the dictionary of named lines. This structure allows Lines to create new lines without needing explicit variables</summary>
		static Dictionary<string, GameObject> namedObject = new Dictionary<string, GameObject>();
		/// <summary>The singleton instance.</summary>
		static Lines instance;

		public static Lines Instance()
		{
			if (instance == null)
			{
				instance = FindComponentInstance<Lines>();
			}
			return instance;
		}
		public static T FindComponentInstance<T>() where T : Component
		{
			T instance = null;
			if ((instance = FindObjectOfType(typeof(T)) as T) == null)
			{
				GameObject g = new GameObject("<" + typeof(T).Name + ">");
				instance = g.AddComponent<T>();
			}
			return instance;
		}

		void Start()
		{
			if (instance != null && instance != this)
			{
				Debug.LogWarning("<Lines> should be a singleton. Deleting extra");
				Destroy(this);
			}
		}

		/// <param name="name"></param>
		/// <param name="createIfNotFound">if true, this function will not return null</param>
		/// <returns>a line object with the given name. can return null if no such object has been made yet with this function</returns>
		public static GameObject Get(string name, bool createIfNotFound = false)
		{
			GameObject go;
			if ((!namedObject.TryGetValue(name, out go) || go == null) && createIfNotFound)
			{
				go = namedObject[name] = MakeLineRenderer(ref go).gameObject;
				go.name = name;
			}
			return go;
		}

		public static Line_ Make(string name, bool createIfNotFound = true)
		{
			Line_ line_ = null;
			GameObject go = Get(name, createIfNotFound);
			if (go != null)
			{
				line_ = go.GetComponent<Line_>();
				if (line_ == null) { line_ = go.AddComponent<Line_>(); }
			}
			return line_;
		}
		public enum End { normal, arrow, arrowBothEnds };
		/// <summary>cached calculations. used to validate if a line needs to be re-calculated</summary>
		public class Line_ : MonoBehaviour
		{
			public enum Kind { none, line, arc, orbital, spiralSphere, box, quaternion, disabled }
			Kind _kind;
			Vector3[] points;
			Vector3 normal;
			Quaternion rotation;
			int count;
			float startSize, endSize, angle;
			End lineEnds;
			LineRenderer lr;
			public int numCapVertices { get { return lr.numCapVertices; } set { lr.numCapVertices = value; } }

			public Kind kind {
				get { return _kind; }
				set {
					if (_kind == Kind.quaternion && value != Kind.quaternion)
					{
						GameObject[] obj = QuaternionAngleChildObjects(points.Length, false);
						if (obj != null)
						{
							System.Array.ForEach(obj, o => { o.transform.SetParent(null); Destroy(o); });
						}
					}
					_kind = value;
				}
			}

			private static bool SameArrayOfVectors(Vector3[] a, Vector3[] b)
			{
				if (ReferenceEquals(a, b)) { return true; }
				if (a == null || b == null || a.Length != b.Length) { return false; }
				for (int i = 0; i < a.Length; ++i) { if (a[i] != b[i]) return false; }
				return true;
			}
			private bool IsLine(Vector3[] points, float startSize, float endSize, End lineEnds)
			{
				return kind == Kind.line && SameArrayOfVectors(this.points, points)
					&& startSize == this.startSize && endSize == this.endSize && this.lineEnds == lineEnds;
			}
			private void SetLine(Vector3[] points, float startSize, float endSize, End lineEnds)
			{
				kind = Kind.line;
				this.points = new Vector3[points.Length]; System.Array.Copy(points, this.points, points.Length);
				this.startSize = startSize; this.endSize = endSize; this.lineEnds = lineEnds;
			}
			private bool IsArc(Vector3 start, Vector3 normal, Vector3 center, float angle, float startSize, float endSize, End lineEnds, int pointCount)
			{
				return kind == Kind.arc && points != null && points.Length == 1 && points[0] == start && count == pointCount
					&& this.normal == normal && startSize == this.startSize && endSize == this.endSize && this.lineEnds == lineEnds
					&& this.transform.position == center && this.normal == normal && this.angle == angle;
			}
			private void SetArc(Vector3 start, Vector3 normal, Vector3 center, float angle, float startSize, float endSize, End lineEnds, int pointCount)
			{
				kind = Kind.arc;
				points = new Vector3[] { start }; count = pointCount;
				this.startSize = startSize; this.endSize = endSize; this.lineEnds = lineEnds;
				this.transform.position = center; this.normal = normal; this.angle = angle;
			}
			private bool IsOrbital(Vector3 start, Vector3 end, Vector3 center, float startSize, float endSize, End lineEnds, int pointCount)
			{
				return kind == Kind.orbital && points != null && points.Length == 2 && count == pointCount
					&& points[0] == start && points[1] == end
					&& startSize == this.startSize && endSize == this.endSize && this.lineEnds == lineEnds
					&& this.transform.position == center;
			}
			private void SetOrbital(Vector3 start, Vector3 end, Vector3 center = default(Vector3), float startSize = LINESIZE, float endSize = LINESIZE,
				End lineEnds = default(End), int pointCount = -1)
			{
				kind = Kind.orbital;
				points = new Vector3[] { start, end }; count = pointCount;
				this.startSize = startSize; this.endSize = endSize; this.lineEnds = lineEnds;
				this.transform.position = center;
			}
			private bool IsSpiralSphere(Vector3 center, float radius, float linesize, Quaternion rotation)
			{
				return kind == Kind.spiralSphere
					&& this.startSize == linesize && this.endSize == linesize
					&& this.transform.position == center && this.angle == radius
					&& (this.rotation.Equals(rotation) || this.rotation == rotation);
			}
			private void SetSpiralSphere(Vector3 center, float radius, float linesize, Quaternion rotation)
			{
				kind = Kind.spiralSphere;
				startSize = endSize = linesize;
				this.transform.position = center; angle = radius; this.rotation = rotation;
			}
			private bool IsBox(Vector3 center, Vector3 size, Quaternion rotation, float linesize)
			{
				return kind == Kind.box
					&& this.startSize == linesize && this.endSize == linesize
					&& this.transform.position == center
					&& transform.localScale == size && transform.rotation == rotation;
			}
			private void SetBox(Vector3 center, Vector3 size, Quaternion rotation, float linesize)
			{
				kind = Kind.box;
				startSize = endSize = linesize;
				this.transform.position = center;
				transform.localScale = size; transform.rotation = rotation;
			}
			private bool IsQuaternion(float an, Vector3 ax, Vector3 position, Vector3[] startPoints, Quaternion orientation, int arcPoints, float linesize)
			{
				return kind == Kind.quaternion && SameArrayOfVectors(this.points, startPoints)
					&& this.startSize == linesize && this.endSize == linesize
					&& this.transform.position == position && normal == ax && angle == an && count == arcPoints
					&& (rotation.Equals(orientation) || rotation == orientation); // quaternions can't easily be tested for equality because of floating point errors
			}
			private void SetQuaternion(float an, Vector3 ax, Vector3 position, Vector3[] startPoints, Quaternion orientation, int arcPoints, float linesize)
			{
				kind = Kind.quaternion;
				if (ReferenceEquals(startPoints, default_quaternion_visualization_points))
				{
					points = default_quaternion_visualization_points;
				} else
				{
					points = new Vector3[startPoints.Length]; System.Array.Copy(startPoints, points, startPoints.Length);
				}
				startSize = endSize = linesize;
				this.transform.position = position; normal = ax; angle = an; count = arcPoints;
				rotation = orientation;
			}
			private static LineRenderer MakeLine(ref GameObject go, Vector3[] points, Color color, float startSize, float endSize, End lineEnds)
			{
				LineRenderer lr;
				if (lineEnds == End.arrow || lineEnds == End.arrowBothEnds)
				{
					Vector3[] line;
					Keyframe[] keyframes = CalculateArrowKeyframes(points, points.Length, out line, startSize, endSize, ARROWSIZE, null);
					lr = MakeArrow(ref go, line, line.Length, color, startSize, endSize);
					lr.widthCurve = new AnimationCurve(keyframes);
					if (lineEnds == End.arrowBothEnds)
					{
						ReverseLineInternal(ref lr);
						Vector3[] p = new Vector3[lr.positionCount];
						lr.GetPositions(p);
						lr = MakeArrow(ref go, p, p.Length, color, endSize, startSize, ARROWSIZE, lr.widthCurve.keys);
						ReverseLineInternal(ref lr);
					}
				} else
				{
					lr = Make(ref go, points, points.Length, color, startSize, endSize);
				}
				return lr;
			}

			public Line_ Line(Vector3 start, Vector3 end, Color color = default(Color), float startSize = LINESIZE, float endSize = LINESIZE)
			{
				return Line(new Vector3[] { start, end }, color, End.normal, startSize, endSize);
			}
			public Line_ Arrow(Vector3 vector, Color color = default(Color), float startSize = LINESIZE, float endSize = LINESIZE)
			{
				return Line(new Vector3[] { Vector3.zero, vector }, color, End.arrow, startSize, endSize);
			}
			public Line_ Arrow(Vector3 start, Vector3 end, Color color = default(Color), float startSize = LINESIZE, float endSize = LINESIZE)
			{
				return Line(new Vector3[] { start, end }, color, End.arrow, startSize, endSize);
			}
			public Line_ Line(Vector3[] points, Color color = default(Color), End lineEnds = default(End), float startSize = LINESIZE, float endSize = LINESIZE)
			{
				GameObject go = gameObject;
				if (!IsLine(points, startSize, endSize, lineEnds))
				{
					SetLine(points, startSize, endSize, lineEnds);
					lr = MakeLine(ref go, points, color, startSize, endSize, lineEnds);
				} //else { Debug.Log("don't need to recalculate line "+name); }
				SetColor(lr, color);
				return this;
			}
			public Line_ Arc(float angle, Vector3 normal, Vector3 firstPoint, Vector3 center = default(Vector3), Color color = default(Color),
				End lineEnds = default(End), int pointCount = -1, float startSize = LINESIZE, float endSize = LINESIZE)
			{
				GameObject go = gameObject;
				if (pointCount < 0) { pointCount = (int)(24 * Mathf.Abs(angle) / 180f) + 1; }
				if (!IsArc(firstPoint, normal, center, angle, startSize, endSize, End.normal, pointCount))
				{
					SetArc(firstPoint, normal, center, angle, startSize, endSize, End.normal, pointCount);
					Vector3[] linepoints = null;
					WriteArc(ref linepoints, pointCount, normal, firstPoint, angle, center);
					lr = MakeLine(ref go, linepoints, color, startSize, endSize, lineEnds);
				} //else { Debug.Log("don't need to recalculate arc "+name);  }
				SetColor(lr, color);
				return this;
			}
			public Line_ Circle(Vector3 center = default(Vector3), Vector3 normal = default(Vector3), Color color = default(Color), float radius = 1,
				int pointCount = -1, float linesize = LINESIZE)
			{
				if (normal == default(Vector3)) { normal = Vector3.up; }
				Vector3 firstPoint = Vector3.zero;
				if (kind == Kind.arc && this.normal == normal && points != null && points.Length > 0)
				{
					float firstRad = points[0].magnitude;
					if (firstRad == radius)
					{
						firstPoint = points[0];
					} else
					{
						firstPoint = points[0] * (radius / firstRad);
					}
				}
				if (firstPoint == Vector3.zero)
				{
					firstPoint = Vector3.right;
					if (normal != Vector3.up && normal != Vector3.forward && normal != Vector3.back)
					{
						firstPoint = Vector3.Cross(normal, Vector3.forward).normalized;
					}
					firstPoint *= radius;
				}
				return Arc(360, normal, firstPoint, center, color, End.normal, pointCount, linesize);
			}
			public Line_ Orbital(Vector3 sphereCenter, Vector3 start, Vector3 end,
				Color color = default(Color), End lineEnds = default(End), float startSize = LINESIZE, float endSize = LINESIZE, int pointCount = -1)
			{
				GameObject go = gameObject;
				if (!IsOrbital(start, end, sphereCenter, startSize, endSize, lineEnds, pointCount))
				{
					SetOrbital(start, end, sphereCenter, startSize, endSize, lineEnds, pointCount);
					Vector3[] linepoints = null;
					WriteArcOnSphere(ref linepoints, pointCount, sphereCenter, start, end);
					lr = MakeLine(ref go, linepoints, color, startSize, endSize, lineEnds);
				} //else { Debug.Log("don't need to recalculate orbital " + name); }
				SetColor(lr, color);
				return this;
			}
			public Line_ SpiralSphere(Color color = default(Color), Vector3 center = default(Vector3), float radius = 1, Quaternion rotation = default(Quaternion), float linesize = LINESIZE)
			{
				GameObject go = gameObject;
				if (!IsSpiralSphere(center, radius, linesize, rotation))
				{
					SetSpiralSphere(center, radius, linesize, rotation);
					lr = MakeSpiralSphere(ref go, radius, center, rotation, color, linesize);
				} //else { Debug.Log("don't need to recalculate spiralsphere " + name); }
				SetColor(lr, color);
				return this;
			}
			public Line_ Box(Vector3 size, Vector3 center = default(Vector3), Quaternion rotation = default(Quaternion), Color color = default(Color), float linesize = LINESIZE)
			{
				GameObject go = gameObject;
				if (!IsBox(center, size, rotation, linesize))
				{
					SetBox(center, size, rotation, linesize);
					lr = MakeBox(ref go, center, size, rotation, color, linesize);
				} //else { Debug.Log("don't need to recalculate box " + name); }
				SetColor(lr, color);
				return this;
			}
			private GameObject[] QuaternionAngleChildObjects(int objectCount, bool createIfNoneExist)
			{
				GameObject[] angleObjs = null;
				const string _A = "_A";
				if (transform.childCount == objectCount)
				{
					int childrenWithLineRenderers = 0;
					Transform[] children = new Transform[transform.childCount];
					for (int i = 0; i < children.Length; ++i) { children[i] = transform.GetChild(i); }
					System.Array.ForEach(children, (child) => {
						if (child.name.Contains(_A) && child.GetComponent<LineRenderer>() != null) { ++childrenWithLineRenderers; }
					});
					if (childrenWithLineRenderers >= objectCount)
					{
						angleObjs = new GameObject[objectCount];
						int validLine = 0;
						for (int i = 0; i < children.Length && validLine < angleObjs.Length; ++i)
						{
							if (children[i].name.Contains(_A) && children[i].GetComponent<LineRenderer>() != null)
								angleObjs[validLine++] = children[i].gameObject;
						}
					}
				}
				if (angleObjs == null && createIfNoneExist)
				{
					angleObjs = new GameObject[objectCount];
					for (int i = 0; i < angleObjs.Length; ++i)
					{
						GameObject angleObject = new GameObject(name + _A + i);
						angleObject.transform.SetParent(transform);
						angleObjs[i] = angleObject;
					}
				}
				return angleObjs;
			}
			private static Vector3[] default_quaternion_visualization_points = new Vector3[] { Vector3.forward, Vector3.up };
			public Line_ Quaternion(Quaternion q, Color color, Vector3 position = default(Vector3), Vector3[] startPoints = null,
				Quaternion orientation = default(Quaternion), int arcPoints = -1, float linesize = LINESIZE)
			{
				GameObject go = gameObject;
				float an; Vector3 ax;
				q.ToAngleAxis(out an, out ax);
				if (startPoints == null) { startPoints = default_quaternion_visualization_points; }
				if (!IsQuaternion(an, ax, position, startPoints, orientation, arcPoints, linesize))
				{
					SetQuaternion(an, ax, position, startPoints, orientation, arcPoints, linesize);
					GameObject[] angleObjs = QuaternionAngleChildObjects(startPoints.Length, true);
					MakeQuaternion(ref go, angleObjs, ax, an, position, color, orientation, arcPoints, linesize, ARROWSIZE, startPoints);
					lr = go.GetComponent<LineRenderer>();
				} //else { Debug.Log("don't need to recalculate quaternion " + name); }
				SetColor(lr, color);
				return this;
			}
		}

		/// <summary>
		/// Make the specified Line.
		/// example usage:
		/// <para><code>
		/// /* GameObject forwardLine should be a member variable */
		/// Lines.Make (ref forwardLine, transform.position,
		///             transform.position + transform.forward, Color.blue, 0.1f, 0);
		/// //This makes a long thin triangle, pointing forward.
		/// </code></para>
		/// </summary>
		/// <param name="lineObject">GameObject host of the LineRenderer</param>
		/// <param name="start">Start, an absolute world-space coordinate</param>
		/// <param name="end">End, an absolute world-space coordinate</param>
		/// <param name="startSize">How wide the line is at the start</param>
		/// <param name="endSize">How wide the line is at the end</param>
		public static LineRenderer Make(ref GameObject lineObject, Vector3 start, Vector3 end,
			Color color = default(Color), float startSize = LINESIZE, float endSize = LINESIZE)
		{
			LineRenderer lr = MakeLineRenderer(ref lineObject, color, startSize, endSize);
			lr.positionCount = 2;
			lr.SetPosition(0, start); lr.SetPosition(1, end);
			return lr;
		}

		/// <summary>convenience method</summary>
		[System.Obsolete("use Lines.Make(name).Line instead, it's more performant.")]
		public static LineRenderer Make(string lineName, Vector3 start, Vector3 end,
			Color color = default(Color), float startSize = LINESIZE, float endSize = LINESIZE)
		{
			GameObject go = Get(lineName, true);
			return Make(ref go, start, end, color, startSize, endSize);
		}

		/// <summary>Make the specified Line from a list of points</summary>
		/// <returns>The LineRenderer hosting the line</returns>
		/// <param name="lineObject">GameObject host of the LineRenderer</param>
		/// <param name="color">Color of the line</param>
		/// <param name="points">List of absolute world-space coordinates</param>
		/// <param name="pointCount">Number of the points used points list</param>
		/// <param name="startSize">How wide the line is at the start</param>
		/// <param name="endSize">How wide the line is at the end</param>
		public static LineRenderer Make(ref GameObject lineObject, IList<Vector3> points, int pointCount,
			Color color = default(Color), float startSize = LINESIZE, float endSize = LINESIZE)
		{
			LineRenderer lr = MakeLineRenderer(ref lineObject, color, startSize, endSize);
			lr.positionCount = pointCount;
			for (int i = 0; i < pointCount; ++i) { lr.SetPosition(i, points[i]); }
			return lr;
		}
		/// <summary>convenience version of LineRenderer Make(ref GameObject lineObject, IList<Vector3> points, int pointCount,
		/// Color color = default(Color), float startSize = LINESIZE, float endSize = LINESIZE)</summary>
		[System.Obsolete("use Lines.Make(name).Line instead, it's more performant.")]
		public static LineRenderer Make(string lineName, IList<Vector3> points,
			Color color = default(Color), float startSize = LINESIZE, float endSize = LINESIZE)
		{
			GameObject go = Get(lineName, true);
			return Make(ref go, points, points.Count, color, startSize, endSize);
		}

		public static LineRenderer MakeLineRenderer(ref GameObject lineObject)
		{
			if (lineObject == null)
			{
				lineObject = new GameObject();
				if (Instance().autoParentLinesToGlobalObject)
				{
					lineObject.transform.SetParent(instance.transform);
				}
			}
			LineRenderer lr = lineObject.GetComponent<LineRenderer>();
			if (lr == null) { lr = lineObject.AddComponent<LineRenderer>(); }
			return lr;
		}

		public static LineRenderer MakeLineRenderer(ref GameObject lineObject, Color color, float startSize, float endSize)
		{
			LineRenderer lr = MakeLineRenderer(ref lineObject);
			lr.startWidth = startSize;
			lr.endWidth = endSize;
			SetColor(lr, color);
			return lr;
		}

		public static Material FindShaderMaterial(string shadername)
		{
			Shader s = Shader.Find(shadername);
			if (s == null)
			{
				throw new System.Exception("Missing shader: " + shadername
					+ ". Please make sure it is in the \"Resources\" folder, "
					+ "or used by at least one other object. Or, create an "
					+ " object with Lines, and assign the material manually");
			}
			return new Material(s);
		}

		public static void SetColor(LineRenderer lr, Color color)
		{
			Material mat = Instance().lineMaterial;
			if (mat == null)
			{
				const string colorShaderName = "Sprites/Default";//"Unlit/Color";
				mat = FindShaderMaterial(colorShaderName);
				Instance().lineMaterial = mat;
			}
			if (lr.material == null || lr.material.name != mat.name) { lr.material = mat; }
			if (color == default(Color)) { color = Color.magenta; } else
			{
				float h, s, v;
				long t = System.DateTime.Now.Ticks / System.TimeSpan.TicksPerMillisecond;
				long duration = 500;
				long secComponent = t % duration;
				float a = Mathf.Abs((2f * secComponent - duration) / duration);
				Color.RGBToHSV(color, out h, out s, out v);
				color = Color.HSVToRGB(h, s + (a * .25f), v + (a * .25f));
			}
			lr.material.color = color;
		}

		/// <summary>Write 2D arc in 3D space, into given Vector3 array</summary>
		/// <param name="points">Will host the list of coordinates</param>
		/// <param name="pointCount">How many vertices to make &gt; 1</param>
		/// <param name="normal">The surface-normal of the arc's plane</param>
		/// <param name="firstPoint">Arc start, rotate about Vector3.zero</param>
		/// <param name="angle">2D angle. Tip: Vector3.Angle(v1, v2)</param>
		/// <param name="offset">How to translate the arc</param>
		public static void WriteArc(ref Vector3[] points, int pointCount,
			Vector3 normal, Vector3 firstPoint, float angle = 360, Vector3 offset = default(Vector3), int startIndex = 0)
		{
			if (pointCount < 0)
			{
				pointCount = (int)Mathf.Abs(24 * angle / 180f) + 1;
			}
			if (pointCount < 0 || pointCount >= 32767) { Debug.LogError("bad point count value: " + pointCount); }
			if (points == null) { points = new Vector3[pointCount]; }
			if (startIndex >= points.Length) return;
			points[startIndex] = firstPoint;
			Quaternion q = Quaternion.AngleAxis(angle / (pointCount - 1), normal);
			for (int i = startIndex + 1; i < startIndex + pointCount; ++i) { points[i] = q * points[i - 1]; }
			if (offset != Vector3.zero)
				for (int i = startIndex; i < startIndex + pointCount; ++i) { points[i] += offset; }
		}

		[System.Obsolete("use Lines.Make(name).Arc instead, it's more performant.")]
		public static LineRenderer MakeArc(string name,
			float angle, int pointCount, Vector3 normal, Vector3 firstPoint,
			Vector3 center = default(Vector3), Color color = default(Color), float startSize = LINESIZE, float endSize = LINESIZE)
		{
			GameObject go = Get(name, true);
			return MakeArc(ref go, angle, pointCount, normal, firstPoint, center, color, startSize, endSize);
		}
		/// <summary>
		/// Make the specified arc line in 3D space. Example usage: <para><code>
		/// /* GameObject turnArc should be a member variable */
		/// Lines.MakeArc(ref turnArc, Vector3.Angle(transform.forward, direction), 
		/// 	10, Vector3.Cross(transform.forward, direction), 
		/// 	transform.forward, transform.position, Color.green, 0.1f, 0);
		/// // makes a curve showing the turn from transform.forward to direction
		/// </code></para>
		/// </summary>
		/// <returns>The LineRenderer hosting the line</returns>
		/// <param name="lineObject">GameObject host of the LineRenderer</param>
		/// <param name="color">Color of the line</param>
		/// <param name="center">Center of arc</param>
		/// <param name="normal">surface-normal of arc's plane</param>
		/// <param name="firstPoint">Arc start, rotate about Vector3.zero</param>
		/// <param name="angle">2D angle. Tip: Vector3.Angle(v1, v2)</param>
		/// <param name="pointCount">How many vertices to make &gt; 1</param>
		/// <param name="startSize">How wide the line is at the start</param>
		/// <param name="endSize">How wide the line is at the end</param>
		public static LineRenderer MakeArc(ref GameObject lineObj,
			float angle, int pointCount, Vector3 normal, Vector3 firstPoint,
			Vector3 center = default(Vector3), Color color = default(Color), float startSize = LINESIZE, float endSize = LINESIZE)
		{
			Vector3[] points = null;
			WriteArc(ref points, pointCount, normal, firstPoint, angle, center);
			return Make(ref lineObj, points, pointCount, color, startSize, endSize);
		}

		[System.Obsolete("use Lines.Make(name).Orbital instead, it's more performant.")]
		public static LineRenderer MakeLineOnSphere(string name, Vector3 sphereCenter, Vector3 start, Vector3 end,
			Color color = default(Color), float startSize = LINESIZE, float endSize = LINESIZE, int pointCount = 24)
		{
			GameObject go = Get(name, true);
			return MakeLineOnSphere(ref go, sphereCenter, start, end, color, startSize, endSize, pointCount);
		}
		public static LineRenderer MakeLineOnSphere(ref GameObject lineObj, Vector3 sphereCenter, Vector3 start, Vector3 end,
			Color color = default(Color), float startSize = LINESIZE, float endSize = LINESIZE, int pointCount = 24)
		{
			Vector3[] points = null;
			WriteArcOnSphere(ref points, pointCount, sphereCenter, start, end);
			return Make(ref lineObj, points, pointCount, color, startSize, endSize);
		}

		public static void WriteArcOnSphere(ref Vector3[] points, int pointCount, Vector3 sphereCenter, Vector3 start, Vector3 end)
		{
			Vector3 axis;
			if (start == -end)
			{
				axis = (start != Vector3.up && end != Vector3.up) ? Vector3.up : Vector3.right;
			} else
			{
				axis = Vector3.Cross(start, end).normalized;
			}
			Vector3 a = start - sphereCenter, b = end - sphereCenter;
			float arad = a.magnitude, brad = b.magnitude, angle = 0;
			if (arad != 0 && brad != 0)
			{
				a /= arad; b /= brad;
				angle = Vector3.Angle(a, b);
				if (float.IsNaN(angle)) { angle = 0; }
			}
			WriteArc(ref points, pointCount, axis, a, angle, Vector3.zero);
			float raddelta = brad - arad;
			for (int i = 0; i < points.Length; ++i)
			{
				points[i] = points[i] * ((i * raddelta / points.Length) + arad);
				points[i] += sphereCenter;
			}
		}

		[System.Obsolete("use Lines.Make(name).Circle instead, it's more performant.")]
		public static LineRenderer MakeCircle(string name, Vector3 center, Vector3 normal,
			Color color = default(Color), float radius = LINESIZE, int pointCount = 0, float lineSize = LINESIZE)
		{
			GameObject go = Get(name, true);
			return MakeCircle(ref go, center, normal, color, radius, pointCount, lineSize);
		}
		/// <summary>Makes a circle with a 3D line</summary>
		/// <returns>The LineRenderer hosting the line</returns>
		/// <param name="lineObj">GameObject host of the LineRenderer</param>
		/// <param name="color">Color of the line</param>
		/// <param name="center">Absolute world-space 3D coordinate</param>
		/// <param name="normal">Which way the circle is facing</param>
		/// <param name="radius"></param>
		/// <param name="pointCount">How many points to use for the circle. If zero, will do 24*PI*r</param>
		/// <param name="linesize">The width of the line</param>
		public static LineRenderer MakeCircle(ref GameObject lineObj, Vector3 center, Vector3 normal,
			Color color = default(Color), float radius = 1, int pointCount = 0, float lineSize = LINESIZE)
		{
			Vector3[] points = null;
			WriteCircle(ref points, center, normal, radius, pointCount);
			LineRenderer lr = Lines.Make(ref lineObj, points, points.Length, color, lineSize, lineSize);
			lr.loop = true;
			return lr;
		}

		public static int WriteCircle(ref Vector3[] points, Vector3 center, Vector3 normal, float radius = 1, int pointCount = 0)
		{
			if (pointCount == 0)
			{
				pointCount = (int)Mathf.Round(24 * 3.14159f * radius + 0.5f);
				if (points != null)
				{
					pointCount = Mathf.Min(points.Length, pointCount);
				}
			}
			Vector3 crossDir = (normal == Vector3.up || normal == Vector3.down) ? Vector3.forward : Vector3.up;
			Vector3 r = Vector3.Cross(normal, crossDir).normalized;
			WriteArc(ref points, pointCount, normal, r * radius, 360, center);
			return pointCount;
		}

		public static LineRenderer MakeSphere(string name, float radius = 1,
			Vector3 center = default(Vector3), Color color = default(Color), float linesize = LINESIZE)
		{
			GameObject go = Get(name, true);
			return MakeSphere(ref go, radius, center, color, linesize);
		}
		/// <returns>a line renderer in the shape of a sphere made of 3 circles, for the x.y.z axis</returns>
		/// <param name="lineObj">Line object.</param>
		/// <param name="radius">Radius.</param>
		/// <param name="center">Center.</param>
		/// <param name="color">Color.</param>
		/// <param name="linesize">Linesize.</param>
		public static LineRenderer MakeSphere(ref GameObject lineObj, float radius = 1,
			Vector3 center = default(Vector3), Color color = default(Color), float linesize = LINESIZE)
		{
			Vector3[] circles = new Vector3[24 * 3];
			Lines.WriteArc(ref circles, 24, Vector3.forward, Vector3.up, 360, center, 24 * 0);
			Lines.WriteArc(ref circles, 24, Vector3.right, Vector3.up, 360, center, 24 * 1);
			Lines.WriteArc(ref circles, 24, Vector3.up, Vector3.forward, 360, center, 24 * 2);
			if (radius != 1) { for (int i = 0; i < circles.Length; ++i) { circles[i] *= radius; } }
			return Lines.Make(ref lineObj, circles, circles.Length, color, linesize, linesize);
		}

		[System.Obsolete("use Lines.Make(name).Box instead, it's more performant.")]
		public static LineRenderer MakeBox(string name, Vector3 center,
			Vector3 size, Quaternion rotation, Color color = default(Color), float linesize = LINESIZE)
		{
			GameObject go = Get(name, true);
			return MakeBox(ref go, center, size, rotation, color, linesize);
		}
		public static LineRenderer MakeBox(ref GameObject lineObj, Vector3 center,
			Vector3 size, Quaternion rotation, Color color = default(Color), float linesize = LINESIZE)
		{
			Vector3 y = Vector3.up / 2 * size.y;
			Vector3 x = Vector3.right / 2 * size.x;
			Vector3 z = Vector3.forward / 2 * size.z;
			Vector3[] line = new Vector3[] {
				 z+y-x, -z+y-x, -z-y-x, -z-y+x, -z+y+x,  z+y+x,  z-y+x,  z-y-x,
				 z+y-x,  z+y+x,  z-y+x, -z-y+x, -z+y+x, -z+y-x, -z-y-x,  z-y-x
			};
			for (int i = 0; i < line.Length; ++i) { line[i] = rotation * line[i] + center; }
			LineRenderer lr = Make(ref lineObj, line, line.Length, color, linesize, linesize);
			lr.numCornerVertices = 4;
			return lr;
		}

		public static LineRenderer MakeMapPin(string name, Color c = default(Color), float size = 1, float lineWidth = 0.1f)
		{
			GameObject go = Get(name, true);
			return MakeMapPin(ref go, c, size, lineWidth);
		}
		private static Vector3[] mapPin_points_base = null;
		/// <summary>Draws a "map pin", which shows a visualization for direction and orientation</summary>
		/// <returns>The LineRenderer hosting the map pin line. The LineRenderer's transform can be adjusted!</returns>
		/// <param name="lineObj">Line object.</param>
		/// <param name="c">C: color</param>
		/// <param name="size">Size: radius of the map pin</param>
		/// <param name="lineWidth">Line width.</param>
		public static LineRenderer MakeMapPin(ref GameObject lineObj, Color c = default(Color), float size = 1, float lineWidth = 0.1f)
		{
			const float epsilon = 1 / 1024.0f;
			if (mapPin_points_base == null)
			{
				Vector3 pstn = Vector3.zero, fwrd = Vector3.forward * size, rght = Vector3.right * size, up__ = Vector3.up;
				float startAngle = (360.0f / 4) - (360.0f / 32);
				Vector3 v = Quaternion.AngleAxis(startAngle, up__) * fwrd;
				Lines.WriteArc(ref mapPin_points_base, 32, up__, v, 360, pstn);
				Vector3 tip = pstn + fwrd * Mathf.Sqrt(2);
				mapPin_points_base[0] = mapPin_points_base[mapPin_points_base.Length - 1];
				int m = (32 * 5 / 8) + 1;
				mapPin_points_base[m++] = mapPin_points_base[m] + (tip - mapPin_points_base[m]) * (1 - epsilon);
				mapPin_points_base[m++] = tip;
				int n = (32 * 7 / 8) + 1;
				while (n < 32) { mapPin_points_base[m++] = mapPin_points_base[n++]; }
				Vector3 side = pstn + rght;
				mapPin_points_base[m++] = mapPin_points_base[m] + (side - mapPin_points_base[m]) * (1 - epsilon);
				mapPin_points_base[m++] = pstn + rght;
				mapPin_points_base[m++] = pstn + rght * epsilon;
				mapPin_points_base[m++] = pstn;
				mapPin_points_base[m++] = pstn + up__ * size * (1 - epsilon);
				mapPin_points_base[m++] = pstn + up__ * size;
			}
			LineRenderer lr = Lines.Make(ref lineObj, mapPin_points_base, mapPin_points_base.Length, c, lineWidth, lineWidth);
			lr.useWorldSpace = false;
			return lr;
		}

		public static LineRenderer SetMapPin(string name, Transform t, Color c = default(Color), float size = 1, float lineWidth = LINESIZE)
		{
			GameObject go = Get(name, true);
			return SetMapPin(ref go, t, c, size, lineWidth);
		}
		/// <summary>Draws a "map pin", which shows a visualization for direction and orientation</summary>
		/// <returns>The LineRenderer hosting the map pin line</returns>
		/// <param name="lineObj">Line object.</param>
		/// <param name="t">t: the transform to attach the map pin visualisation to</param>
		/// <param name="c">C: color</param>
		/// <param name="size">Size: radius of the map pin</param>
		/// <param name="lineWidth">Line width.</param>
		public static LineRenderer SetMapPin(ref GameObject lineObj, Transform t, Color c = default(Color), float size = 1, float lineWidth = LINESIZE)
		{
			LineRenderer line_ = MakeMapPin(ref lineObj, c, size, lineWidth);
			line_.transform.SetParent(t);
			line_.transform.localPosition = Vector3.zero;
			line_.transform.localRotation = Quaternion.identity;
			return line_;
		}

		public static Vector3 GetForwardVector(Quaternion q)
		{
			return new Vector3(2 * (q.x * q.z + q.w * q.y),
				2 * (q.y * q.z + q.w * q.x),
				1 - 2 * (q.x * q.x + q.y * q.y));
		}
		public static Vector3 GetUpVector(Quaternion q)
		{
			return new Vector3(2 * (q.x * q.y + q.w * q.z),
				1 - 2 * (q.x * q.x + q.z * q.z),
				2 * (q.y * q.z + q.w * q.x));
		}
		public static Vector3 GetRightVector(Quaternion q)
		{
			return new Vector3(1 - 2 * (q.y * q.y + q.z * q.z),
				2 * (q.x * q.y + q.w * q.z),
				2 * (q.x * q.z + q.w * q.y));
		}

		/// <example>CreateSpiralSphere(transform.position, 0.5f, transform.up, transform.forward, 16, 8);</example>
		/// <summary>creates a line spiraled onto a sphere</summary>
		/// <param name="center"></param>
		/// <param name="radius"></param>
		/// <param name="axis">example: Vector3.up</param>
		/// <param name="axisFace">example: Vector3.right</param>
		/// <param name="sides"></param>
		/// <param name="rotations"></param>
		/// <returns></returns>
		public static Vector3[] CreateSpiralSphere(Vector3 center = default(Vector3), float radius = 1,
			Quaternion rotation = default(Quaternion), float sides = 12, float rotations = 6)
		{
			List<Vector3> points = new List<Vector3>(); // List instead of Array because sides and rotations are floats!
			Vector3 axis = Vector3.up;
			Vector3 axisFace = Vector3.right;
			if (sides != 0 && rotations != 0)
			{
				float iter = 0;
				float increment = 1f / (rotations * sides);
				points.Add(center + axis * radius);
				do
				{
					iter += increment;
					Quaternion faceTurn = Quaternion.AngleAxis(iter * 360 * rotations, axis);
					Vector3 newFace = faceTurn * axisFace;
					Quaternion q = Quaternion.LookRotation(newFace);
					Vector3 right = GetUpVector(q);
					Vector3 r = right * radius;
					q = Quaternion.AngleAxis(iter * 180, newFace);
					r = q * r;
					r = rotation * r;
					Vector3 newPoint = center + r;
					points.Add(newPoint);
				}
				while (iter < 1);
			}
			return points.ToArray();
		}

		[System.Obsolete("use Lines.Make(name).SpiralSphere instead, it's more performant.")]
		public static LineRenderer MakeSpiralSphere(string name, float radius = 1,
			Vector3 center = default(Vector3), Quaternion rotation = default(Quaternion), Color color = default(Color), float linesize = LINESIZE)
		{
			GameObject go = Get(name, true);
			return MakeSpiralSphere(ref go, radius, center, rotation, color, linesize);
		}
		/// <returns>a line renderer in the shape of a spiraling sphere, spiraling about the Vector3.up axis</returns>
		/// <param name="lineObj">Line object.</param>
		/// <param name="radius">Radius.</param>
		/// <param name="center">Center.</param>
		/// <param name="color">Color.</param>
		/// <param name="linesize">Linesize.</param>
		public static LineRenderer MakeSpiralSphere(ref GameObject lineObj, float radius = 1,
			Vector3 center = default(Vector3), Quaternion rotation = default(Quaternion), Color color = default(Color), float linesize = LINESIZE)
		{
			Vector3[] verts = CreateSpiralSphere(center, radius, rotation, 24, 3);
			return Make(ref lineObj, verts, verts.Length, color, linesize, linesize);
		}

		/// <summary>draws the given vector as an arrow</summary>
		/// <param name="name">a unique name for the vector</param>
		/// <param name="vector">math definition of the vector</param>
		/// <param name="color"></param>
		/// <param name="startSize"></param>
		/// <param name="endSize"></param>
		/// <returns></returns>
		[System.Obsolete("use Lines.Make(name).Arrow instead, it's more performant.")]
		public static LineRenderer MakeArrow(string name, Vector3 vector,
			Color color = default(Color), float startSize = LINESIZE, float endSize = LINESIZE)
		{
			GameObject go = Get(name, true);
			return MakeArrow(ref go, Vector3.zero, vector, color, startSize, endSize);
		}
		/// <summary>draws the given vector as an arrow</summary>
		/// <param name="name">a unique name for the vector</param>
		/// <param name="start">where the vector should start</param>
		/// <param name="end">where the vector should end</param>
		/// <param name="color"></param>
		/// <param name="startSize"></param>
		/// <param name="endSize"></param>
		/// <returns></returns>
		[System.Obsolete("use Lines.Make(name).Arrow instead, it's more performant.")]
		public static LineRenderer MakeArrow(string name, Vector3 start, Vector3 end,
			Color color = default(Color), float startSize = LINESIZE, float endSize = LINESIZE, float arrowHeadSize = ARROWSIZE)
		{
			GameObject go = Get(name, true);
			return MakeArrow(ref go, start, end, color, startSize, endSize, arrowHeadSize);
		}
		[System.Obsolete("use Lines.Make(name).Arrow instead, it's more performant.")]
		public static LineRenderer MakeArrow(string name, IList<Vector3> points, int pointCount,
			Color color = default(Color), float startSize = LINESIZE, float endSize = LINESIZE, float arrowHeadSize = ARROWSIZE, Keyframe[] lineKeyFrames = null)
		{
			GameObject go = Get(name, true);
			return MakeArrow(ref go, points, pointCount, color, startSize, endSize, arrowHeadSize, lineKeyFrames);
		}

		public const float ARROWSIZE = 3, LINESIZE = 1f / 8;
		public static LineRenderer MakeArrow(ref GameObject lineObject, Vector3 start, Vector3 end,
			Color color = default(Color), float startSize = LINESIZE, float endSize = LINESIZE, float arrowHeadSize = ARROWSIZE)
		{
			return MakeArrow(ref lineObject, new Vector3[] { start, end }, 2, color, startSize, endSize, arrowHeadSize);
		}

		public static LineRenderer MakeArrow(ref GameObject lineObject, IList<Vector3> points, int pointCount,
			Color color = default(Color), float startSize = LINESIZE, float endSize = LINESIZE, float arrowHeadSize = ARROWSIZE, Keyframe[] lineKeyFrames = null)
		{
			Vector3[] line;
			Keyframe[] keyframes = CalculateArrowKeyframes(points, pointCount, out line, startSize, endSize, arrowHeadSize, lineKeyFrames);
			LineRenderer lr = Make(ref lineObject, line, line.Length, color, startSize, endSize);
			lr.widthCurve = new AnimationCurve(keyframes);
			return lr;
		}

		public static Keyframe[] CalculateArrowKeyframes(IList<Vector3> points, int pointCount, out Vector3[] line,
			float startSize = LINESIZE, float endSize = LINESIZE, float arrowHeadSize = ARROWSIZE, Keyframe[] lineKeyFrames = null)
		{
			float arrowSize = endSize * arrowHeadSize;
			int lastGoodIndex = 0;
			Vector3 arrowheadBase = Vector3.zero, arrowheadWidest = Vector3.zero;
			const float distanceBetweenArrowBaseAndWidePoint = 1.0f / 512;
			Vector3 delta, dir = Vector3.zero;
			// find where, in the list of points, to place the arrowhead
			float dist = 0, extraFromLastGoodIndex = 0;
			for (int i = points.Count - 1; i > 0; --i)
			{ // go backwards (from the pointy end)
				float d = Vector3.Distance(points[i], points[i - 1]);
				dist += d;
				// if the arrow direction hasn't been calculated and sufficient distance for the arrowhead has been passed
				if (dir == Vector3.zero && dist >= arrowSize)
				{
					// calculate w,here the arrowheadBase should be (requires 2 points) based on the direction of this segment
					lastGoodIndex = i - 1;
					delta = points[i] - points[i - 1];
					dir = delta.normalized;
					extraFromLastGoodIndex = dist - arrowSize;
					arrowheadBase = points[lastGoodIndex] + dir * extraFromLastGoodIndex;
				}
			}
			// if the line is not long enough for an arrow head, make the whole thing an arrowhead
			if (dist <= arrowSize)
			{
				line = new Vector3[] { points[0], points[points.Count - 1] };
				return new Keyframe[] { new Keyframe(0, arrowSize), new Keyframe(1, 0) };
			}
			delta = points[points.Count - 1] - arrowheadBase;
			dir = delta.normalized;
			arrowheadWidest = arrowheadBase + dir * (dist * distanceBetweenArrowBaseAndWidePoint);
			line = new Vector3[lastGoodIndex + 4];
			for (int i = 0; i <= lastGoodIndex; i++)
			{
				line[i] = points[i];
			}
			line[lastGoodIndex + 3] = points[points.Count - 1];
			line[lastGoodIndex + 2] = arrowheadWidest;
			line[lastGoodIndex + 1] = arrowheadBase;
			Keyframe[] keyframes;
			float arrowHeadBaseStart = 1 - arrowSize / dist;
			float arrowHeadBaseWidest = 1 - (arrowSize / dist - distanceBetweenArrowBaseAndWidePoint);
			if (lineKeyFrames == null)
			{
				keyframes = new Keyframe[] {
					new Keyframe(0, startSize), new Keyframe(arrowHeadBaseStart, endSize),
					new Keyframe(arrowHeadBaseWidest, arrowSize), new Keyframe(1, 0)
				};
			} else
			{
				// count how many there are after arrowHeadBaseStart.
				float t = 0;
				int validCount = lineKeyFrames.Length;
				for (int i = 0; i < lineKeyFrames.Length; ++i)
				{
					t = lineKeyFrames[i].time;
					if (t > arrowHeadBaseStart) { validCount = i; break; }
				}
				// those are irrelivant now. they'll be replaced by the 3 extra points
				keyframes = new Keyframe[validCount + 3];
				for (int i = 0; i < validCount; ++i) { keyframes[i] = lineKeyFrames[i]; }
				keyframes[validCount + 0] = new Keyframe(arrowHeadBaseStart, endSize);
				keyframes[validCount + 1] = new Keyframe(arrowHeadBaseWidest, arrowSize);
				keyframes[validCount + 2] = new Keyframe(1, 0);
			}
			return keyframes;
		}

		[System.Obsolete("use Lines.Make(name).Line instead, it's more performant.")]
		public static LineRenderer MakeArrowBothEnds(string name, Vector3 start, Vector3 end,
			Color color = default(Color), float startSize = LINESIZE, float endSize = LINESIZE, float arrowHeadSize = ARROWSIZE)
		{
			GameObject go = Get(name, true);
			return MakeArrowBothEnds(ref go, start, end, color, startSize, endSize, arrowHeadSize);
		}
		public static LineRenderer MakeArrowBothEnds(ref GameObject lineObject, Vector3 start, Vector3 end,
			Color color = default(Color), float startSize = LINESIZE, float endSize = LINESIZE, float arrowHeadSize = ARROWSIZE)
		{
			return MakeArrowBothEnds(ref lineObject, new Vector3[] { end, start }, 2, color, startSize, endSize, arrowHeadSize);
		}
		public static LineRenderer MakeArrowBothEnds(ref GameObject lineObject, IList<Vector3> points, int pointCount,
			Color color = default(Color), float startSize = LINESIZE, float endSize = LINESIZE, float arrowHeadSize = ARROWSIZE)
		{
			LineRenderer lr = MakeArrow(ref lineObject, points, pointCount, color, startSize, endSize, arrowHeadSize, null);
			ReverseLineInternal(ref lr);
			Vector3[] p = new Vector3[lr.positionCount];
			lr.GetPositions(p);
			lr = MakeArrow(ref lineObject, p, p.Length, color, endSize, startSize, arrowHeadSize, lr.widthCurve.keys);
			ReverseLineInternal(ref lr);
			return lr;
		}
		public static LineRenderer ReverseLineInternal(ref LineRenderer lr)
		{
			Vector3[] p = new Vector3[lr.positionCount];
			lr.GetPositions(p);
			System.Array.Reverse(p);
			lr.SetPositions(p);
			if (lr.widthCurve != null && lr.widthCurve.length > 1)
			{
				Keyframe[] kf = new Keyframe[lr.widthCurve.keys.Length];
				Keyframe[] okf = lr.widthCurve.keys;
				System.Array.Copy(lr.widthCurve.keys, kf, lr.widthCurve.keys.Length); //for(int i = 0; i<kf.Length; ++i) { kf[i]=okf[i]; }
				System.Array.Reverse(kf);
				for (int i = 0; i < kf.Length; ++i) { kf[i].time = 1 - kf[i].time; }
				lr.widthCurve = new AnimationCurve(kf);
			}
			return lr;
		}

		[System.Obsolete("use Lines.Make(name).Arc instead, it's more performant.")]
		public static LineRenderer MakeArcArrow(string name, Vector3 start, Vector3 end, Color color = default(Color), float angle = 90, Vector3 upNormal = default(Vector3),
			float startSize = LINESIZE, float endSize = LINESIZE, float arrowHeadSize = ARROWSIZE, int pointCount = 0)
		{
			GameObject go = Get(name, true);
			return MakeArcArrow(ref go, start, end, color, angle, upNormal, startSize, endSize, arrowHeadSize, pointCount);
		}
		public static LineRenderer MakeArcArrow(ref GameObject lineObj,
			float angle, int pointCount, Vector3 arcPlaneNormal = default(Vector3), Vector3 firstPoint = default(Vector3),
			Vector3 center = default(Vector3), Color color = default(Color), float startSize = LINESIZE, float endSize = LINESIZE, float arrowHeadSize = ARROWSIZE)
		{
			if (arcPlaneNormal == default(Vector3)) { arcPlaneNormal = Vector3.up; }
			if (center == default(Vector3) && firstPoint == default(Vector3)) { firstPoint = Vector3.right; }
			Vector3[] points = null;
			WriteArc(ref points, pointCount, arcPlaneNormal, firstPoint, angle, center);
			return MakeArrow(ref lineObj, points, pointCount, color, startSize, endSize, arrowHeadSize);
		}

		public static LineRenderer MakeArcArrowBothEnds(ref GameObject lineObj,
			float angle, int pointCount, Vector3 arcPlaneNormal = default(Vector3), Vector3 firstPoint = default(Vector3),
			Vector3 center = default(Vector3), Color color = default(Color), float startSize = LINESIZE, float endSize = LINESIZE, float arrowHeadSize = ARROWSIZE)
		{
			LineRenderer lr = MakeArcArrow(ref lineObj, angle, pointCount, arcPlaneNormal, firstPoint, center, color, startSize, endSize, arrowHeadSize);
			ReverseLineInternal(ref lr);
			Vector3[] p = new Vector3[lr.positionCount];
			lr.GetPositions(p);
			lr = MakeArrow(ref lineObj, p, p.Length, color, endSize, startSize, arrowHeadSize, lr.widthCurve.keys);
			ReverseLineInternal(ref lr);
			return lr;
		}
		[System.Obsolete("use Lines.Make(name).Line instead, it's more performant.")]
		public static LineRenderer MakeArcArrowBothEnds(string name,
			float angle, int pointCount, Vector3 arcPlaneNormal = default(Vector3), Vector3 firstPoint = default(Vector3),
			Vector3 center = default(Vector3), Color color = default(Color), float startSize = LINESIZE, float endSize = LINESIZE, float arrowHeadSize = ARROWSIZE)
		{
			GameObject go = Get(name, true);
			return MakeArcArrowBothEnds(ref go, angle, pointCount, arcPlaneNormal, firstPoint, center, color, startSize, endSize, arrowHeadSize);
		}
		public static LineRenderer MakeArcArrow(ref GameObject lineObject, Vector3 start, Vector3 end, Color color = default(Color), float angle = 90, Vector3 upNormal = default(Vector3),
			float startSize = LINESIZE, float endSize = LINESIZE, float arrowHeadSize = ARROWSIZE, int pointCount = 0)
		{
			Vector3[] arc;
			if (end == start || Mathf.Abs(angle) >= 360)
			{
				arc = new Vector3[] { start, end };
			} else
			{
				if (upNormal == default(Vector3)) { upNormal = Vector3.up; }
				if (pointCount == 0) { pointCount = Mathf.Max((int)(angle * 24 / 180) + 1, 2); }
				arc = new Vector3[pointCount];
				Vector3 delta = end - start;
				float dist = delta.magnitude;
				Vector3 dir = delta / dist;
				Vector3 right = Vector3.Cross(upNormal, dir).normalized;
				WriteArc(ref arc, pointCount, right, -upNormal, angle);
				Vector3 arcDelta = arc[arc.Length - 1] - arc[0];
				float arcDist = arcDelta.magnitude;
				float angleDiff = Vector3.Angle(arcDelta / arcDist, delta / dist);
				Quaternion turn = Quaternion.AngleAxis(angleDiff, right);
				float ratio = dist / arcDist;
				for (int i = 0; i < arc.Length; ++i)
				{
					arc[i] = (turn * arc[i]) * ratio;
				}
				Vector3 offset = start - arc[0];
				for (int i = 0; i < arc.Length; ++i)
				{
					arc[i] += offset;
				}
			}
			return MakeArrow(ref lineObject, arc, arc.Length, color, startSize, endSize, arrowHeadSize);
		}

		[System.Obsolete("use Lines.Make(name).Quaternion instead, it's more performant.")]
		public static void MakeQuaternion(string name, Quaternion q,
			Vector3 position = default(Vector3), Color color = default(Color), Vector3[] startPoint = null, Quaternion orientation = default(Quaternion),
			int arcPoints = 24, float lineSize = LINESIZE, float arrowHeadSize = ARROWSIZE)
		{
			string axisname = name + "_X";
			Vector3 axis;
			float angle;
			q.ToAngleAxis(out angle, out axis);
			GameObject axisObj = Get(axisname, true);
			if (startPoint == null || startPoint.Length == 0)
			{
				startPoint = new Vector3[] {
					Vector3.forward, Vector3.up,
				};
			}
			GameObject[] angleObjects = new GameObject[startPoint.Length];
			for (int i = 0; i < startPoint.Length; ++i)
			{
				angleObjects[i] = Get(name + "_A" + i, true);
			}
			MakeQuaternion(ref axisObj, angleObjects, axis, angle, position, color, orientation, arcPoints, lineSize, arrowHeadSize, startPoint);
		}

		public static void MakeQuaternion(ref GameObject axisObj, GameObject[] angleObj, Vector3 axis, float angle,
			Vector3 position = default(Vector3), Color color = default(Color), Quaternion orientation = default(Quaternion),
			int arcPoints = 24, float lineSize = LINESIZE, float arrowHeadSize = ARROWSIZE, Vector3[] startPoint = null)
		{
			if (angleObj.Length != startPoint.Length) { throw new System.Exception("angleObj and startPoint should be parallel arrays"); }
			while (angle >= 180) { angle -= 360; }
			while (angle < -180) { angle += 360; }
			Vector3 axisRotated = orientation * axis;
			MakeArrow(ref axisObj, position - axisRotated, position + axisRotated, color, lineSize, lineSize, arrowHeadSize);
			for (int i = 0; i < angleObj.Length; ++i)
			{
				GameObject aObj = angleObj[i];
				MakeArcArrow(ref aObj, angle, arcPoints, axisRotated, startPoint[i], position, color, lineSize, lineSize, arrowHeadSize);
				angleObj[i] = aObj;
			}
		}

		// code to draw quaternion with a single line renderer is commented out because it doesn't work right yet.
		///// <summary>draws a quaternion with a single line renderer. the visual is very buggy for small angles. Keyframes are finicky.</summary>
		///// <param name="lineObject"></param>
		///// <param name="axis"></param>
		///// <param name="angle"></param>
		///// <param name="color"></param>
		///// <param name="start"></param>
		///// <param name="forwardNormal"></param>
		///// <param name="startSize"></param>
		///// <param name="endSize"></param>
		///// <param name="arrowHeadSize"></param>
		///// <param name="pointCount"></param>
		///// <returns></returns>
		//public static LineRenderer MakeQuaternion(ref GameObject lineObject, Vector3 axis, float angle = 90, Color color = default(Color), 
		//	Vector3 start = default(Vector3), Vector3 forwardNormal = default(Vector3),
		//	float startSize = LINESIZE, float endSize = LINESIZE, float arrowHeadSize = ARROWSIZE, int pointCount = 0) {
		//	Vector3[] arc;
		//	if(forwardNormal == default(Vector3)) {
		//		if(axis == Vector3.forward || axis == Vector3.back) { forwardNormal = Vector3.up; }
		//		else { forwardNormal = Vector3.forward; }
		//	}
		//	if(pointCount == 0) { pointCount = Mathf.Max((int)((angle * 24) / 180)+1, 2); }
		//	//Debug.Log(angle + " " + pointCount);
		//	arc = new Vector3[pointCount+6];
		//	Vector3 right = Vector3.Cross(forwardNormal, axis).normalized;
		//	WriteArc(ref arc, pointCount, axis, right, angle);
		//	System.Array.Reverse(arc, 0, pointCount);
		//	for(int i = 0; i < arc.Length; ++i) {
		//		arc[i] = arc[i] * .5f + start;
		//	}
		//	const float epsilon = (1f / 256);
		//	arc[pointCount + 0] = arc[pointCount - 1] + (arc[pointCount-1]-arc[pointCount-2]).normalized * epsilon;
		//	arc[pointCount + 1] = start;
		//	arc[pointCount + 2] = start - axis / 4;
		//	arc[pointCount + 3] = start - axis /3;
		//	arc[pointCount + 4] = start - axis /2;
		//	arc[pointCount + 5] = start + axis /2;
		//	LineRenderer lr = MakeArrowBothEnds(ref lineObject, arc, arc.Length, color, startSize, endSize, arrowHeadSize);
		//	// calculate where to lower the width of the bridge connecting the axis and angle
		//	float connectionSegmentStart = 0, connectionSegmentEnd = 0, totalDistance = 0;
		//	int angleBeginIndex = arc.Length - 6;
		//	int axisEndIndex = arc.Length - 3;
		//	for(int i = 1; i < angleBeginIndex; ++i) {
		//		float d = Vector3.Distance(arc[i], arc[i-1]);
		//		connectionSegmentStart += d;
		//	}
		//	connectionSegmentEnd += connectionSegmentStart;
		//	for(int i = angleBeginIndex; i < axisEndIndex; ++i) {
		//		float d = Vector3.Distance(arc[i], arc[i-1]);
		//		connectionSegmentEnd += d;
		//	}
		//	totalDistance += connectionSegmentEnd;
		//	for(int i = axisEndIndex; i < arc.Length; ++i) {
		//		float d = Vector3.Distance(arc[i], arc[i-1]);
		//		totalDistance += d;
		//	}
		//	//Debug.Log(connectionSegmentStart+" < "+ connectionSegmentEnd+" < "+ totalDistance);
		//	AnimationCurve ac = lr.widthCurve;
		//	Keyframe endOfArc = new Keyframe((connectionSegmentStart - epsilon) / totalDistance, startSize);//, 0, -float.MaxValue);
		//	Keyframe beginningOfBridge = new Keyframe((connectionSegmentStart + epsilon) / totalDistance, 0, -float.MaxValue, 0);
		//	Keyframe endOfBridge = new Keyframe(connectionSegmentEnd / totalDistance, 0, 0, float.MaxValue);
		//	Keyframe beginningOfAxis = new Keyframe(connectionSegmentEnd / totalDistance + epsilon, endSize);//, float.MaxValue, 0);
		//	ac.AddKey(endOfArc);
		//	ac.AddKey(beginningOfBridge);
		//	ac.AddKey(endOfBridge);
		//	ac.AddKey(beginningOfAxis);
		//	lr.widthCurve = ac;
		//	return lr;
		//}
		//public static LineRenderer MakeQuaternion(ref GameObject lineObject, Quaternion q, Color color = default(Color), 
		//	Vector3 start = default(Vector3), Vector3 fowardNormal = default(Vector3),
		//	float startSize = LINESIZE, float endSize = LINESIZE, float arrowHeadSize = ARROWSIZE, int pointCount = 0) {
		//	Vector3 axis;
		//	float angle;
		//	q.ToAngleAxis(out angle, out axis);
		//	return MakeQuaternion(ref lineObject, axis, angle, color, start, fowardNormal, startSize, endSize, arrowHeadSize, pointCount);
		//}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="rectTransform"></param>
		/// <param name="x0"></param>
		/// <param name="y0"></param>
		/// <param name="x1"></param>
		/// <param name="y1"></param>
		/// <param name="col"></param>
		public static void DrawLine(RectTransform rectTransform, int x0, int y0, int x1, int y1, Color col)
		{
			UnityEngine.UI.RawImage rimg = rectTransform.GetComponent<UnityEngine.UI.RawImage>();
			if (rimg == null)
			{
				rimg = rectTransform.gameObject.AddComponent<UnityEngine.UI.RawImage>();
			}
			if (rimg == null) { throw new System.Exception("unable to create a RawImage on " + rectTransform.name + ", does it already have another renderer?"); }
			if (rimg.color.a == 0) { Color c = rimg.color; c.a = 1; rimg.color = c; }
			Texture2D img = rimg.texture as Texture2D;
			if (img == null)
			{
				Rect r = rectTransform.rect;
				img = new Texture2D((int)r.width, (int)r.height);
				img.SetPixels(0, 0, (int)r.width, (int)r.height, new Color[(int)(r.width * r.height)]); // set pixels to the default color, which is clear
				rimg.texture = img;
			}
			DrawLine(img, x0, y0, x1, y1, col);
			img.Apply();
		}

		/// <summary>draws an un-aliased single-pixel line on the given texture with the given color</summary>ne
		/// <param name="texture"></param>
		/// <param name="x0"></param>
		/// <param name="y0"></param>
		/// <param name="x1"></param>
		/// <param name="y1"></param>
		/// <param name="color"></param>
		public static void DrawLine(Texture2D texture, int x0, int y0, int x1, int y1, Color color)
		{
			int dy = y1 - y0;
			int dx = x1 - x0;
			int stepy, stepx;
			if (dy < 0) { dy = -dy; stepy = -1; } else { stepy = 1; }
			if (dx < 0) { dx = -dx; stepx = -1; } else { stepx = 1; }
			dy <<= 1;
			dx <<= 1;
			float fraction = 0;
			texture.SetPixel(x0, y0, color);
			if (dx > dy)
			{
				fraction = dy - (dx >> 1);
				while (Mathf.Abs(x0 - x1) > 1)
				{
					if (fraction >= 0)
					{
						y0 += stepy;
						fraction -= dx;
					}
					x0 += stepx;
					fraction += dy;
					texture.SetPixel(x0, y0, color);
				}
			} else
			{
				fraction = dx - (dy >> 1);
				while (Mathf.Abs(y0 - y1) > 1)
				{
					if (fraction >= 0)
					{
						x0 += stepx;
						fraction -= dy;
					}
					y0 += stepy;
					fraction += dx;
					texture.SetPixel(x0, y0, color);
				}
			}
		}
	}
}