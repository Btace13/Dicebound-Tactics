using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public static class GridPositioningEvaluator
{
    public static Vector3 FindUnexploredArea(Vector3 startingPosition, int range, Queue<Vector3> recentPositions)
    {
        // Example: Target areas the AI hasn't recently visited
        return GridManager.Instance.GetWalkablePositions(startingPosition, range)
                   .OrderByDescending(pos => DistanceFromRecentLocations(new Vector3(pos.x, startingPosition.y, pos.y), recentPositions))
                   .FirstOrDefault();
    }

    private static float DistanceFromPosition(Vector3 position, Vector3 targetPosition)
    {
        return Vector3.Distance(targetPosition, position);
    }

    /// <summary>
    /// Returns the minimum distance to the most recently visited point
    /// </summary>
    /// <param name="position">Position you are comparing</param>
    /// <param name="unit">Unit for the recent positions</param>
    /// <returns></returns>
    private static float DistanceFromRecentLocations(Vector3 position, Queue<Vector3> recentPositions)
    {
        float minDistance = float.MaxValue;

        foreach (Vector3 recentPosition in recentPositions)
        {
            float dist = DistanceFromPosition(position, recentPosition);

            if (dist < minDistance)
                minDistance = dist;
        }

        Debug.Log($"Min Distance from {position} is {minDistance} m");

        return minDistance;
    }

    private static float DistanceFromClosestObject<T>(List<T> objectsOfType, Vector3 position) where T : MonoBehaviour
    {
        float minDistance = float.MaxValue;

        foreach (T obj in objectsOfType)
        {
            float dist = DistanceFromPosition(position, obj.transform.position);

            if (dist < minDistance)
                minDistance = dist;
        }

        return minDistance;
    }

    public static bool IsLocationCloserToTarget(Vector3 previousLocation, Vector3 currentLocation, Vector3 targetPosition)
    {
        float oldDistance = DistanceFromPosition(previousLocation, targetPosition);
        float newDistance = DistanceFromPosition(currentLocation, targetPosition);

        return newDistance < oldDistance;
    }
}
