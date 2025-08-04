using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

[BurstCompile]
public struct ProjectileCollisionJob : IJobParallelFor
{
    public NativeArray<Vector3> ProjectilePositions;
    public NativeArray<Vector3> ProjectileVelocities;
    public NativeArray<float> ProjectileRadius;
    public LayerMask layerMask;
    public NativeArray<bool> CollisionResults;

    public void Execute(int index)
    {
        Vector3 futurePos = ProjectilePositions[index] + ProjectileVelocities[index];

        float radius = ProjectileRadius[index];

        Collider[] hitColliders = new Collider[1];
        int hitCount = Physics.OverlapSphereNonAlloc(futurePos, radius, hitColliders, layerMask);

        CollisionResults[index] = hitCount > 0;
    }
}
