using UnityEngine;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityExtensions;
using System.Collections.Generic;
using TacticsToolkit;

public class ProjectileController : MonoBehaviour
{
    [SerializeField] private SphereCollider sphereCollider;
    [SerializeField] private Transform origin;
    [SerializeField] private string shooterTag = "";
    [SerializeField] private bool overrideProjectilePath = false;
    [SerializeField, ShowIf("overrideProjectilePath")] ProjectileManager.ProjectilePath projectilePath = ProjectileManager.ProjectilePath.STRAIGHT;
    [SerializeField] private bool shouldRotateToTarget = true;
    [SerializeField] AbilitySO abilityData;

    private Transform _target;
    private List<Transform> _targets = new List<Transform>();
    private float _elapsedTime = 0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (sphereCollider == null)
        {
            sphereCollider = transform.GetOrAddComponent<SphereCollider>();
        }

        sphereCollider.isTrigger = true;
        sphereCollider.center = Vector3.zero;
        sphereCollider.radius = abilityData.range;

        if (origin == null)
        {
            origin = transform;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Entity>() != null)
        {
            if (_targets.Contains(other.transform)) return;

            _targets.Add(other.transform);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<Entity>() != null)
        {
            if (!_targets.Contains(other.transform)) return;

            _targets.Remove(other.transform);
        }
    }

    void Update()
    {
        if (_targets.Count == 0) return;

        _target = GetClosestTarget() ?? _target;

        if (_target == null) return;

        if (shouldRotateToTarget)
        {
            Vector3 direction = (_target.position - origin.position);
            direction.y = 0; // Ignore vertical difference for Y-axis rotation only

            // Prevent LookRotation error if target is directly above/below
            if (direction != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 180f);
            }
        }

        if (abilityData == null)
        {
            Debug.LogError("Ability Data is not set.");
            return;
        }

        if (_target != null && Vector3.Dot((_target.position - origin.position).normalized, origin.forward) < 0.95f)
        {
            return;
        }

        if (_elapsedTime >= abilityData.cooldown)
        {
            _elapsedTime = 0f;
            LaunchProjectile(_target, abilityData.projectileData.projectileSpeed, abilityData.range, abilityData.projectileData.maxLifeTime, projectilePath);
        }

        _elapsedTime += Time.deltaTime;
    }

    public void LaunchProjectile(Transform target, float speed, float range, float maxLifeTime, ProjectileManager.ProjectilePath projectilePath)
    {
        if (abilityData == null)
        {
            Debug.LogError("Projectile Attack Data is not set.");
            return;
        }

        Vector3 launchDirection = origin.forward;
        float projectileSpeed = abilityData.projectileData.projectileSpeed;

        if (overrideProjectilePath ? projectilePath == ProjectileManager.ProjectilePath.PARABOLIC : abilityData.projectileData.projectilePath == ProjectileManager.ProjectilePath.PARABOLIC)
        {
            float dist = Mathf.Min(Vector3.Distance(origin.position, target.position), range);

            launchDirection = (target.position - origin.position).normalized;
            launchDirection += Vector3.up * Mathf.Sqrt(2 * Physics.gravity.magnitude * dist);
            projectileSpeed = launchDirection.magnitude / maxLifeTime;
        }

        ProjectileManager.CreateProjectile(
        origin.position, // origin position 
        launchDirection, // direction
        Vector3.one * 0.25f, // scale of the projectile
        abilityData.projectileData, // projectile data
        projectilePath, // projectile path
        projectileSpeed, // speed of the projectile
        (abilityData as DamageAbilitySO).damageAmount,
        maxLifeTime, // lifetime of the projectile
        hit =>
        { //impact callback

        },
        abilityData.projectileData.impactObject, // impact vfx prefab
        shooterTag, // shooter tag
        target // target
        );
    }

    public Transform GetClosestTarget()
    {
        if (_targets.Count == 0) return null;

        Transform closestTarget = _targets[0];
        float closestDistance = Vector3.Distance(origin.position, closestTarget.position);

        foreach (Transform target in _targets)
        {
            float distance = Vector3.Distance(origin.position, target.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestTarget = target;
            }
        }

        return closestTarget;
    }
}
