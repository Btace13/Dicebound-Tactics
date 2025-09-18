using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;

public class Forge : MonoBehaviour, IForge
{
    [Header("Forge Information")]
    [SerializeField] private string forgeName = "Blacksmith Forge";
    [SerializeField] private string forgeDescription = "Craft weapons and equipment";

    [Header("Crafting Recipes")]
    [SerializeField] private List<CraftingRecipe> craftingRecipes = new List<CraftingRecipe>();

    [Header("Forge Settings")]
    [SerializeField] private bool autoRefreshOnStart = true;
    [SerializeField] private float craftingTime = 2f; // Time it takes to craft items

    [Header("Audio")]
    [SerializeField] private AudioClip craftingStartSound;
    [SerializeField] private AudioClip craftingCompleteSound;
    [SerializeField] private AudioClip craftingFailedSound;
    [SerializeField] private AudioClip forgeOpenSound;

    [Header("VFX")]
    [SerializeField] private ParticleSystem craftingParticles;
    [SerializeField] private Transform itemSpawnPoint;

    private bool isCrafting = false;

    // Events
    public System.Action<Forge> OnForgeOpened;
    public System.Action<Forge> OnForgeClosed;
    public System.Action<CraftingRecipe> OnCraftingStarted;
    public System.Action<CraftingRecipe> OnItemCrafted;
    public System.Action<CraftingRecipe> OnCraftingFailed;

    public string ForgeName => forgeName;
    public string ForgeDescription => forgeDescription;
    public bool IsCrafting => isCrafting;

    private void Start()
    {
        InitializeForge();
        
        if (autoRefreshOnStart)
        {
            RefreshRecipes();
        }
    }

    private void InitializeForge()
    {
        // Subscribe to recipe events
        foreach (var recipe in craftingRecipes)
        {
            recipe.OnCrafted += OnRecipeCraftedInternal;
            recipe.OnCraftingFailed += OnRecipeCraftingFailedInternal;
        }
    }

    public ICraftable[] GetAvailableRecipes()
    {
        return craftingRecipes.Where(recipe => recipe.IsUnlocked).Cast<ICraftable>().ToArray();
    }

    public CraftingRecipe[] GetAllRecipes()
    {
        return craftingRecipes.ToArray();
    }

    public bool ProcessCrafting(ICraftable craftable)
    {
        if (isCrafting)
        {
            Debug.LogWarning("Forge is already crafting an item!");
            return false;
        }

        if (craftable is CraftingRecipe recipe && craftingRecipes.Contains(recipe))
        {
            if (recipe.CanCraft())
            {
                StartCrafting(recipe);
                return true;
            }
        }
        return false;
    }

    private void StartCrafting(CraftingRecipe recipe)
    {
        isCrafting = true;
        
        PlayAudio(craftingStartSound);
        PlayCraftingParticles();
        
        OnCraftingStarted?.Invoke(recipe);
        
        // Start crafting process
        Invoke(nameof(CompleteCrafting), craftingTime);
        
        // Actually spend the currency and craft the item
        recipe.Craft();
    }

    private void CompleteCrafting()
    {
        isCrafting = false;
        PlayAudio(craftingCompleteSound);
        StopCraftingParticles();
    }

    public void RefreshRecipes()
    {
        // Load unlock states from save system
        LoadRecipeStates();

        Debug.Log($"{forgeName} recipes refreshed");
    }

    private void OnRecipeCraftedInternal(CraftingRecipe recipe)
    {
        OnItemCrafted?.Invoke(recipe);
        
        // Spawn the crafted item if we have a spawn point and prefab
        if (itemSpawnPoint != null && recipe.CraftedItemPrefab != null)
        {
            for (int i = 0; i < recipe.OutputQuantity; i++)
            {
                Vector3 spawnPosition = itemSpawnPoint.position + Vector3.up * i * 0.1f;
                Instantiate(recipe.CraftedItemPrefab, spawnPosition, itemSpawnPoint.rotation);
            }
        }
        
        SaveRecipeState(recipe);
    }

    private void OnRecipeCraftingFailedInternal(CraftingRecipe recipe)
    {
        isCrafting = false;
        PlayAudio(craftingFailedSound);
        StopCraftingParticles();
        OnCraftingFailed?.Invoke(recipe);
    }

    public void OpenForge()
    {
        PlayAudio(forgeOpenSound);
        RefreshRecipes();
        OnForgeOpened?.Invoke(this);
    }

    public void CloseForge()
    {
        OnForgeClosed?.Invoke(this);
    }

    private void PlayAudio(AudioClip clip)
    {
        if (clip != null)
        {
            // Create temporary audio source for one-shot audio
            GameObject audioObject = new GameObject("ForgeAudio");
            AudioSource audioSource = audioObject.AddComponent<AudioSource>();
            audioSource.clip = clip;
            audioSource.Play();
            Destroy(audioObject, clip.length);
        }
    }

    private void PlayCraftingParticles()
    {
        if (craftingParticles != null)
        {
            craftingParticles.Play();
        }
    }

    private void StopCraftingParticles()
    {
        if (craftingParticles != null)
        {
            craftingParticles.Stop();
        }
    }

    #region Recipe Management

    [Button("Add Test Recipes")]
    private void AddTestRecipes()
    {
        if (craftingRecipes == null) craftingRecipes = new List<CraftingRecipe>();

        // Add some test recipes
        var testRecipe = new CraftingRecipe();
        // Note: Since CraftingRecipe fields are private, we'd need to make them public or add setters
        // For now, this is just a structure example
        craftingRecipes.Add(testRecipe);
    }

    public void AddRecipe(CraftingRecipe recipe)
    {
        if (!craftingRecipes.Contains(recipe))
        {
            craftingRecipes.Add(recipe);
            recipe.OnCrafted += OnRecipeCraftedInternal;
            recipe.OnCraftingFailed += OnRecipeCraftingFailedInternal;
        }
    }

    public void RemoveRecipe(CraftingRecipe recipe)
    {
        if (craftingRecipes.Contains(recipe))
        {
            craftingRecipes.Remove(recipe);
            recipe.OnCrafted -= OnRecipeCraftedInternal;
            recipe.OnCraftingFailed -= OnRecipeCraftingFailedInternal;
        }
    }

    public CraftingRecipe GetRecipeByName(string recipeName)
    {
        return craftingRecipes.FirstOrDefault(recipe => recipe.ItemName == recipeName);
    }

    public void UnlockRecipe(string recipeName)
    {
        var recipe = GetRecipeByName(recipeName);
        if (recipe != null)
        {
            recipe.UnlockRecipe();
        }
    }

    #endregion

    #region Save/Load

    private void SaveRecipeState(CraftingRecipe recipe)
    {
        if (GameStateManager.Instance != null)
        {
            string key = $"forge_{forgeName}_{recipe.ItemName}_crafted";
            GameStateManager.Instance.Set(key, true);
        }
    }

    private void LoadRecipeStates()
    {
        if (GameStateManager.Instance == null) return;

        foreach (var recipe in craftingRecipes)
        {
            recipe.LoadUnlockState();
        }
    }

    #endregion

    private void OnDestroy()
    {
        // Unsubscribe from events
        foreach (var recipe in craftingRecipes)
        {
            if (recipe != null)
            {
                recipe.OnCrafted -= OnRecipeCraftedInternal;
                recipe.OnCraftingFailed -= OnRecipeCraftingFailedInternal;
            }
        }
    }
}