using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DiceboundTactics.UI
{
    /// <summary>
    /// Manages sprites for UI elements that represent input controls across different device types.
    /// </summary>
    [CreateAssetMenu(fileName = "InputIconManager", menuName = "Dicebound Tactics/UI/Input Icon Manager")]
    public class InputIconManager : ScriptableObject
    {
        [System.Serializable]
        public class InputIconSet
        {
            public string actionName;
            public Sprite keyboardSprite;
            public Sprite xboxControllerSprite;
            public Sprite playstationControllerSprite;
            public Sprite switchControllerSprite;
        }

        [SerializeField] private List<InputIconSet> inputIcons = new List<InputIconSet>();

        private Dictionary<string, InputIconSet> iconLookup;

        private void OnEnable()
        {
            // Build lookup dictionary for faster access
            iconLookup = new Dictionary<string, InputIconSet>();
            foreach (var iconSet in inputIcons)
            {
                if (!string.IsNullOrEmpty(iconSet.actionName))
                {
                    iconLookup[iconSet.actionName] = iconSet;
                }
            }
        }

        /// <summary>
        /// Gets the appropriate sprite for the input action based on the current control scheme
        /// </summary>
        /// <param name="actionName">The name of the input action</param>
        /// <returns>The sprite for the current device, or null if not found</returns>
        public Sprite GetIconForAction(string actionName)
        {
            if (iconLookup == null) OnEnable();

            if (!iconLookup.TryGetValue(actionName, out var iconSet))
                return null;

            var currentDevice = GetCurrentControlScheme();
            return GetSpriteForDevice(iconSet, currentDevice);
        }

        /// <summary>
        /// Gets the sprite for a specific device type for the input action
        /// </summary>
        public Sprite GetIconForAction(string actionName, string deviceType)
        {
            if (iconLookup == null) OnEnable();

            if (!iconLookup.TryGetValue(actionName, out var iconSet))
                return null;

            return GetSpriteForDevice(iconSet, deviceType);
        }

        private Sprite GetSpriteForDevice(InputIconSet iconSet, string deviceType)
        {
            return deviceType.ToLowerInvariant() switch
            {
                "keyboard" or "keyboard&mouse" => iconSet.keyboardSprite,
                "gamepad" or "xbox" => iconSet.xboxControllerSprite,
                "playstation" or "ps4" or "ps5" => iconSet.playstationControllerSprite,
                "switch" => iconSet.switchControllerSprite,
                _ => iconSet.keyboardSprite // Default to keyboard
            };
        }

        private string GetCurrentControlScheme()
        {
            if (Gamepad.current != null)
            {
                var gamepad = Gamepad.current;

                // Detect gamepad type based on available information
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
                if (gamepad.name.ToLowerInvariant().Contains("xbox"))
                    return "xbox";
                else if (gamepad.name.ToLowerInvariant().Contains("ps"))
                    return "playstation";
                else if (gamepad.name.ToLowerInvariant().Contains("switch") || gamepad.name.ToLowerInvariant().Contains("nintendo"))
                    return "switch";
                else
                    return "gamepad";
#elif UNITY_PS4 || UNITY_PS5
                return "playstation";
#elif UNITY_SWITCH
                return "switch";
#elif UNITY_XBOX
                return "xbox";
#else
                return "gamepad";
#endif
            }

            return "keyboard";
        }
    }
}
