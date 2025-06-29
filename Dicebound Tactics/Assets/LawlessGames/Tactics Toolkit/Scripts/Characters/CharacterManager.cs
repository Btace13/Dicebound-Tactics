using UnityEngine;

namespace TacticsToolkit
{
    [RequireComponent(typeof(OverworldCharacterController))]
    //Script for a playable character.
    public class CharacterManager : Entity, IOverworldControllable
    {
        public bool IsControllable { get; set; } = true;
        public bool IsControlled { get; set; } = false;
        public OverworldCharacterController OverworldCharacterController { get; set; }

        private void Awake()
        {
            OverworldCharacterController = GetComponent<OverworldCharacterController>();

            if (OverworldCharacterController == null)
            {
                Debug.LogError("OverworldCharacterController component is missing on the CharacterManager.");
            }
        }
    }
}