using UnityEngine;
using System.Collections.Generic;
using System;

public class SaveableObject : MonoBehaviour
{
    public Guid guid = Guid.NewGuid();

    [SerializeReference]
    public List<SaveData> saveDataModules = new();

    public List<SaveData> CollectSaveData() => saveDataModules;

    public void LoadSaveData(List<SaveData> loadedData = null)
    {
        if (loadedData == null)
        {
            try
            {
                loadedData = ES3.Load<List<SaveData>>(guid.ToString(), "saveData.es3");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"No save data found for {name} with GUID: {guid}. Exception: {ex.Message}");
                return;
            }
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