using UnityEngine;
using System.Collections.Generic;

public class SaveableObject : MonoBehaviour
{
    [SerializeReference]
    public List<SaveData> saveDataModules = new();

    public List<SaveData> CollectSaveData() => saveDataModules;
}