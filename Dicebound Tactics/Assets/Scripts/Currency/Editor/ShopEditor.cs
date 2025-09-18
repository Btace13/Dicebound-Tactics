using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(Shop))]
public class ShopEditor : Editor
{
    private Shop shop;
    private SerializedProperty shopItems;
    
    // For creating new items
    private string newItemName = "New Item";
    private string newItemDescription = "Item description";
    private CurrencyType newItemCurrencyType = CurrencyType.Gold;
    private int newItemCost = 100;
    private bool showItemCreator = false;

    private void OnEnable()
    {
        shop = (Shop)target;
        shopItems = serializedObject.FindProperty("shopItems");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Header
        EditorGUILayout.Space();
        GUILayout.Label("Shop Manager", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // Default inspector
        DrawDefaultInspector();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Shop Management", EditorStyles.boldLabel);

        // Item creator toggle
        showItemCreator = EditorGUILayout.Foldout(showItemCreator, "Create New Item", true);
        
        if (showItemCreator)
        {
            EditorGUI.indentLevel++;
            
            newItemName = EditorGUILayout.TextField("Item Name:", newItemName);
            newItemDescription = EditorGUILayout.TextField("Description:", newItemDescription);
            newItemCurrencyType = (CurrencyType)EditorGUILayout.EnumPopup("Currency Type:", newItemCurrencyType);
            newItemCost = EditorGUILayout.IntField("Cost:", newItemCost);
            
            if (GUILayout.Button("Create Shop Item"))
            {
                CreateNewShopItem();
            }
            
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space();

        // Runtime controls
        if (Application.isPlaying)
        {
            EditorGUILayout.LabelField("Runtime Controls", EditorStyles.boldLabel);
            
            if (GUILayout.Button("Refresh Shop Inventory"))
            {
                shop.RefreshInventory();
            }
            
            if (GUILayout.Button("Open Shop"))
            {
                shop.OpenShop();
            }
            
            if (GUILayout.Button("Close Shop"))
            {
                shop.CloseShop();
            }

            EditorGUILayout.Space();
            
            // Display available items
            var availableItems = shop.GetAvailableItems();
            if (availableItems.Length > 0)
            {
                EditorGUILayout.LabelField("Available Items:", EditorStyles.miniLabel);
                foreach (var item in availableItems)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField($"  {item.ItemName} - {item.Cost}");
                    if (GUILayout.Button("Test Purchase", GUILayout.Width(100)))
                    {
                        shop.ProcessPurchase(item);
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
        }
        else
        {
            EditorGUILayout.HelpBox("Runtime controls are only available during play mode.", MessageType.Info);
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void CreateNewShopItem()
    {
        // Create a new ShopItem
        var newItem = new ShopItem();
        
        // Note: Since ShopItem fields are serialized as private, we'd need to modify the class
        // to have public setters or a constructor. For now, this is a placeholder.
        
        // Add to shop items list
        shopItems.arraySize++;
        var newItemProperty = shopItems.GetArrayElementAtIndex(shopItems.arraySize - 1);
        
        // This would need to be implemented based on the actual ShopItem structure
        // newItemProperty.FindPropertyRelative("itemName").stringValue = newItemName;
        // newItemProperty.FindPropertyRelative("description").stringValue = newItemDescription;
        
        Debug.Log($"Created new shop item: {newItemName}");
        
        // Reset fields
        newItemName = "New Item";
        newItemDescription = "Item description";
        newItemCost = 100;
    }
}