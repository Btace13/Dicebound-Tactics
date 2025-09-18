using UnityEngine;

public interface ICraftable
{
    string ItemName { get; }
    string Description { get; }
    CurrencyCost CraftingCost { get; }
    bool IsUnlocked { get; }
    bool CanCraft();
    void Craft();
}

public interface IForge
{
    string ForgeName { get; }
    ICraftable[] GetAvailableRecipes();
    bool ProcessCrafting(ICraftable craftable);
    void RefreshRecipes();
}