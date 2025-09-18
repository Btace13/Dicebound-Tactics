using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class CurrencySystemValidator : EditorWindow
{
    private Vector2 scrollPosition;
    private List<ValidationResult> validationResults = new List<ValidationResult>();

    [MenuItem("Tools/Currency System Validator")]
    public static void ShowWindow()
    {
        var window = GetWindow<CurrencySystemValidator>("Currency Validator");
        window.Show();
        window.RunValidation();
    }

    private void OnGUI()
    {
        EditorGUILayout.Space();
        
        GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 16,
            alignment = TextAnchor.MiddleCenter
        };
        
        EditorGUILayout.LabelField("Currency System Validator", headerStyle);
        EditorGUILayout.Space();

        if (GUILayout.Button("Run Validation", GUILayout.Height(25)))
        {
            RunValidation();
        }

        EditorGUILayout.Space();

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        foreach (var result in validationResults)
        {
            DrawValidationResult(result);
        }

        EditorGUILayout.EndScrollView();
    }

    private void DrawValidationResult(ValidationResult result)
    {
        MessageType messageType = MessageType.Info;
        switch (result.severity)
        {
            case ValidationSeverity.Error:
                messageType = MessageType.Error;
                break;
            case ValidationSeverity.Warning:
                messageType = MessageType.Warning;
                break;
            case ValidationSeverity.Info:
                messageType = MessageType.Info;
                break;
        }

        EditorGUILayout.BeginVertical();
        EditorGUILayout.HelpBox(result.message, messageType);

        if (!string.IsNullOrEmpty(result.suggestion))
        {
            EditorGUILayout.LabelField("Suggestion:", EditorStyles.miniLabel);
            EditorGUILayout.LabelField(result.suggestion, EditorStyles.wordWrappedMiniLabel);
        }

        if (result.target != null)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Select Object", GUILayout.Width(100)))
            {
                Selection.activeObject = result.target;
            }
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.Space();
        EditorGUILayout.EndVertical();
    }

    private void RunValidation()
    {
        validationResults.Clear();

        ValidateCurrencyManager();
        ValidateShops();
        ValidateForges();
        ValidateCurrencyPickups();
        ValidateUI();
        ValidateSaveSystem();

        Repaint();
    }

    private void ValidateCurrencyManager()
    {
        var currencyManager = FindObjectOfType<CurrencyManager>();
        
        if (currencyManager == null)
        {
            validationResults.Add(new ValidationResult
            {
                severity = ValidationSeverity.Error,
                message = "No CurrencyManager found in scene!",
                suggestion = "Create a CurrencyManager using Tools > Currency System Manager > Overview > Create Currency Manager"
            });
            return;
        }

        validationResults.Add(new ValidationResult
        {
            severity = ValidationSeverity.Info,
            message = "✓ CurrencyManager found in scene",
            target = currencyManager
        });

        // Check if it's set to DontDestroyOnLoad
        if (currencyManager.gameObject.scene.name != "DontDestroyOnLoad")
        {
            validationResults.Add(new ValidationResult
            {
                severity = ValidationSeverity.Warning,
                message = "CurrencyManager should persist across scenes",
                suggestion = "The CurrencyManager will automatically set itself to DontDestroyOnLoad when the game starts",
                target = currencyManager
            });
        }
    }

    private void ValidateShops()
    {
        var shops = FindObjectsOfType<Shop>();
        
        if (shops.Length == 0)
        {
            validationResults.Add(new ValidationResult
            {
                severity = ValidationSeverity.Info,
                message = "No shops found in scene",
                suggestion = "Create shops using Tools > Currency System Manager > System Tools > Create Test Shop"
            });
            return;
        }

        validationResults.Add(new ValidationResult
        {
            severity = ValidationSeverity.Info,
            message = $"✓ Found {shops.Length} shop(s) in scene"
        });

        foreach (var shop in shops)
        {
            var items = shop.GetAllItems();
            if (items.Length == 0)
            {
                validationResults.Add(new ValidationResult
                {
                    severity = ValidationSeverity.Warning,
                    message = $"Shop '{shop.ShopName}' has no items configured",
                    suggestion = "Add shop items in the inspector",
                    target = shop
                });
            }
        }
    }

    private void ValidateForges()
    {
        var forges = FindObjectsOfType<Forge>();
        
        if (forges.Length == 0)
        {
            validationResults.Add(new ValidationResult
            {
                severity = ValidationSeverity.Info,
                message = "No forges found in scene",
                suggestion = "Create forges using Tools > Currency System Manager > System Tools > Create Test Forge"
            });
            return;
        }

        validationResults.Add(new ValidationResult
        {
            severity = ValidationSeverity.Info,
            message = $"✓ Found {forges.Length} forge(s) in scene"
        });

        foreach (var forge in forges)
        {
            var recipes = forge.GetAllRecipes();
            if (recipes.Length == 0)
            {
                validationResults.Add(new ValidationResult
                {
                    severity = ValidationSeverity.Warning,
                    message = $"Forge '{forge.ForgeName}' has no recipes configured",
                    suggestion = "Add crafting recipes in the inspector",
                    target = forge
                });
            }
        }
    }

    private void ValidateCurrencyPickups()
    {
        var pickups = FindObjectsOfType<CurrencyPickup>();
        
        validationResults.Add(new ValidationResult
        {
            severity = ValidationSeverity.Info,
            message = $"Found {pickups.Length} currency pickup(s) in scene"
        });

        foreach (var pickup in pickups)
        {
            var collider = pickup.GetComponent<Collider>();
            if (collider == null)
            {
                validationResults.Add(new ValidationResult
                {
                    severity = ValidationSeverity.Error,
                    message = $"Currency pickup missing collider component",
                    suggestion = "Add a Collider component and set it as a trigger",
                    target = pickup
                });
            }
            else if (!collider.isTrigger)
            {
                validationResults.Add(new ValidationResult
                {
                    severity = ValidationSeverity.Warning,
                    message = $"Currency pickup collider should be set as trigger",
                    suggestion = "Enable 'Is Trigger' on the collider component",
                    target = pickup
                });
            }
        }
    }

    private void ValidateUI()
    {
        var currencyDisplays = FindObjectsOfType<CurrencyDisplay>();
        var currencyPanels = FindObjectsOfType<CurrencyPanel>();

        if (currencyDisplays.Length == 0 && currencyPanels.Length == 0)
        {
            validationResults.Add(new ValidationResult
            {
                severity = ValidationSeverity.Warning,
                message = "No currency UI components found",
                suggestion = "Create CurrencyDisplay or CurrencyPanel components to show currency amounts to players"
            });
        }
        else
        {
            validationResults.Add(new ValidationResult
            {
                severity = ValidationSeverity.Info,
                message = $"✓ Found {currencyDisplays.Length} currency display(s) and {currencyPanels.Length} currency panel(s)"
            });
        }
    }

    private void ValidateSaveSystem()
    {
        var saveManager = FindObjectOfType<SaveManager>();
        
        if (saveManager == null)
        {
            validationResults.Add(new ValidationResult
            {
                severity = ValidationSeverity.Warning,
                message = "No SaveManager found in scene",
                suggestion = "Currency progress will not be saved between sessions without a SaveManager"
            });
        }
        else
        {
            validationResults.Add(new ValidationResult
            {
                severity = ValidationSeverity.Info,
                message = "✓ SaveManager found - currency will be persisted",
                target = saveManager
            });
        }
    }

    private class ValidationResult
    {
        public ValidationSeverity severity;
        public string message;
        public string suggestion;
        public Object target;
    }

    private enum ValidationSeverity
    {
        Info,
        Warning,
        Error
    }
}