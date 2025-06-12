using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;

public enum NoiseFalloffType
{
	LINEAR = 0,
	QUADRATIC = 1,
	CUBIC = 2
}

public class NoiseManager : MonoBehaviour
{
	public static NoiseManager Instance { get; private set; }

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
			DontDestroyOnLoad(gameObject);
		}
		else
		{
			Destroy(gameObject);
		}
	}

	[ReadOnly, SerializeField] public Dictionary<Vector2, NoiseListener> listeners = new Dictionary<Vector2, NoiseListener>();

	public void UpdateListenerPosition(Vector2 previousPosition, Vector2 newPosition)
	{
		if (listeners.TryGetValue(previousPosition, out NoiseListener listener))
		{
			var tmp = listener;
			listeners.Remove(previousPosition);
			listeners.Add(newPosition, tmp);
		}
	}

	public void CheckForListeners(Vector2 noiseLocation, float volume)
	{
		if (!listeners.ContainsKey(noiseLocation)) return;

		NoiseListener listener = listeners[noiseLocation];

		if (listener.NoiseThreshold <= volume)
			listener.OnHeardNoise?.Invoke(noiseLocation, volume);
	}
}
