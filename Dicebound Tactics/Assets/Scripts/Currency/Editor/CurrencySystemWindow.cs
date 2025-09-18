using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class CurrencySystemWindow : EditorWindow
{
    private Vector2 scrollPosition;
    private int selectedTab = 0;
    private string[] tabNames = { "Overview", "Test Currency", "Create Pickups", "System Tools" };

    // Test values
    private CurrencyType testCurrencyType = CurrencyType.Gold;
    private int testAmount = 100;
    private Vector3 spawnPosition = Vector3.zero;

    // Currency overview
    private Dictionary<CurrencyType, int> currentCurrencies = new Dictionary<CurrencyType, int>();

    [MenuItem("Tools/Currency System Manager")]
    public static void ShowWindow()
    {
        var window = GetWindow<CurrencySystemWindow>("Currency System");
        window.Show();
    }

    private void OnEnable()
    {
        RefreshCurrencyData();
    }

    private void OnGUI()
    {
        DrawHeader();
        
        selectedTab = GUILayout.Toolbar(selectedTab, tabNames);
        
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        switch (selectedTab)
        {
            case 0:
                DrawOverviewTab();
                break;
            case 1:
                DrawTestCurrencyTab();
                break;
            case 2:
                DrawCreatePickupsTab();
                break;
            case 3:
                DrawSystemToolsTab();
                break;
        }
        
        EditorGUILayout.EndScrollView();
    }

    private void DrawHeader()
    {
        EditorGUILayout.Space();
        
        GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 18,
            alignment = TextAnchor.MiddleCenter
        };
        
        EditorGUILayout.LabelField("Currency System Manager", headerStyle);
        EditorGUILayout.Space();
        
        // Status indicator
        if (Application.isPlaying)
        {
            if (CurrencyManager.Instance != null)
            {
                EditorGUILayout.HelpBox("✓ Currency System Active", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox("⚠ Currency Manager not found in scene!", MessageType.Warning);
            }
        }
        else
        {
            EditorGUILayout.HelpBox("Enter Play Mode to test currency operations", MessageType.None);
        }
        
        EditorGUILayout.Space();
    }

    private void DrawOverviewTab()
    {
        EditorGUILayout.LabelField("System Overview", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // Currency Manager status
        var currencyManager = FindObjectOfType<CurrencyManager>();
        if (currencyManager != null)
        {
            EditorGUILayout.LabelField("Currency Manager:", "Found in scene");
            EditorGUILayout.LabelField("Location:", currencyManager.gameObject.name);
            
            if (Application.isPlaying)
            {
                RefreshCurrencyData();
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Current Currencies:", EditorStyles.boldLabel);
                
                foreach (var currency in currentCurrencies)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField($"  {currency.Key}:", GUILayout.Width(100));
                    EditorGUILayout.LabelField(currency.Value.ToString(), EditorStyles.boldLabel);
                    EditorGUILayout.EndHorizontal();
                }
            }
        }
        else
        {
            EditorGUILayout.HelpBox("No Currency Manager found in scene. Create one to use the currency system.", MessageType.Warning);
            
            if (GUILayout.Button("Create Currency Manager"))
            {
                CreateCurrencyManager();
            }
        }

        EditorGUILayout.Space();

        // Shops and Forges count
        var shops = FindObjectsOfType<Shop>();
        var forges = FindObjectsOfType<Forge>();
        var pickups = FindObjectsOfType<CurrencyPickup>();

        EditorGUILayout.LabelField("System Components:", EditorStyles.boldLabel);
        EditorGUILayout.LabelField($"  Shops: {shops.Length}");
        EditorGUILayout.LabelField($"  Forges: {forges.Length}");
        EditorGUILayout.LabelField($"  Currency Pickups: {pickups.Length}");

        if (shops.Length > 0)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Shops:", EditorStyles.miniLabel);
            foreach (var shop in shops)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"  • {shop.ShopName}", GUILayout.Width(200));
                if (GUILayout.Button("Select", GUILayout.Width(60)))
                {
                    Selection.activeObject = shop;
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        if (forges.Length > 0)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Forges:", EditorStyles.miniLabel);
            foreach (var forge in forges)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"  • {forge.ForgeName}", GUILayout.Width(200));
                if (GUILayout.Button("Select", GUILayout.Width(60)))
                {
                    Selection.activeObject = forge;
                }
                EditorGUILayout.EndHorizontal();
            }
        }
    }

    private void DrawTestCurrencyTab()
    {
        EditorGUILayout.LabelField("Test Currency Operations", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        GUI.enabled = Application.isPlaying && CurrencyManager.Instance != null;

        EditorGUILayout.BeginHorizontal();
        testCurrencyType = (CurrencyType)EditorGUILayout.EnumPopup("Currency Type:", testCurrencyType, GUILayout.Width(250));
        testAmount = EditorGUILayout.IntField("Amount:", testAmount);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Add Currency"))
        {
            CurrencyManager.Instance?.AddCurrency(testCurrencyType, testAmount);
            RefreshCurrencyData();
        }
        
        if (GUILayout.Button("Spend Currency"))
        {
            CurrencyManager.Instance?.SpendCurrency(testCurrencyType, testAmount);
            RefreshCurrencyData();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        // Quick amounts
        EditorGUILayout.LabelField("Quick Add Amounts:", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("+ 10"))
        {
            CurrencyManager.Instance?.AddCurrency(testCurrencyType, 10);
            RefreshCurrencyData();
        }
        if (GUILayout.Button("+ 100"))
        {
            CurrencyManager.Instance?.AddCurrency(testCurrencyType, 100);
            RefreshCurrencyData();
        }
        if (GUILayout.Button("+ 1000"))
        {
            CurrencyManager.Instance?.AddCurrency(testCurrencyType, 1000);
            RefreshCurrencyData();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        // Currency status
        if (Application.isPlaying && CurrencyManager.Instance != null)
        {
            RefreshCurrencyData();
            EditorGUILayout.LabelField("Current Status:", EditorStyles.boldLabel);
            foreach (var currency in currentCurrencies)
            {
                EditorGUILayout.LabelField($"{currency.Key}: {currency.Value}");
            }
        }

        GUI.enabled = true;

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Enter Play Mode to test currency operations", MessageType.Info);
        }
    }

    private void DrawCreatePickupsTab()
    {
        EditorGUILayout.LabelField("Create Currency Pickups", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // Prefab Configuration Status
        if (CurrencyConfiguration.Instance != null)
        {
            EditorGUILayout.LabelField("Prefab Configuration:", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");
            
            foreach (CurrencyType type in System.Enum.GetValues(typeof(CurrencyType)))
            {
                var prefab = CurrencyConfiguration.Instance.GetPickupPrefab(type);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"{type}:", GUILayout.Width(80));
                if (prefab != null)
                {
                    EditorGUILayout.LabelField($"✓ {prefab.name}", EditorStyles.miniLabel);
                }
                else
                {
                    EditorGUILayout.LabelField("⚠ No prefab assigned", EditorStyles.miniLabel);
                }
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }
        else
        {
            EditorGUILayout.HelpBox("No CurrencyConfiguration found! Pickups will use basic fallback.", MessageType.Warning);
        }

        testCurrencyType = (CurrencyType)EditorGUILayout.EnumPopup("Currency Type:", testCurrencyType);
        testAmount = EditorGUILayout.IntField("Amount:", testAmount);
        spawnPosition = EditorGUILayout.Vector3Field("Spawn Position:", spawnPosition);

        EditorGUILayout.Space();

        if (GUILayout.Button("Create Pickup at Position"))
        {
            CurrencyPickup.CreateCurrencyPickup(spawnPosition, testCurrencyType, testAmount);
        }

        if (GUILayout.Button("Create Pickup at Scene View Center"))
        {
            Vector3 center = SceneView.lastActiveSceneView?.camera?.transform?.position ?? Vector3.zero;
            CurrencyPickup.CreateCurrencyPickup(center, testCurrencyType, testAmount);
        }

        EditorGUILayout.Space();

        // Batch creation
        EditorGUILayout.LabelField("Batch Creation:", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Create Gold Scatter (100 total, 10 pickups)"))
        {
            CurrencyUtils.SpawnCurrencyScatter(spawnPosition, CurrencyType.Gold, 100, 10, 5f);
        }

        if (GUILayout.Button("Create Shard Scatter (25 total, 5 pickups)"))
        {
            CurrencyUtils.SpawnCurrencyScatter(spawnPosition, CurrencyType.Shards, 25, 5, 3f);
        }

        if (GUILayout.Button("Create Victory Drop (Mixed Currencies)"))
        {
            // Create a victory-style drop with both currencies
            CurrencyUtils.SpawnCurrencyScatter(spawnPosition, CurrencyType.Gold, Random.Range(50, 101), 8, 4f);
            if (Random.Range(0f, 1f) < 0.7f) // 70% chance for shards
            {
                CurrencyUtils.SpawnCurrencyScatter(spawnPosition + Vector3.right * 2f, CurrencyType.Shards, Random.Range(5, 16), 3, 2f);
            }
        }
    }

    private void DrawSystemToolsTab()
    {
        EditorGUILayout.LabelField("System Tools", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // Scene setup
        EditorGUILayout.LabelField("Scene Setup:", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Create Currency Manager"))
        {
            CreateCurrencyManager();
        }

        if (GUILayout.Button("Create Test Shop"))
        {
            CreateTestShop();
        }

        if (GUILayout.Button("Create Test Forge"))
        {
            CreateTestForge();
        }

        EditorGUILayout.Space();

        // Runtime tools
        GUI.enabled = Application.isPlaying && CurrencyManager.Instance != null;

        EditorGUILayout.LabelField("Runtime Tools:", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Reset All Currencies"))
        {
            foreach (CurrencyType type in System.Enum.GetValues(typeof(CurrencyType)))
            {
                CurrencyManager.Instance?.SetCurrency(type, 0);
            }
            RefreshCurrencyData();
        }

        if (GUILayout.Button("Max All Currencies"))
        {
            foreach (CurrencyType type in System.Enum.GetValues(typeof(CurrencyType)))
            {
                CurrencyManager.Instance?.SetCurrency(type, 9999);
            }
            RefreshCurrencyData();
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("Refresh All Shops"))
        {
            var shops = FindObjectsOfType<Shop>();
            foreach (var shop in shops)
            {
                shop.RefreshInventory();
            }
        }

        if (GUILayout.Button("Refresh All Forges"))
        {
            var forges = FindObjectsOfType<Forge>();
            foreach (var forge in forges)
            {
                forge.RefreshRecipes();
            }
        }

        GUI.enabled = true;
    }

    private void RefreshCurrencyData()
    {
        if (Application.isPlaying && CurrencyManager.Instance != null)
        {
            currentCurrencies = CurrencyManager.Instance.GetAllCurrencies();
        }
    }

    private void CreateCurrencyManager()
    {
        GameObject managerObject = new GameObject("CurrencyManager");
        managerObject.AddComponent<CurrencyManager>();
        Selection.activeObject = managerObject;
        
        EditorUtility.DisplayDialog("Currency Manager Created", 
            "Currency Manager has been created and added to the scene.", "OK");
    }

    private void CreateTestShop()
    {
        GameObject shopObject = new GameObject("Test Shop");
        var shop = shopObject.AddComponent<Shop>();
        Selection.activeObject = shopObject;
        
        EditorUtility.DisplayDialog("Test Shop Created", 
            "A test shop has been created. Configure items in the inspector.", "OK");
    }

    private void CreateTestForge()
    {
        GameObject forgeObject = new GameObject("Test Forge");
        var forge = forgeObject.AddComponent<Forge>();
        Selection.activeObject = forgeObject;
        
        EditorUtility.DisplayDialog("Test Forge Created", 
            "A test forge has been created. Configure recipes in the inspector.", "OK");
    }

    private void OnInspectorUpdate()
    {
        if (Application.isPlaying && selectedTab == 0)
        {
            Repaint();
        }
    }
}