using UnityEngine;
using UnityEngine.Events;

public class NoiseListener : MonoBehaviour
{
	[Tooltip("The minimum level of noise necessary to \"hear\" a noise")]
	public float NoiseThreshold = 1;

	[Tooltip("Event invoked when a noise is registed on the noise listener")]
	public UnityEvent<Vector2, float> OnHeardNoise = new UnityEvent<Vector2, float>();

	public void Awake()
	{
		Vector2 gridPos = GridManager.Instance.WorldToGridPosition(transform.position);

		if (NoiseManager.Instance && !NoiseManager.Instance.listeners.ContainsKey(gridPos))
			NoiseManager.Instance.listeners.Add(gridPos, this);
		else
			Debug.LogWarning($"NoiseListener at {transform.position} already exists in NoiseManager's listeners dictionary.");
	}
}
