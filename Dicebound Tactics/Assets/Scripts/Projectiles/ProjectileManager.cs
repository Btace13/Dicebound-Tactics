using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;
using System;
using Unity.Collections;
using Unity.Jobs;
using TacticsToolkit;

public class ProjectileManager : MonoBehaviour
{
	public static ProjectileManager Instance { get; private set; }
	[HideInInspector] public bool useFriendlyFire = false;

	[System.Serializable]
	public enum ProjectilePath
	{
		STRAIGHT = 0,
		PARABOLIC = 1,
		HOMING = 2
	}

	[System.Serializable]
	public class ProjectileData
	{
		public GameObject projectileObject;
		public Vector3 origin;
		public Vector3 direction;
		public ParticleData projectileType;
		public ProjectilePath projectilePath;
		public Transform target;
		public float speed;
		public float damage;
		public float duration;
		public float timeAlive;
		public bool isReseting;
		public string shooterTag = "";
		public Action<RaycastHit> OnImpact;
		[HideInInspector] public Vector3 previousPosition;
	}

	[SerializeField] LayerMask layersToCheck;

	private static UDictionary<ProjectileData, GameObject> impactIndicators = new UDictionary<ProjectileData, GameObject>();
	private static UDictionary<ParticleData, List<GameObject>> pooledProjectiles = new UDictionary<ParticleData, List<GameObject>>();
	private static List<ProjectileData> activeProjectiles = new List<ProjectileData>();

	private NativeArray<SpherecastCommand> spherecastCommands;
	private NativeArray<RaycastHit> collisionResults;

	private JobHandle jobHandle;
	private bool jobScheduled = false;

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			Destroy(gameObject);
			return;
		}

		ClearProjectiles();
	}

	private void OnDestroy()
	{
		ClearProjectiles();
	}

	private void ClearProjectiles()
	{
		if (spherecastCommands.IsCreated) spherecastCommands.Dispose();
		if (collisionResults.IsCreated) collisionResults.Dispose();

		pooledProjectiles.Clear();
		activeProjectiles.Clear();
		objectsQueuedForCleanup.Clear();
	}

	private void Update()
	{
		// Only do collision checking if there are active projectiles
		if (activeProjectiles.Count > 0)
		{
			// Clean up first, before scheduling any jobs
			CleanupQueuedProjectiles();

			// Resize NativeArrays dynamically if projectile count changes
			if (!jobScheduled || spherecastCommands.Length != activeProjectiles.Count)
			{
				AllocateNativeArrays();
			}

			// Skip job scheduling if no projectiles remain after cleanup
			if (activeProjectiles.Count == 0)
				return;

			// Set up commands for each active projectile
			for (int i = 0; i < activeProjectiles.Count; i++)
			{
				UpdateProjectile(i);
			}

			// Schedule job to handle projectile collisions
			jobHandle = SpherecastCommand.ScheduleBatch(spherecastCommands, collisionResults, activeProjectiles.Count, jobHandle);

			jobScheduled = true;
		}
	}

	void LateUpdate()
	{
		if (!jobScheduled) return;

		// Complete the job and process results
		jobHandle.Complete();

		// Process results
		for (int i = 0; i < collisionResults.Length; i++)
		{
			// Debug.Log($"SphereCast Result {i}: {collisionResults[i].collider?.name}");
			HandleProjectileCollision(activeProjectiles[i], collisionResults[i]);
		}

		// Reset job flag
		jobScheduled = false;
	}

	void AllocateNativeArrays()
	{
		if (spherecastCommands.IsCreated) spherecastCommands.Dispose();
		if (collisionResults.IsCreated) collisionResults.Dispose();

		spherecastCommands = new NativeArray<SpherecastCommand>(activeProjectiles.Count, Allocator.TempJob);
		collisionResults = new NativeArray<RaycastHit>(activeProjectiles.Count, Allocator.TempJob);
	}

	/// <summary>
	/// Used to create object pools for required projectiles to cut down on performance loss during runtime
	/// </summary>
	private void CreateObjPool(ParticleData data)
	{
		if (pooledProjectiles.ContainsKey(data))
		{
			Debug.LogWarning($"Object pool for {data.ToString()} already exists");
			ExpandObjPool(data, 10);
			return;
		}

		List<GameObject> objPool = new List<GameObject>();

		for (int i = 0; i < 20; i++)
		{
			GameObject p = Instantiate(data.particleObject, transform);
			p.name = $"{data.ToString()}_{i + 1}";
			p.gameObject.SetActive(false);
			objPool.Add(p);
		}

		pooledProjectiles.Add(data, objPool);
	}

	private void ExpandObjPool(ParticleData data, int amount)
	{
		if (!pooledProjectiles.ContainsKey(data))
		{
			Debug.LogWarning($"Object pool for {data.ToString()} does not exist");
			return;
		}

		for (int i = 0; i < amount; i++)
		{
			GameObject p = Instantiate(data.particleObject, transform);
			p.name = $"{data.ToString()}_{pooledProjectiles[data].Count + 1}";
			p.gameObject.SetActive(false);
			pooledProjectiles[data].Add(p);
		}
	}

	/// <summary>
	/// Creates a projectile based on provided arguments
	/// </summary>
	/// <param name="origin">where the projectile is instantiated</param>
	/// <param name="direction">the facing / travel direction of the projectile</param>
	/// <param name="projectileScale">how large is the projectile in meters</param>
	/// <param name="projectileType">the type of projectile to spawn, given its corresponding enum</param>
	/// <param name="speed">the speed at which the projectile travels over one second</param>
	/// <param name="damage">the amount of damage to inflict on impact</param>
	/// <param name="duration">how long the projectile should exist if it doesn't collide with anything</param>
	public static void CreateProjectile(Vector3 origin, Vector3 direction, Vector3 projectileScale, ParticleData projectileType, ProjectilePath projectilePath = ProjectilePath.STRAIGHT, float speed = 3, float damage = 0, float duration = -1, Action<RaycastHit> OnImpact = null, GameObject vfxPrefab = null, string shooterTag = "", Transform intendedTarget = null)
	{
		if (!pooledProjectiles.ContainsKey(projectileType) || pooledProjectiles[projectileType].Count == 0)
		{
			Instance.CreateObjPool(projectileType);
		}

		//grab pooled instance to use
		GameObject projectileObj = pooledProjectiles[projectileType][0];

		if (vfxPrefab != null)
		{
			projectileObj = Instantiate(vfxPrefab, Instance.transform);
		}

		//set position and orientation and starting scale
		projectileObj.transform.position = origin;
		projectileObj.transform.forward = direction;
		projectileObj.transform.localScale = Vector3.one * 0.01f;

		//if duration is less than 0, as in not set, consider it to not have a max duration
		duration = duration < 0 ? Mathf.Infinity : duration;

		//create and add projectile data and remove from pool
		ProjectileData data = new ProjectileData()
		{
			projectileObject = projectileObj,
			origin = origin,
			direction = direction,
			projectileType = projectileType,
			projectilePath = projectilePath,
			speed = speed,
			damage = damage,
			duration = duration,
			timeAlive = 0,
			isReseting = false,
			shooterTag = shooterTag,
			OnImpact = OnImpact,
			target = intendedTarget,
			previousPosition = origin // Initialize previous position to origin
		};

		activeProjectiles.Add(data);
		pooledProjectiles[projectileType].Remove(projectileObj);

		//enable projectile 
		projectileObj.SetActive(true);

		if (projectilePath == ProjectilePath.PARABOLIC)
		{
			GameObject impactIndicatorObject = Resources.Load<GameObject>("ImpactIndicator");

			if (impactIndicatorObject != null)
			{
				if (data.target == null)
				{
					Debug.LogWarning("Target is null for parabolic projectile");
					return;
				}

				Vector3 endPoint = data.target.position + Vector3.up; // Adjusted to account for the projectile's scale

				GameObject impactIndicator = Instantiate(impactIndicatorObject, endPoint, Quaternion.identity);
				impactIndicator.transform.localScale = Vector3.one * projectileScale.x * 0.5f;
				impactIndicator.transform.rotation = Quaternion.LookRotation(Vector3.down, Vector3.forward); // Point indicator downwards

				impactIndicators.Add(data, impactIndicator);
			}
			else
			{
				Debug.LogWarning("Impact indicator prefab not found in Resources folder");
			}
		}
	}

	/// <summary>
	/// Used to create a group of projectiles in a radial pattern
	/// </summary>
	/// <param name="origin"></param>
	/// <param name="forwardDir"></param>
	/// <param name="projectileScale"></param>
	/// <param name="count"></param>
	/// <param name="projectileType"></param>
	/// <param name="angleOffsetInDegrees"></param>
	/// <param name="startRadius"></param>
	/// <param name="speed"></param>
	/// <param name="damage"></param>
	/// <param name="duration"></param>
	public static void Circle(Vector3 origin, Vector3 forwardDir, Vector3 projectileScale, int count, ParticleData projectileType, ProjectilePath projectilePath = ProjectilePath.STRAIGHT, float angleOffsetInDegrees = 0, float startRadius = 0, float speed = 3, float damage = 0, float duration = -1)
	{
		for (int i = 0; i < count; i++)
		{
			Vector3 offsetDirection = Quaternion.Euler(0, i * 360f / count + angleOffsetInDegrees, 0) * forwardDir;
			Vector3 offsetOrigin = origin + offsetDirection * startRadius;

			CreateProjectile(offsetOrigin, offsetDirection, projectileScale, projectileType, projectilePath, speed, damage, duration);
		}
	}

	public static void Cone(Vector3 origin, Vector3 forwardDir, Vector3 projectileScale, int count, ParticleData projectileType, ProjectilePath projectilePath = ProjectilePath.STRAIGHT, float coneAngleInDegrees = 45, float startRadius = 0, float speed = 3, float damage = 0, float duration = -1)
	{
		float startAngle = coneAngleInDegrees * -0.5f;
		float incrementAmount = coneAngleInDegrees / count;

		for (float a = startAngle; a < -startAngle; a += incrementAmount)
		{
			Vector3 offsetDirection = Quaternion.Euler(0, a, 0) * forwardDir;
			Vector3 offsetOrigin = origin + offsetDirection * startRadius;

			CreateProjectile(offsetOrigin, offsetDirection, projectileScale, projectileType, projectilePath, speed, damage, duration);
		}
	}

	/// <summary>
	/// Called every frame for active projectiles
	/// Manages state of active projectiles, including whether or not to destroy the projectile on collision or time alive
	/// </summary>
	/// <param name="data"></param>
	private void UpdateProjectile(int index)
	{
		ProjectileData data = activeProjectiles[index];

		if (data.projectilePath == ProjectilePath.HOMING && data.target != null)
		{
			data.direction = (data.target.position - data.projectileObject.transform.position).normalized;
			data.projectileObject.transform.rotation = Quaternion.RotateTowards(data.projectileObject.transform.rotation, Quaternion.LookRotation(data.direction), 720f * Time.deltaTime);
		}
		else if (data.projectilePath == ProjectilePath.PARABOLIC)
		{
			data.direction += Physics.gravity * Time.deltaTime;
			data.projectileObject.transform.rotation = Quaternion.RotateTowards(data.projectileObject.transform.rotation, Quaternion.LookRotation(data.direction), 720f * Time.deltaTime);
		}

		data.projectileObject.transform.localScale = Vector3.Lerp(data.projectileObject.transform.localScale, Vector3.one, Mathf.Clamp01(data.timeAlive));

		if (data.timeAlive < 0.25f)
		{
			// Set a non-colliding spherecast command (e.g., zero distance)
			// This prevents collision checks during the initial phase.
			spherecastCommands[index] = new SpherecastCommand(
										data.projectileObject.transform.position, // Current position
										0f, // Zero radius to avoid hits
										data.direction,
										new QueryParameters(), // Default query parameters
										0f); // Zero distance

			data.previousPosition = data.projectileObject.transform.position; // Update previous position to current
			return;
		}

		// --- Proceed with normal collision setup if projectile is old enough ---

		var layers = layersToCheck;

		if (data.projectilePath == ProjectilePath.PARABOLIC)
		{
			layers = layers | (1 << LayerMask.NameToLayer("Ground"));
		}

		var queryParameters = new QueryParameters(layers, false, QueryTriggerInteraction.Ignore);
		// Calculate previous position more accurately based on current position and movement this frame
		Vector3 currentPosition = data.projectileObject.transform.position;
		float castDistance = (currentPosition - data.previousPosition).magnitude; // Use the actual movement distance

		// Ensure radius is not zero if scaling from zero
		float currentRadius = Mathf.Max(data.projectileObject.transform.localScale.x * 0.5f, 0.01f);

		spherecastCommands[index] = new SpherecastCommand(
									data.previousPosition,
									currentRadius, // Use calculated radius
									data.direction,
									queryParameters,
									castDistance);
	}

	public void HandleProjectileCollision(ProjectileData data, RaycastHit hit)
	{
		if (hit.collider != null)
		{
			//if the object hit is damageable, apply damage
			if (hit.collider.gameObject.TryGetComponent(out Entity damageable))
			{
				// Only apply damage if the entity is properly initialized
				if (damageable != null && damageable.statsContainer != null && damageable.statsContainer.CurrentHealth != null)
				{
					damageable.TakeDamage((int)data.damage);
				}
				else
				{
					Debug.LogWarning($"Entity {hit.collider.name} is not properly initialized for damage calculation");
				}
			}

			//invoke any additional callbacks on hit
			data.OnImpact?.Invoke(hit);

			if (data.projectilePath == ProjectilePath.PARABOLIC && impactIndicators.ContainsKey(data))
			{
				Destroy(impactIndicators[data]);
				impactIndicators.Remove(data);
			}

			objectsQueuedForCleanup.Add(data);
			data.isReseting = true;
			data.OnImpact = null;
			GameObject impactObj = Instantiate(data.projectileType.impactObject, hit.point, Quaternion.LookRotation(hit.normal));
			StartCoroutine(DelayDestroy(impactObj));
			return;
		}

		data.previousPosition = data.projectileObject.transform.position; // Update previous position to current
		data.projectileObject.transform.rotation = Quaternion.RotateTowards(data.projectileObject.transform.rotation, Quaternion.LookRotation(data.direction), 720f * Time.deltaTime);
		data.projectileObject.transform.position += data.direction * data.speed * Time.deltaTime;
		data.timeAlive += Time.deltaTime;

		if (data.timeAlive >= data.duration)
		{
			// Destroy the impact indicator if it exists when the projectile expires
			if (data.projectilePath == ProjectilePath.PARABOLIC && impactIndicators.ContainsKey(data))
			{
				Destroy(impactIndicators[data]);
				impactIndicators.Remove(data);
			}

			data.isReseting = true;

			ScaleProjectile(data.projectileObject.transform, Vector3.one * 0.01f, 1f, () =>
			{
				objectsQueuedForCleanup.Add(data);
			});
		}
	}

	private static List<ProjectileData> objectsQueuedForCleanup = new List<ProjectileData>();

	/// <summary>
	/// Removes and readds to the object pool projectiles that have been queued to be inactive
	/// Prevents any enumeration issues from the previous frame
	/// </summary>
	private void CleanupQueuedProjectiles()
	{
		if (objectsQueuedForCleanup.Count == 0)
		{
			Debug.LogWarning("No projectiles queued for cleanup");
			return;
		}

		print($"Cleaning up {objectsQueuedForCleanup.Count} projectiles");

		// Create a new list to store indices to remove
		List<int> indicesToRemove = new List<int>();

		// Find the indices of items to remove
		for (int i = 0; i < activeProjectiles.Count; i++)
		{
			foreach (ProjectileData queuedData in objectsQueuedForCleanup)
			{
				if (activeProjectiles[i].projectileObject == queuedData.projectileObject)
				{
					indicesToRemove.Add(i);
					break;
				}
			}
		}

		// Remove items from highest index to lowest to avoid index shifting issues
		indicesToRemove.Sort();
		indicesToRemove.Reverse();

		foreach (int index in indicesToRemove)
		{
			ProjectileData data = activeProjectiles[index];
			data.projectileObject.SetActive(false);
			pooledProjectiles[data.projectileType].Add(data.projectileObject);
			activeProjectiles.RemoveAt(index);
		}

		objectsQueuedForCleanup.Clear();
	}

	/// <summary>
	/// Scales a projectile over time, up or down (used for beginning and end of projectile life)
	/// </summary>
	/// <param name="projectile"></param>
	/// <param name="endScale"></param>
	/// <param name="duration"></param>
	/// <param name="OnComplete"></param>
	public static void ScaleProjectile(Transform projectile, Vector3 endScale, float duration, UnityAction OnComplete = null)
	{
		projectile.DOScale(endScale, duration).onComplete += () =>
		{
			OnComplete?.Invoke();
		};
	}

	public static void CheckReflectProjectiles(Transform reflectOrigin, float radius = 2f)
	{
		for (int i = 0; i < activeProjectiles.Count; i++)
		{
			//skip over any that are in the process of being cleaned up or have made impact already
			if (activeProjectiles[i].isReseting) continue;

			float distance = Vector3.Distance(reflectOrigin.position, activeProjectiles[i].projectileObject.transform.position);
			float dotProd = Vector3.Dot(reflectOrigin.forward, (activeProjectiles[i].projectileObject.transform.position - reflectOrigin.position).normalized);
			float projectileRadius = activeProjectiles[i].projectileObject.transform.localScale.x * 0.5f;

			if (distance < radius && distance > projectileRadius && dotProd >= 0.25f)
			{
				ReflectProjectile(activeProjectiles[i], -activeProjectiles[i].direction);
			}
		}
	}

	/// <summary>
	/// Reflects the projectile in the new direction and resets time alive
	/// </summary>
	/// <param name="projectile"></param>
	/// <param name="direction"></param>
	private static void ReflectProjectile(ProjectileData projectile, Vector3 direction)
	{
		if (activeProjectiles.Contains(projectile))
		{
			ProjectileData data = projectile;

			print("Reflected projectile!");
			data.timeAlive = 0f;
			data.direction = direction;
			data.speed *= 1.5f;
			data.projectileObject.transform.rotation = Quaternion.LookRotation(direction);

			activeProjectiles[activeProjectiles.IndexOf(projectile)] = data;
		}
	}

	IEnumerator DelayDestroy(GameObject objectToDestroy)
	{
		yield return new WaitForSeconds(1f);

		Destroy(objectToDestroy);
	}
}
