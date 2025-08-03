using UnityEngine;

[CreateAssetMenu(fileName = "AudioConfigProfile", menuName = "Audio/Config Profile")]
public class AudioConfigProfile : ScriptableObject
{
    [Header("Menu Sounds")]
    public AudioClip menuButtonPressed;
    public AudioClip successButtonPressed;

    [Header("Combat Sounds")]
    public AudioClip combatStart;
    public AudioClip combatEnd;
    public AudioClip characterTurnStart;
    public AudioClip enemyTurnStart;
    public AudioClip gameOver;
    public AudioClip modifierApplied;
}