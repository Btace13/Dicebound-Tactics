using UnityEngine;
using System.Collections.Generic;
using System;

public class SaveableObject : MonoBehaviour
{
    public string instanceID => gameObject.GetInstanceID().ToString();

    [SerializeField] public List<SaveData> saveDataModules = new();

    public List<SaveData> CollectSaveData()
    {
        // Collect all SaveData modules attached to this GameObject
        foreach (var module in saveDataModules)
        {
            if (module != null)
            {
                module.Capture(gameObject);
            }
        }

        return saveDataModules;
    }

    public void LoadSaveData(List<SaveData> loadedData = null)
    {
        if (loadedData == null)
        {
            try
            {
                loadedData = ES3.Load<List<SaveData>>(instanceID.ToString(), "saveData.es3");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"No save data found for {name} with GUID: {instanceID}. Exception: {ex.Message}");
                return;
            }
        }
        else if (loadedData.Count == 0)
        {
            Debug.LogWarning($"No save data found for {name} with GUID: {instanceID}. Using empty data.");
            return;
        }

        saveDataModules = loadedData;

        ApplySaveData();
    }

    private void ApplySaveData()
    {
        foreach (var module in saveDataModules)
        {
            module.Apply(gameObject);
        }
    }
}