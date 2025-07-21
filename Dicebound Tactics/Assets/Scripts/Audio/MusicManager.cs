using UnityEngine;
using System.Collections;
using UnityEngine.Audio;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public float fadeDuration = 1.5f;

    // TEMP
    [SerializeField] private AudioClip defaultMusicClip;

    private Coroutine fadeRoutine;

    private void Awake()
    {
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Event listeners
        EventManager.OnCombatEncounterStarted += OnCombatEncounterStarted;
        EventManager.OnCombatEncounterEnded += OnCombatEncounterEnded;
    }

    private void OnDisable()
    {
        EventManager.OnCombatEncounterStarted -= OnCombatEncounterStarted;
        EventManager.OnCombatEncounterEnded -= OnCombatEncounterEnded;
    }

    // TEMP
    private void Start()
    {
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
        }

        // Start with default music
        PlayMusic(defaultMusicClip);
    }

    /// <summary>
    /// Plays a new music clip, fading out the current one if needed.
    /// </summary>
    public void PlayMusic(AudioClip newClip)
    {
        if (musicSource.clip == newClip)
            return;

        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(FadeToNewClip(newClip));
    }

    private IEnumerator FadeToNewClip(AudioClip newClip)
    {
        // Fade out current music
        float startVolume = musicSource.volume;
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            musicSource.volume = Mathf.Lerp(startVolume, 0f, t / fadeDuration);
            yield return null;
        }

        musicSource.volume = 0f;
        musicSource.Stop();

        // Switch clip
        musicSource.clip = newClip;
        musicSource.Play();

        // Fade in new music
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            musicSource.volume = Mathf.Lerp(0f, startVolume, t / fadeDuration);
            yield return null;
        }

        musicSource.volume = startVolume;
        fadeRoutine = null;
    }

    private void OnCombatEncounterStarted(CombatEncounter encounter)
    {
        // Play combat music
        if (encounter.encounterMusic != null)
        {
            PlayMusic(encounter.encounterMusic);
        }
        else
        {
            PlayMusic(defaultMusicClip); // Fallback to default music
        }
    }

    private void OnCombatEncounterEnded(CombatEncounter encounter)
    {
        // Stop combat music and fade back to default
        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(FadeToNewClip(defaultMusicClip));
    }
}
