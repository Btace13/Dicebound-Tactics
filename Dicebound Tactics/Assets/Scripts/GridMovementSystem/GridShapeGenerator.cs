using UnityEngine;
using Shapes;
using System.Collections.Generic;

public class GridShapeGenerator : ImmediateModeShapeDrawer
{
	public struct PolylineData
	{
		public float LineThickness;
		public Color LineColor;
		public PolylinePath Path;
		public bool IsClosed;

		public PolylineData(PolylinePath p, float _lineThickness, Color _color, bool _isClosed)
		{
			Path = p;
			LineThickness = _lineThickness;
			LineColor = _color;
			IsClosed = _isClosed;
		}
	}

	public struct PolygonData
	{
		public Color Color;
		public PolygonPath Path;

		public PolygonData(PolygonPath p, Color _color)
		{
			Path = p;
			Color = _color;
		}
	}

	private Dictionary<GameObject, PolylineData> linesToDraw = new Dictionary<GameObject, PolylineData>();
	private Dictionary<GameObject, PolygonData> shapesToDraw = new Dictionary<GameObject, PolygonData>();

	private void Awake()
	{
		// set up static parameters. these are used for all following Draw.Line calls
		Draw.LineGeometry = LineGeometry.Volumetric3D;
		Draw.ThicknessSpace = ThicknessSpace.Meters;

		// set static parameter to draw in the local space of this object
		Draw.Matrix = transform.localToWorldMatrix;
	}

	/// <summary>
	/// Adds line or path to be drawn
	/// </summary>
	/// <param name="go">the object to associate the line with</param>
	/// <param name="path">the path of the line or shape</param>
	/// <param name="thickness">the thickness of the line</param>
	/// <param name="color">the color of the line</param>
	public void DrawPolyline(GameObject go, PolylinePath path, float thickness, Color color, bool isClosed)
	{
		PolylineData shape = new PolylineData(path, thickness, color, isClosed);

		if (linesToDraw.ContainsKey(go))
			linesToDraw[go] = shape;
		else
			linesToDraw.Add(go, shape);
	}

	public void DrawPolygon(GameObject go, PolygonPath path, Color color)
	{
		PolygonData shape = new PolygonData(path, color);

		if (shapesToDraw.ContainsKey(go))
			shapesToDraw[go] = shape;
		else
			shapesToDraw.Add(go, shape);
	}

	/// <summary>
	/// Clears line associated with game object
	/// </summary>
	/// <param name="go"></param>
	public void ClearLine(GameObject go)
	{
		if (linesToDraw.ContainsKey(go))
			linesToDraw.Remove(go);
	}

	/// <summary>
	/// Clears line associated with game object
	/// </summary>
	/// <param name="go"></param>
	public void ClearShape(GameObject go)
	{
		if (shapesToDraw.ContainsKey(go))
			shapesToDraw.Remove(go);
	}

	public override void DrawShapes(Camera cam)
	{
		using (Draw.Command(cam))
		{
			foreach (PolylineData line in linesToDraw.Values)
			{
				if (line.Path != null && line.Path.Count > 1)
					Draw.Polyline(line.Path, line.IsClosed, line.LineThickness, PolylineJoins.Round, line.LineColor);
				else
					Debug.LogWarning("Outline is null");
			}

			foreach (PolygonData polygon in shapesToDraw.Values)
			{
				if (polygon.Path != null && polygon.Path.Count > 1)
					Draw.Polygon(polygon.Path, polygon.Color);
				else
					Debug.LogWarning("Shape is null");
			}
		}
	}
}