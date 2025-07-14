using System;
using System.Collections;
using System.Collections.Generic;
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

            if (data == null || data.Count == 0)
            {
                Debug.LogWarning($"No save data found for {saveable.name} with GUID: {saveable.instanceID}");
                continue;
            }

            ES3.Save(saveable.instanceID.ToString(), data, "saveData.es3");
            Debug.Log($"Saved {data.Count} data modules for {saveable.name} with GUID: {saveable.instanceID}");
        }

        UDictionary<string, bool> flags = GameStateManager.Instance.GetAll();

        ES3.Save("flags", flags, "saveData.es3");
        Debug.Log("Game state flags saved successfully.");
    }

    [Button("Load Game")]
    public void LoadGame()
    {
        // Load data from disk
        foreach (var saveable in FindObjectsByType<SaveableObject>(FindObjectsSortMode.None))
        {
            try
            {
                var data = ES3.Load<List<SaveData>>(saveable.instanceID.ToString(), "saveData.es3");
                // Apply the loaded data to the saveable object
                saveable.LoadSaveData(data);

                Debug.Log($"Loaded {data.Count} data modules for {saveable.name} with GUID: {saveable.instanceID}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error loading save data for {saveable.name} with GUID: {saveable.instanceID}. Exception: {ex.Message}");
                continue;
            }
        }

        try
        {
            LoadGameStateFlags();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error loading game state flags: {ex.Message}");
        }
    }

    [Button("Clear Saves")]
    public void ClearSaves()
    {
        // Clear all saved data
        ES3.DeleteFile("saveData.es3");
        Debug.Log("All save data cleared.");
    }

    public void LoadGameStateFlags()
    {
        // Load game state flags from disk
        UDictionary<string, bool> flags = ES3.Load<UDictionary<string, bool>>("flags", "saveData.es3");
        GameStateManager.Instance.LoadAll(flags);
        Debug.Log("Game state flags loaded successfully.");
    }
}