using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    IEnumerator Start()
    {
        // Initialize the save system, e.g., load existing saves or prepare for saving
        yield return null; // Simulate asynchronous operation
        LoadGame();
    }

    [Button("Save Game")]
    public void SaveGame()
    {
        foreach (var saveable in FindObjectsByType<SaveableObject>(FindObjectsSortMode.None))
        {
            var data = saveable.CollectSaveData();
            // Serialize and save data to disk
        }
    }

    [Button("Load Game")]
    public void LoadGame()
    {
        // Load data from disk
        foreach (var saveable in FindObjectsByType<SaveableObject>(FindObjectsSortMode.None))
        {
            var data = saveable.CollectSaveData();
            // Deserialize and apply data to the game objects
        }
    }
}