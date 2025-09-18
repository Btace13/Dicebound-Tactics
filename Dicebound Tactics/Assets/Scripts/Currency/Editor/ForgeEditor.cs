using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Forge))]
public class ForgeEditor : Editor
{
    private Forge forge;
    private SerializedProperty craftingRecipes;
    
    // For creating new recipes
    private string newRecipeName = "New Recipe";
    private string newRecipeDescription = "Recipe description";
    private CurrencyType newRecipeCurrencyType = CurrencyType.Shards;
    private int newRecipeCost = 50;
    private bool showRecipeCreator = false;

    private void OnEnable()
    {
        forge = (Forge)target;
        craftingRecipes = serializedObject.FindProperty("craftingRecipes");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Header
        EditorGUILayout.Space();
        GUILayout.Label("Forge Manager", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // Default inspector
        DrawDefaultInspector();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Forge Management", EditorStyles.boldLabel);

        // Recipe creator toggle
        showRecipeCreator = EditorGUILayout.Foldout(showRecipeCreator, "Create New Recipe", true);
        
        if (showRecipeCreator)
        {
            EditorGUI.indentLevel++;
            
            newRecipeName = EditorGUILayout.TextField("Recipe Name:", newRecipeName);
            newRecipeDescription = EditorGUILayout.TextField("Description:", newRecipeDescription);
            newRecipeCurrencyType = (CurrencyType)EditorGUILayout.EnumPopup("Currency Type:", newRecipeCurrencyType);
            newRecipeCost = EditorGUILayout.IntField("Cost:", newRecipeCost);
            
            if (GUILayout.Button("Create Recipe"))
            {
                CreateNewRecipe();
            }
            
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space();

        // Runtime controls
        if (Application.isPlaying)
        {
            EditorGUILayout.LabelField("Runtime Controls", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Open Forge"))
            {
                forge.OpenForge();
            }
            
            if (GUILayout.Button("Close Forge"))
            {
                forge.CloseForge();
            }
            EditorGUILayout.EndHorizontal();
            
            if (GUILayout.Button("Refresh Recipes"))
            {
                forge.RefreshRecipes();
            }

            // Show crafting status
            if (forge.IsCrafting)
            {
                EditorGUILayout.HelpBox("Forge is currently crafting an item...", MessageType.Info);
            }

            EditorGUILayout.Space();
            
            // Display available recipes
            var availableRecipes = forge.GetAvailableRecipes();
            if (availableRecipes.Length > 0)
            {
                EditorGUILayout.LabelField("Available Recipes:", EditorStyles.miniLabel);
                foreach (var recipe in availableRecipes)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField($"  {recipe.ItemName} - {recipe.CraftingCost}");
                    
                    GUI.enabled = !forge.IsCrafting && recipe.CanCraft();
                    if (GUILayout.Button("Test Craft", GUILayout.Width(100)))
                    {
                        forge.ProcessCrafting(recipe);
                    }
                    GUI.enabled = true;
                    
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

    private void CreateNewRecipe()
    {
        // Create a new CraftingRecipe
        var newRecipe = new CraftingRecipe();
        
        // Note: Since CraftingRecipe fields are serialized as private, we'd need to modify the class
        // to have public setters or a constructor. For now, this is a placeholder.
        
        // Add to recipes list
        craftingRecipes.arraySize++;
        var newRecipeProperty = craftingRecipes.GetArrayElementAtIndex(craftingRecipes.arraySize - 1);
        
        Debug.Log($"Created new recipe: {newRecipeName}");
        
        // Reset fields
        newRecipeName = "New Recipe";
        newRecipeDescription = "Recipe description";
        newRecipeCost = 50;
    }
}