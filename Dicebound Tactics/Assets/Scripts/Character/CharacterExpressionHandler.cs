using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using DG.Tweening;

public class CharacterExpressionHandler : MonoBehaviour
{
    [BoxGroup("References"), SerializeField] MeshRenderer leftEyebrowMeshRenderer;
    [BoxGroup("References"), SerializeField] MeshRenderer rightEyebrowMeshRenderer;
    [BoxGroup("References"), SerializeField] MeshRenderer mouthMeshRenderer;
    [BoxGroup("References"), SerializeField] MeshRenderer leftEyeMeshRenderer;
    [BoxGroup("References"), SerializeField] MeshRenderer rightEyeMeshRenderer;

    public CharacterExpressionData expressionData;

    private Material _leftEyeMaterial;
    private Material _rightEyeMaterial;
    private MaterialPropertyBlock _leftEyePropertyBlock;
    private MaterialPropertyBlock _rightEyePropertyBlock;

    private float _blinkTimer = 0f;
    private float _blinkIntervalMin => expressionData?.eyeSettings?.blinkIntervalMin ?? 2f;
    private float _blinkIntervalMax => expressionData?.eyeSettings?.blinkIntervalMax ?? 4f;
    private float _nextBlinkTime = 0f;

    private void Awake()
    {
        if (expressionData == null)
        {
            Debug.LogError("Expression data is not assigned in CharacterExpressionHandler.");
            return;
        }

        _leftEyeMaterial = leftEyeMeshRenderer.material;
        _rightEyeMaterial = rightEyeMeshRenderer.material;

        _leftEyePropertyBlock = new MaterialPropertyBlock();
        _rightEyePropertyBlock = new MaterialPropertyBlock();

        ApplyExpressionData();
        SetNextBlinkTime();
    }

    private void ApplyExpressionData()
    {
        // Apply the expression data to the character's visual elements

        // - Set the eyebrow sprite
        _leftEyePropertyBlock.SetTexture("_MainTex", expressionData.eyebrowSprite.texture);
        _rightEyePropertyBlock.SetTexture("_MainTex", expressionData.eyebrowSprite.texture);

        // - Set the mouth sprite
        if (mouthMeshRenderer != null && expressionData.mouthSprite != null)
        {
            mouthMeshRenderer.material.SetTexture("_MainTex", expressionData.mouthSprite.texture);
        }

        // - Adjust eye settings
        if (expressionData.eyeSettings != null)
        {
            // Set iris color
            _leftEyePropertyBlock.SetColor("_IrisColor", expressionData.eyeSettings.irisColor);
            _rightEyePropertyBlock.SetColor("_IrisColor", expressionData.eyeSettings.irisColor);

            // Set pupil size
            _leftEyePropertyBlock.SetVector("_PupilSize", new Vector4(expressionData.eyeSettings.pupilSize.x, expressionData.eyeSettings.pupilSize.y, 0, 0));
            _rightEyePropertyBlock.SetVector("_PupilSize", new Vector4(expressionData.eyeSettings.pupilSize.x, expressionData.eyeSettings.pupilSize.y, 0, 0));

            // Blink speed is now randomized per interval, so no direct property to set here.

            // Set eyelash thickness
            _leftEyePropertyBlock.SetFloat("_TopEyelashThickness", expressionData.eyeSettings.topEyelashThickness);
            _leftEyePropertyBlock.SetFloat("_BottomEyelashThickness", expressionData.eyeSettings.bottomEyelashThickness);
            _rightEyePropertyBlock.SetFloat("_TopEyelashThickness", expressionData.eyeSettings.topEyelashThickness);
            _rightEyePropertyBlock.SetFloat("_BottomEyelashThickness", expressionData.eyeSettings.bottomEyelashThickness);
        }
    }

    private void LateUpdate()
    {
        // Handle blinking logic
        _blinkTimer += Time.deltaTime;
        if (_blinkTimer >= _nextBlinkTime)
        {
            BlinkEyes();
            _blinkTimer = 0f;
            SetNextBlinkTime();
        }
    }

    private void SetNextBlinkTime()
    {
        _nextBlinkTime = Random.Range(_blinkIntervalMin, _blinkIntervalMax);
    }

    private void BlinkEyes()
    {
        string topEyelidControl = "_TopEyelidControl";
        float topEyelidXValue = 0.5f; // Adjust this value to control the eyelid position
        float topEyelidYValue = 0.8f; // Adjust this value to control the eyelid position

        // Blink animation for both eyes
        _leftEyeMaterial.SetVector(topEyelidControl, new Vector2(topEyelidXValue, topEyelidYValue));
        _rightEyeMaterial.SetVector(topEyelidControl, new Vector2(topEyelidXValue, topEyelidYValue));

        DOTween.To(() => topEyelidYValue, x => topEyelidYValue = x, 0.15f, Random.Range(0.1f, 0.2f))
            .OnUpdate(() =>
            {
                _leftEyeMaterial.SetVector(topEyelidControl, new Vector2(topEyelidXValue, topEyelidYValue));
                _rightEyeMaterial.SetVector(topEyelidControl, new Vector2(topEyelidXValue, topEyelidYValue));
            })
            .SetLoops(2, LoopType.Yoyo);
    }
}
