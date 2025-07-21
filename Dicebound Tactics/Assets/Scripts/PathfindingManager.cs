using UnityEngine;
using Pathfinding;
using UnityEngine.SceneManagement;

public class PathfindingManager : MonoBehaviour
{
    [SerializeField] private AstarPath astarPath;

    private void Start()
    {
        if (astarPath == null)
        {
            astarPath = GetComponent<AstarPath>();
        }

        RecalculatePaths();

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (astarPath != null)
        {
            if (astarPath == null)
            {
                astarPath = GetComponent<AstarPath>();
            }

            RecalculatePaths();
        }
        else
        {
            Debug.LogError("AstarPath component not found in the scene after loading.");
        }
    }

    public void RecalculatePaths()
    {
        if (astarPath != null)
        {
            astarPath.Scan();
        }
    }
}
