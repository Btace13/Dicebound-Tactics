using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections;
using TMPro;

public class LoadingScreenHandler : MonoBehaviour
{
    [BoxGroup("Settings"), SerializeField, Range(0f, 1f)] float fadeDuration = 0.5f;
    [BoxGroup("References"), SerializeField] CanvasGroup loadingScreenCanvasGroup;
    [SerializeField, BoxGroup("References")] Camera loadingDiceRenderCamera;
    [BoxGroup("References"), SerializeField] Transform loadingDiceTransform;
    [BoxGroup("References"), SerializeField] TextMeshProUGUI loadingText;

    // Define possible face rotations for a standard d6 die
    private static readonly Quaternion[] dieFaceRotations = new Quaternion[]
    {
        Quaternion.Euler(0, 0, 0),      // Face 1
        Quaternion.Euler(0, 0, 180),    // Face 6
        Quaternion.Euler(0, 0, 90),     // Face 3
        Quaternion.Euler(0, 0, -90),    // Face 4
        Quaternion.Euler(90, 0, 0),     // Face 2
        Quaternion.Euler(-90, 0, 0),    // Face 5
    };

    private Coroutine diceFaceCoroutine;

    public void Start()
    {
        loadingScreenCanvasGroup.alpha = 0f;
        loadingScreenCanvasGroup.blocksRaycasts = false;
        loadingScreenCanvasGroup.interactable = false;
        loadingDiceRenderCamera.enabled = false; // Ensure the camera is disabled initially
        loadingDiceTransform.gameObject.SetActive(false); // Ensure the dice is hidden initially

        Debug.Log("Loading screen initialized and hidden.");
    }

    [Button("Show Loading Screen")]
    public void ShowLoadingScreen()
    {
        // Reset the loading screen state
        loadingScreenCanvasGroup.alpha = 0f;
        loadingScreenCanvasGroup.blocksRaycasts = true;
        loadingScreenCanvasGroup.interactable = true;
        loadingDiceRenderCamera.enabled = true; // Enable the camera to start rendering the dice
        loadingDiceTransform.gameObject.SetActive(true); // Show the dice

        StartCoroutine(LoadingScreenTextCoroutine());
        diceFaceCoroutine = StartCoroutine(DiceFaceCoroutine());

        // Fade in the loading screen
        loadingScreenCanvasGroup.DOFade(1f, fadeDuration).SetEase(Ease.InOutQuad);
    }

    [Button("Hide Loading Screen")]
    public void HideLoadingScreen()
    {
        loadingScreenCanvasGroup.DOFade(0f, fadeDuration).SetEase(Ease.InOutQuad).OnComplete(() =>
        {
            loadingScreenCanvasGroup.blocksRaycasts = false;
            loadingScreenCanvasGroup.interactable = false;
            loadingDiceRenderCamera.enabled = false; // Disable the camera to stop rendering the dice
            StopAllCoroutines(); // Stop the loading text coroutine
        });

        if (diceFaceCoroutine != null)
        {
            StopCoroutine(diceFaceCoroutine);
            diceFaceCoroutine = null;
        }
    }

    IEnumerator LoadingScreenTextCoroutine()
    {
        while (true)
        {
            UpdateLoadingText("Loading" + new string('.', (int)(Time.time * 2) % 4));
            yield return null; // Wait for the next frame
        }
    }

    IEnumerator DiceFaceCoroutine()
    {
        Quaternion currentRotation = loadingDiceTransform.rotation;

        while (true)
        {
            Quaternion startRot = loadingDiceTransform.rotation;
            Quaternion targetRot = dieFaceRotations[Random.Range(0, dieFaceRotations.Length)];

            if (targetRot == currentRotation)
            {
                // If the target rotation is the same as the current, skip to the next iteration
                continue;
            }

            float t = 0f;
            float duration = 0.3f;
            while (t < duration)
            {
                loadingDiceTransform.rotation = Quaternion.Slerp(startRot, targetRot, t / duration);
                t += Time.deltaTime;
                yield return null;
            }
            loadingDiceTransform.rotation = targetRot;
            yield return new WaitForSeconds(0.15f);
        }
    }

    public void UpdateLoadingText(string text)
    {
        if (loadingText != null)
        {
            loadingText.text = text;
        }
        else
        {
            Debug.LogWarning("Loading text reference is not set.");
        }
    }

    // No longer needed for face rotation, but kept for compatibility
    public void RotateDice(float rotationSpeed)
    {
        if (loadingDiceTransform != null)
        {
            // Rotate randomly like a die in all directions
            Vector3 randomAxis = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
            loadingDiceTransform.Rotate(randomAxis, rotationSpeed * Time.deltaTime);
        }
        else
        {
            Debug.LogWarning("Loading dice transform reference is not set.");
        }
    }
}