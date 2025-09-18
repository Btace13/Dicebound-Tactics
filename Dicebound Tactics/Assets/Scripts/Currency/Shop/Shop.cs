using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;

public class Shop : MonoBehaviour, IShop
{
    [Header("Shop Information")]
    [SerializeField] private string shopName = "General Store";
    [SerializeField] private string shopDescription = "Buy and sell various items";

    [Header("Shop Items")]
    [SerializeField] private List<ShopItem> shopItems = new List<ShopItem>();

    [Header("Shop Settings")]
    [SerializeField] private bool autoRefreshOnStart = true;
    [SerializeField] private float refreshInterval = 0f; // 0 for no auto refresh

    [Header("Audio")]
    [SerializeField] private AudioClip purchaseSuccessSound;
    [SerializeField] private AudioClip purchaseFailedSound;
    [SerializeField] private AudioClip shopOpenSound;

    private Dictionary<ShopItem, bool> purchaseStates = new Dictionary<ShopItem, bool>();

    // Events
    public System.Action<Shop> OnShopOpened;
    public System.Action<Shop> OnShopClosed;
    public System.Action<ShopItem> OnItemPurchased;
    public System.Action<ShopItem> OnPurchaseFailed;

    public string ShopName => shopName;
    public string ShopDescription => shopDescription;

    private void Start()
    {
        InitializeShop();
        
        if (autoRefreshOnStart)
        {
            RefreshInventory();
        }

        // Set up auto refresh if interval is set
        if (refreshInterval > 0)
        {
            InvokeRepeating(nameof(RefreshInventory), refreshInterval, refreshInterval);
        }
    }

    private void InitializeShop()
    {
        // Subscribe to item events
        foreach (var item in shopItems)
        {
            item.OnPurchased += OnItemPurchasedInternal;
            item.OnPurchaseFailed += OnItemPurchaseFailedInternal;
        }
    }

    public IPurchasable[] GetAvailableItems()
    {
        return shopItems.Where(item => item.IsAvailable).Cast<IPurchasable>().ToArray();
    }

    public ShopItem[] GetAllItems()
    {
        return shopItems.ToArray();
    }

    public bool ProcessPurchase(IPurchasable item)
    {
        if (item is ShopItem shopItem && shopItems.Contains(shopItem))
        {
            if (shopItem.CanPurchase())
            {
                shopItem.Purchase();
                return true;
            }
        }
        return false;
    }

    public void RefreshInventory()
    {
        // Load purchase states from save system
        LoadPurchaseStates();

        // Refresh any time-based availability or stock
        foreach (var item in shopItems)
        {
            // You can add logic here to refresh stock, change prices, etc.
        }

        Debug.Log($"{shopName} inventory refreshed");
    }

    private void OnItemPurchasedInternal(ShopItem item)
    {
        PlayAudio(purchaseSuccessSound);
        SavePurchaseState(item);
        OnItemPurchased?.Invoke(item);
    }

    private void OnItemPurchaseFailedInternal(ShopItem item)
    {
        PlayAudio(purchaseFailedSound);
        OnPurchaseFailed?.Invoke(item);
    }

    public void OpenShop()
    {
        PlayAudio(shopOpenSound);
        RefreshInventory();
        OnShopOpened?.Invoke(this);
    }

    public void CloseShop()
    {
        OnShopClosed?.Invoke(this);
    }

    private void PlayAudio(AudioClip clip)
    {
        if (clip != null)
        {
            // Create temporary audio source for one-shot audio
            GameObject audioObject = new GameObject("ShopAudio");
            AudioSource audioSource = audioObject.AddComponent<AudioSource>();
            audioSource.clip = clip;
            audioSource.Play();
            Destroy(audioObject, clip.length);
        }
    }

    #region Shop Management

    [Button("Add Test Items")]
    private void AddTestItems()
    {
        if (shopItems == null) shopItems = new List<ShopItem>();

        // Add some test items
        var goldItem = new ShopItem();
        // Note: Since ShopItem fields are private, we'd need to make them public or add setters
        // For now, this is just a structure example
        shopItems.Add(goldItem);
    }

    public void AddItem(ShopItem item)
    {
        if (!shopItems.Contains(item))
        {
            shopItems.Add(item);
            item.OnPurchased += OnItemPurchasedInternal;
            item.OnPurchaseFailed += OnItemPurchaseFailedInternal;
        }
    }

    public void RemoveItem(ShopItem item)
    {
        if (shopItems.Contains(item))
        {
            shopItems.Remove(item);
            item.OnPurchased -= OnItemPurchasedInternal;
            item.OnPurchaseFailed -= OnItemPurchaseFailedInternal;
        }
    }

    public ShopItem GetItemByName(string itemName)
    {
        return shopItems.FirstOrDefault(item => item.ItemName == itemName);
    }

    #endregion

    #region Save/Load

    private void SavePurchaseState(ShopItem item)
    {
        if (GameStateManager.Instance != null)
        {
            string key = $"shop_{shopName}_{item.ItemName}_purchased";
            GameStateManager.Instance.Set(key, item.HasBeenPurchased);
        }
    }

    private void LoadPurchaseStates()
    {
        if (GameStateManager.Instance == null) return;

        foreach (var item in shopItems)
        {
            string key = $"shop_{shopName}_{item.ItemName}_purchased";
            bool purchased = GameStateManager.Instance.Get(key);
            item.LoadPurchaseState(purchased);
        }
    }

    #endregion

    private void OnDestroy()
    {
        // Unsubscribe from events
        foreach (var item in shopItems)
        {
            if (item != null)
            {
                item.OnPurchased -= OnItemPurchasedInternal;
                item.OnPurchaseFailed -= OnItemPurchaseFailedInternal;
            }
        }
    }
}