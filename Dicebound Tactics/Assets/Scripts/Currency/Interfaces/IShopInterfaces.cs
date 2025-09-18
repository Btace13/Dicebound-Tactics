using UnityEngine;

public interface IPurchasable
{
    string ItemName { get; }
    string Description { get; }
    CurrencyCost Cost { get; }
    bool IsAvailable { get; }
    bool CanPurchase();
    void Purchase();
}

public interface IShop
{
    string ShopName { get; }
    IPurchasable[] GetAvailableItems();
    bool ProcessPurchase(IPurchasable item);
    void RefreshInventory();
}

public interface ISellable
{
    string ItemName { get; }
    CurrencyCost SellValue { get; }
    bool CanSell();
    void Sell();
}