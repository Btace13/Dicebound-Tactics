using UnityEngine;
using System.Collections.Generic;
using TacticsToolkit;
using Sirenix.OdinInspector;

public class PartyManager : MonoBehaviour
{
    public static PartyManager Instance { get; private set; }

    [BoxGroup("Party Settings"), SerializeField] private List<CharacterManager> PartyMembers = new List<CharacterManager>();
    [BoxGroup("Party Settings"), SerializeField] private int maxPartySize = 4;

    public CharacterManager PartyLeader => PartyMembers.Count > 0 ? PartyMembers[0] : null;

    [BoxGroup("Events")] public GameEventCharacterManager OnPartyMemberAdded;
    [BoxGroup("Events")] public GameEventCharacterManager OnPartyMemberRemoved;
    [BoxGroup("Events")] public GameEventCharacterManager OnPartyLeaderChanged;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void InitializeParty(List<CharacterManager> initialMembers)
    {
        if (initialMembers == null || initialMembers.Count == 0)
        {
            Debug.LogWarning("No initial party members provided. Starting with an empty party.");
            return;
        }

        foreach (var member in initialMembers)
        {
            AddPartyMember(member);
        }
    }

    public List<CharacterManager> GetPartyMembers()
    {
        return PartyMembers;
    }

    public CharacterManager SwitchPartyLeader(CharacterManager newLeader)
    {
        if (PartyMembers.Contains(newLeader))
        {
            CharacterManager oldLeader = PartyMembers[0];
            PartyMembers.Remove(newLeader);
            PartyMembers.Insert(0, newLeader);

            Debug.Log($"Switched party leader from {oldLeader.name} to {newLeader.name}.");

            OnPartyLeaderChanged?.Raise(newLeader);

            return oldLeader;
        }
        else
        {
            Debug.LogWarning($"{newLeader.name} is not in the party. Cannot switch leader.");
            return null;
        }
    }

    public void AddPartyMember(CharacterManager character)
    {
        if (PartyMembers.Count < maxPartySize && !PartyMembers.Contains(character))
        {
            PartyMembers.Add(character);
            OnPartyMemberAdded?.Raise(character);
            Debug.Log($"Added {character.name} to the party.");
        }
        else
        {
            Debug.LogWarning($"Cannot add {character.name} to the party. Party is full or already contains this member.");
        }
    }

    public void RemovePartyMember(CharacterManager character)
    {
        if (PartyMembers.Contains(character))
        {
            PartyMembers.Remove(character);

            OnPartyMemberRemoved?.Raise(character);
            Debug.Log($"Removed {character.name} from the party.");
        }
        else
        {
            Debug.LogWarning($"Cannot remove {character.name} from the party. Character not found in the party.");
        }
    }

    public void OnGameStateChanged(GameState newState)
    {
        // Ensure the party leader's movement changes based on the game state
        PartyLeader?.GetComponent<OverworldCharacterController>().OnGameStateChanged(newState);
    }
}
