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

            ES3.Save(saveable.guid.ToString(), data, "saveData.es3");
            Debug.Log($"Saved {data.Count} data modules for {saveable.name} with GUID: {saveable.guid}");
        }
    }

    [Button("Load Game")]
    public void LoadGame()
    {
        // Load data from disk
        foreach (var saveable in FindObjectsByType<SaveableObject>(FindObjectsSortMode.None))
        {
            try
            {
                var data = ES3.Load<List<SaveData>>(saveable.guid.ToString(), "saveData.es3");
                // Apply the loaded data to the saveable object
                saveable.LoadSaveData(data);

                Debug.Log($"Loaded {data.Count} data modules for {saveable.name} with GUID: {saveable.guid}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error loading save data for {saveable.name} with GUID: {saveable.guid}. Exception: {ex.Message}");
                continue;
            }
        }
    }

    [Button("Clear Saves")]
    public void ClearSaves()
    {
        // Clear all saved data
        ES3.DeleteFile("saveData.es3");
        Debug.Log("All save data cleared.");
    }
}