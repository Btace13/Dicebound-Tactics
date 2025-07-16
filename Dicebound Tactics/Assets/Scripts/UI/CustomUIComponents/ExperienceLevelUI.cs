using System.Threading.Tasks;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Sirenix.OdinInspector;

public class ExperienceLevelUI : MonoBehaviour
{
    [BoxGroup("Values"), SerializeField] private int experienceLevel = 1; // Default level
    [BoxGroup("Values"), SerializeField] private int currentExperiencePoints = 0; // Current points
    [BoxGroup("Values"), SerializeField] private int pointsToNextLevel = 100; // Points required to reach the next level

    public int ExperienceLevel
    {
        get => experienceLevel;
        set
        {
            experienceLevel = value;
            // Optionally, update the UI or perform other actions when the level changes
        }
    }
    public int PointsToNextLevel
    {
        get => pointsToNextLevel;
        set
        {
            pointsToNextLevel = value;
            // Optionally, update the UI or perform other actions when the points change
        }
    }
    public int CurrentExperiencePoints
    {
        get => currentExperiencePoints;
        set
        {
            currentExperiencePoints = value;
            // Optionally, update the UI or perform other actions when the points change
        }
    }

    [BoxGroup("References"), SerializeField] private TextMeshProUGUI experienceLevelText;
    [BoxGroup("References"), SerializeField] private Slider experienceSlider;

    [BoxGroup("Events"), SerializeField] public UnityEvent<int> OnLevelUp; // Event to notify when the player levels up

    public async void AddExperience(int points, bool shouldAnimate = true)
    {
        if (points <= 0) return;

        while (points > 0)
        {
            // Add points to current experience
            currentExperiencePoints++;

            // Check if we have enough points to level up
            if (currentExperiencePoints >= pointsToNextLevel)
            {
                LevelUp();
            }

            // Optionally, update the UI or perform other actions when experience is added
            if (shouldAnimate)
            {
                UpdateUI(); // Update the UI to reflect the new experience level and points

                // Simulate a delay for the animation effect
                await Task.Yield(); // Yield to allow other processes to run
            }

            points -= 1; // Decrement points to simulate adding experience over time
        }
    }

    public void LevelUp()
    {
        // Set the new level and reset experience points
        ExperienceLevel++;
        currentExperiencePoints = 0; // Reset points after leveling up

        OnLevelUp?.Invoke(ExperienceLevel); // Invoke the level-up event
    }

    private void UpdateUI()
    {
        if (experienceLevelText != null)
        {
            experienceLevelText.text = $"Level: {ExperienceLevel}";
        }

        if (experienceSlider != null)
        {
            experienceSlider.maxValue = PointsToNextLevel;
            experienceSlider.value = (float)CurrentExperiencePoints;
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        UpdateUI();
    }
#endif
}
