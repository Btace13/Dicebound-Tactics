using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Pathfinding;

[RequireComponent(typeof(CharacterController), typeof(Seeker))]
public class GridCharacterController : MonoBehaviour, ISelectable
{
	[ReadOnly] Vector2 target = new Vector2();
	[Space(5)]
	[SerializeField] int distanceCanMove = 5;
	[SerializeField] int distanceCanSprint = 8;
	[SerializeField] float movementSpeed = 1;
	[SerializeField] float rotationSpeed = 1;

	public int DistanceCanMove { get => distanceCanMove; }
	public int DistanceCanSprint { get => distanceCanSprint; }
	public float MovementSpeed { get => movementSpeed; }
	public float SprintSpeed { get => movementSpeed * 2; }
	public float RotationSpeed { get => rotationSpeed; }

	public bool IsTakingAction { get; private set; } = false;
	public bool IsSelected { get; set; } = false;
	public bool IsSprinting { get; private set; } = false;

	private Path path;
	private Seeker seeker;
	public CharacterController controller { get; private set; }
	private int currentWaypoint = 0;
	private float nextWaypointDistance = 0.1f;
	private Vector3 targetDestination;

	private UnityAction<Vector2Int> _onGridLocationUpdated = null;
	public UnityAction<Vector2Int> OnGridLocationUpdated { get { return _onGridLocationUpdated; } set { _onGridLocationUpdated = value; } }

	private UnityAction _onReachedDestination = null;
	public UnityAction OnReachedDestination { get; set; }

	private Vector2 _currentGridPosition;

	[Header("Debugging")]
	[SerializeField] bool useDebugs = false;

	private void Start()
	{
		Init();
	}

	public void Init()
	{
		seeker = GetComponent<Seeker>();
		controller = GetComponent<CharacterController>();

		//reset character position so that it is in the middle of the closest grid square
		Vector2 gridPos = GridManager.Instance.GetNearestWalkablePosition(transform.position);
		transform.position = new Vector3(gridPos.x, transform.position.y, gridPos.y);
	}

	public void OnSelect()
	{
		IsSelected = true;

		if (GridManager.Instance)
		{
			GridManager.Instance.HideMoveableArea();
			GridManager.Instance.HidePathVisual();
			GridManager.Instance.UpdateMoveableArea(transform.position, distanceCanMove, distanceCanSprint);

			_onGridLocationUpdated?.Invoke(GridManager.Instance.WorldToGridPositionRounded(transform.position));
		}
	}

	public void OnDeselect()
	{
		IsSelected = false;

		if (path != null)
			path.Release(this);

		if (GridManager.Instance != null)
		{
			GridManager.Instance.moveableAreaHandler.HideMoveableArea();
			GridManager.Instance.HidePathVisual();
		}
	}

	private void OnGridUpdated()
	{
		if (!IsSelected) return;

		GridManager.Instance.UpdateMoveableArea(transform.position, distanceCanMove, distanceCanSprint);
	}

	private void OnActionMapChanged(InputManager.ActionMap actionMap)
	{
		if (actionMap != InputManager.ActionMap.PLAYER)
		{
			GridManager.Instance.moveableAreaHandler.HideMoveableArea();
			GridManager.Instance.HidePathVisual();
		}
		else if (IsSelected)
		{
			GridManager.Instance.UpdateMoveableArea(transform.position, distanceCanMove, distanceCanSprint);
		}
	}

	public void UpdatePathToTarget(Vector3 destination, bool showVisualizer = false, UnityAction<Path> OnPathCalculated = null)
	{
		Vector2 closestTarget = GridManager.Instance.GetNearestWalkablePosition(destination);
		Vector2 playerPos = GridManager.Instance.WorldToGridPosition(transform.position);

		//don't update path if over the same unit
		if (closestTarget == playerPos)
		{
			seeker.CancelCurrentPathRequest();
			GridManager.Instance.HidePathVisual();
			return;
		}

		targetDestination = new Vector3(closestTarget.x, destination.y, closestTarget.y);

		if (showVisualizer)
			GridManager.Instance.CoverChecker.CheckForCover(targetDestination, 2, (int)GridManager.Instance.CellSize);

		seeker.CancelCurrentPathRequest();
		seeker.StartPath(new Vector3(playerPos.x, transform.position.y, playerPos.y), targetDestination, p =>
		{
			p.Claim(this);

			//return if path has error
			if (p.error)
			{
				Debug.LogError("Error: " + p.errorLog);
				p.Release(this);
				return;
			}

			//we don't want to set the path if the unit is not selected or in the middle of an action
			if (IsTakingAction)
			{
				p.Release(this);
				return;
			}

			if (p != null && p.path.Count > 0)
			{
				if (useDebugs)
					print($"Found path of {p.path.Count} length");

				path = p;

				if (showVisualizer)
					UpdatePathVisual();

				OnPathCalculated?.Invoke(p);
			}
			else
			{
				if (useDebugs)
					Debug.LogWarning("No path found");

				p.Release(this);

				if (showVisualizer)
					UpdatePathVisual();
			}
		});
	}

	private void UpdatePathVisual()
	{
		if (path == null || path.path.Count < 2)
		{
			return;
		}

		List<Vector3> pathUpdated = new List<Vector3>();

		//calculate position slightly offset from the player in the direction of the first path node
		Vector3 playerPos = new Vector3(transform.position.x, 0.1f, transform.position.z);
		Vector3 dirToFirstNode = new Vector3(path.path[1].position.x, 0.1f, path.path[1].position.z) - playerPos;
		Vector3 offsetPlayerPosition = playerPos + dirToFirstNode.normalized;

		//start at offset player position 
		pathUpdated.Add(playerPos);

		for (int i = currentWaypoint + 1; i < path.vectorPath.Count; i++)
		{
			Vector3 waypoint = new Vector3(path.vectorPath[i].x, 0.1f, path.vectorPath[i].z);

			if (useDebugs)
				print($"Waypoint {i} = " + waypoint);

			pathUpdated.Add(waypoint);
		}

		if (pathUpdated.Count > 1)
		{
			// GridManager.Instance.DrawLine(pathUpdated, Color.white, 0.04f);
		}
	}

	public void MoveToTarget(UnityAction OnReachedDestination = null)
	{
		if (path != null && useDebugs)
			print("Path count = " + path.path.Count);

		//exit if there is no path or in the middle of an action
		if (IsTakingAction || path == null || path.path.Count == 0) return;

		//hide visualizations
		GridManager.Instance.HidePathVisual();
		GridManager.Instance.HideMoveableArea();

		GridManager.Instance.CoverChecker.HideIndicators();

		_onReachedDestination = OnReachedDestination;
		_onReachedDestination += this.OnReachedDestination;

		IsTakingAction = true;

		IsSprinting = path.path.Count - 1 > distanceCanMove;
	}

	public void StopMovement()
	{
		_onReachedDestination = null;

		IsTakingAction = false;
		currentWaypoint = 0; // remember to reset waypoint index, otherwise you'll encounter the minimum movement bug
		controller.SimpleMove(Vector3.zero);

		//clear path
		try { path.Release(this); }
		catch (System.Exception e)
		{
			Debug.LogWarning("Unable to release path. Possibly was unclaimed.");
			Debug.LogError($"{e.Message}");
		}
	}

	public void Update()
	{
		if (path == null || !IsTakingAction)
		{
			// We have no path to follow yet, so don't do anything
			return;
		}

		// Check in a loop if we are close enough to the current waypoint to switch to the next one.
		// We do this in a loop because many waypoints might be close to each other and we may reach
		// several of them in the same frame.
		bool reachedEndOfPath = false;
		// The distance to the next waypoint in the path
		float distanceToWaypoint = 0;

		while (true)
		{
			if (path == null || path.vectorPath.Count < currentWaypoint + 1)
			{
				IsTakingAction = false;
				return;
			}

			// If you want maximum performance you can check the squared distance instead to get rid of a
			// square root calculation. But that is outside the scope of this tutorial.
			distanceToWaypoint = Vector3.Distance(transform.position, path.vectorPath[currentWaypoint]);
			if (distanceToWaypoint < nextWaypointDistance)
			{
				// Check if there is another waypoint or if we have reached the end of the path
				if (currentWaypoint + 1 < path.vectorPath.Count)
				{
					if (currentWaypoint > 0)
						_onGridLocationUpdated?.Invoke(GridManager.Instance.WorldToGridPositionRounded(path.vectorPath[currentWaypoint]));

					currentWaypoint++;
				}
				else
				{
					// Set a status variable to indicate that the agent has reached the end of the path.
					// You can use this to trigger some special code if your game requires that.
					reachedEndOfPath = true;
					if (useDebugs)
						print("Reached end of path");
					break;
				}
			}
			else
			{
				break;
			}
		}

		// Slow down smoothly upon approaching the end of the path
		// This value will smoothly go from 1 to 0 as the agent approaches the last waypoint in the path.
		var speedFactor = reachedEndOfPath ? Mathf.Sqrt(distanceToWaypoint / nextWaypointDistance) : 1f;

		// Direction to the next waypoint
		// Normalize it so that it has a length of 1 world unit
		Vector3 sameHeightWaypoint = path.vectorPath[currentWaypoint];
		sameHeightWaypoint.y = transform.position.y;

		Vector3 dir = (sameHeightWaypoint - transform.position).normalized;
		// Multiply the direction by our desired speed to get a velocity
		Vector3 velocity = dir * (IsSprinting ? SprintSpeed : MovementSpeed) * speedFactor;

		// Move the agent using the CharacterController component
		// Note that SimpleMove takes a velocity in meters/second, so we should not multiply by Time.deltaTime
		controller.SimpleMove(velocity);
		controller.transform.rotation = Quaternion.Slerp(controller.transform.rotation, Quaternion.LookRotation(dir, Vector3.up), Time.deltaTime * rotationSpeed);

		//clean up path, if at end of path
		if (reachedEndOfPath)
		{
			transform.position = new Vector3(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y), Mathf.RoundToInt(transform.position.z));

			//invoke any events
			_onGridLocationUpdated?.Invoke(GridManager.Instance.WorldToGridPositionRounded(transform.position));
			_onReachedDestination?.Invoke();

			StopMovement();
		}
	}

	private void SetCurrentGridSpace(Vector2 gridSpace)
	{
		if (!IsSelected) return;

		_currentGridPosition = gridSpace;
	}
}
