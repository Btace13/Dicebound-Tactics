using UnityEngine;
using UnityEditor;
using System.Linq;

#if UNITY_EDITOR
[CustomEditor(typeof(CombatItemInventory))]
public class CombatItemInventoryEditor : Editor
{
    public override void OnInspectorGUI()
    {
        CombatItemInventory inventory = (CombatItemInventory)target;
        
        // Check for available combat items
        var allCombatItems = Resources.FindObjectsOfTypeAll<CombatItem>();
        var assetCombatItems = allCombatItems.Where(item => AssetDatabase.Contains(item)).ToArray();
        
        // Show warning if there are null entries
        if (HasNullEntries(inventory))
        {
            EditorGUILayout.HelpBox("This inventory contains null entries. Click 'Clean Up Null Entries' to fix.", MessageType.Warning);
        }
        
        DrawDefaultInspector();
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Clean Up Null Entries"))
        {
            CleanupInventory(inventory);
            EditorUtility.SetDirty(inventory);
        }
        
        if (assetCombatItems.Length == 0 && GUILayout.Button("Clean Up Old Asset Files"))
        {
            EditorApplication.ExecuteMenuItem("Tools/Clean Up Old Asset Files");
        }
    }
    
    private bool HasNullEntries(CombatItemInventory inventory)
    {
        if (inventory.combatItems == null) return false;
        
        try
        {
            foreach (var entry in inventory.combatItems)
            {
                if (entry == null || entry.item == null) return true;
            }
        }
        catch (System.Exception)
        {
            return true; // If we can't iterate, assume there are issues
        }
        
        return false;
    }
    
    private void CleanupInventory(CombatItemInventory inventory)
    {
        if (inventory.combatItems == null)
        {
            inventory.combatItems = new System.Collections.Generic.List<CombatItemEntry>();
            return;
        }
        
        try
        {
            int removedCount = 0;
            
            // Remove null entries or entries with null items from the end backwards
            for (int i = inventory.combatItems.Count - 1; i >= 0; i--)
            {
                if (inventory.combatItems[i] == null || inventory.combatItems[i].item == null || inventory.combatItems[i].quantity <= 0)
                {
                    inventory.combatItems.RemoveAt(i);
                    removedCount++;
                }
            }
            
            if (removedCount > 0)
            {
                Debug.Log($"Cleaned up {removedCount} invalid entries from inventory.");
            }
            else
            {
                Debug.Log("No invalid entries found in inventory.");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error during manual cleanup: {ex.Message}");
            inventory.combatItems = new System.Collections.Generic.List<CombatItemEntry>();
        }
    }
}
#endif
