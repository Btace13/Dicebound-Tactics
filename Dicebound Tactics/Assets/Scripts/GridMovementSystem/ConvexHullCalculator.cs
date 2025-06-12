using System;
using System.Collections.Generic;
using UnityEngine;

public class ConvexHullCalculator
{
    // Find the convex hull using the Graham Scan algorithm for Vector2 points
    public static List<Vector2> GrahamScan(List<Vector2> points)
    {
        if (points.Count <= 1) return points;

        // Sort points by x-coordinate (and y-coordinate for ties)
        points.Sort((a, b) => a.x != b.x ? a.x.CompareTo(b.x) : a.y.CompareTo(b.y));

        List<Vector2> hull = new List<Vector2>();

        // Lower hull
        foreach (var point in points)
        {
            while (hull.Count >= 2 && Cross(hull[hull.Count - 2], hull[hull.Count - 1], point) < 0) // Changed to < 0
            {
                hull.RemoveAt(hull.Count - 1);
            }
            hull.Add(point);
        }

        // Upper hull
        int lowerHullCount = hull.Count;
        for (int i = points.Count - 2; i >= 0; i--)
        {
            var point = points[i];

            while (hull.Count > lowerHullCount && Cross(hull[hull.Count - 2], hull[hull.Count - 1], point) < 0) // Changed to < 0
            {
                hull.RemoveAt(hull.Count - 1);
            }
            hull.Add(point);
        }

        hull.RemoveAt(hull.Count - 1); // Remove the last point since it's the same as the first
        Debug.Log("Final Convex Hull: " + string.Join(", ", hull));
        return hull;
    }

    // Find the convex hull using the Graham Scan algorithm for Vector3 points
    public static List<Vector3> GrahamScan(List<Vector3> points)
    {
        if (points.Count <= 1) return points;

        // Sort points by x-coordinate (and z-coordinate for ties)
        points.Sort((a, b) => a.x != b.x ? a.x.CompareTo(b.x) : a.z.CompareTo(b.z));

        List<Vector3> hull = new List<Vector3>();

        // Lower hull
        foreach (var point in points)
        {
            while (hull.Count >= 2 && Cross(hull[hull.Count - 2], hull[hull.Count - 1], point) < 0) // Changed to < 0
            {
                hull.RemoveAt(hull.Count - 1);
            }
            hull.Add(point);
        }

        // Upper hull
        int lowerHullCount = hull.Count;
        for (int i = points.Count - 2; i >= 0; i--)
        {
            var point = points[i];
            while (hull.Count > lowerHullCount && Cross(hull[hull.Count - 2], hull[hull.Count - 1], point) < 0) // Changed to < 0
            {
                hull.RemoveAt(hull.Count - 1);
            }
            hull.Add(point);
        }

        hull.RemoveAt(hull.Count - 1); // Remove the last point since it's the same as the first
        Debug.Log("Final Convex Hull (3D): " + string.Join(", ", hull));
        return hull;
    }

    // Cross product to determine the orientation of three points (2D)
    private static float Cross(Vector2 o, Vector2 a, Vector2 b)
    {
        return (a.x - o.x) * (b.y - o.y) - (a.y - o.y) * (b.x - o.x);
    }

    // Cross product to determine the orientation of three points (3D)
    private static float Cross(Vector3 o, Vector3 a, Vector3 b)
    {
        return (a.x - o.x) * (b.z - o.z) - (a.z - o.z) * (b.x - o.x);
    }
}
