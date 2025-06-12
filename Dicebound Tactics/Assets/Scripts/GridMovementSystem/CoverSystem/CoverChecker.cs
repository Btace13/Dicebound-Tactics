using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class CoverChecker : MonoBehaviour
{
	[BoxGroup("Refrences"), SerializeField] GameObject coverIndicatorPrefab;
	[BoxGroup("Settings"), SerializeField] LayerMask coverLayer;

	private int poolSize = 16;
	private List<(GameObject, Material)> coverIndicators = new List<(GameObject, Material)>();

	private void Awake()
	{
		GeneratePool();
	}

	private void GeneratePool()
	{
		for (int i = 0; i < poolSize; i++)
		{
			GameObject indicator = Instantiate(coverIndicatorPrefab, transform);
			indicator.SetActive(false);
			MeshRenderer renderer = indicator.GetComponentInChildren<MeshRenderer>(true);
			Material copiedMat = renderer.material;
			renderer.material = copiedMat;
			coverIndicators.Add((indicator, renderer.material));
		}
	}

	public void CheckForCover(Vector3 position, int range = 2, float cellSize = 1, bool shouldFade = true)
	{
		int poolIndex = 0;

		for (int x = -range; x < range; x++)
		{
			for (int y = -range; y < range; y++)
			{
				Vector3 current = position + Vector3.right * x * cellSize + Vector3.forward * y * cellSize;

				//check if walkable, if not we can skip the raycasts for this tile
				bool isWalkable = GridManager.Instance.IsPositionWalkable(current);

				if (isWalkable)
				{
					for (int i = 0; i < 360; i+=90)
					{
						Vector3 dir = Quaternion.Euler(0, i, 0) * Vector3.forward;

						if (Physics.Raycast(current + Vector3.up, dir, out RaycastHit hit, cellSize, coverLayer))
						{
							(GameObject,Material) indicator = coverIndicators[poolIndex];
							indicator.Item1.transform.SetPositionAndRotation(current + (0.5f * cellSize * dir), Quaternion.LookRotation(hit.normal));
							indicator.Item1.transform.position += indicator.Item1.transform.forward * 0.05f; // slightly move off of wall
							Color c = indicator.Item2.color;
							c.a = Mathf.Clamp01(1f - ((x*x + y*y) / ((range-0.5f) * (range-0.5f))));
							indicator.Item2.color = c;
							indicator.Item1.SetActive(true);
							//print($"Indicator at {indicator.Item1.transform.position}");
							//increment pool index
							poolIndex++;
						}
					}
				}
			}
		}

		//turn off unused indicators
		for (int r = poolIndex; r < coverIndicators.Count; r++)
			coverIndicators[r].Item1.SetActive(false);
	}

	public void HideIndicators()
	{
		foreach (var indicator in coverIndicators)
		{
			indicator.Item1.SetActive(false);
		}
	}
}
