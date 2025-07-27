using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

public class CharacterExpressionHandler : MonoBehaviour
{
    [BoxGroup("References"), SerializeField] Image leftEyebrowImage;
    [BoxGroup("References"), SerializeField] Image rightEyebrowImage;
    [BoxGroup("References"), SerializeField] Image mouthImage;
    [BoxGroup("References"), SerializeField] Image leftEyeImage;
    [BoxGroup("References"), SerializeField] Image rightEyeImage;

    public CharacterExpressionData expressionData;

    private Material _leftEyeMaterial;
    private Material _rightEyeMaterial;
    private MaterialPropertyBlock _leftEyePropertyBlock;
    private MaterialPropertyBlock _rightEyePropertyBlock;

    private void Awake()
    {
        if (expressionData == null)
        {
            Debug.LogError("Expression data is not assigned in CharacterExpressionHandler.");
            return;
        }

        _leftEyeMaterial = leftEyeImage.material;
        _rightEyeMaterial = rightEyeImage.material;

        _leftEyePropertyBlock = new MaterialPropertyBlock();
        _rightEyePropertyBlock = new MaterialPropertyBlock();

        ApplyExpressionData();
    }

    private void ApplyExpressionData()
    {
        // Apply the expression data to the character's visual elements

        // - Set the eyebrow sprite
        leftEyebrowImage.sprite = expressionData.eyebrowSprite;
        rightEyebrowImage.sprite = expressionData.eyebrowSprite;

        // - Set the mouth sprite
        mouthImage.sprite = expressionData.mouthSprite;

        // - Adjust eye settings
        if (expressionData.eyeSettings != null)
        {
            // Set iris color
            _leftEyePropertyBlock.SetColor("_IrisColor", expressionData.eyeSettings.irisColor);
            _rightEyePropertyBlock.SetColor("_IrisColor", expressionData.eyeSettings.irisColor);

            // Set pupil size
            _leftEyePropertyBlock.SetVector("_PupilSize", new Vector4(expressionData.eyeSettings.pupilSize.x, expressionData.eyeSettings.pupilSize.y, 0, 0));
            _rightEyePropertyBlock.SetVector("_PupilSize", new Vector4(expressionData.eyeSettings.pupilSize.x, expressionData.eyeSettings.pupilSize.y, 0, 0));

            // Set blink speed
            _leftEyePropertyBlock.SetFloat("_BlinkSpeed", expressionData.eyeSettings.blinkSpeed);
            _rightEyePropertyBlock.SetFloat("_BlinkSpeed", expressionData.eyeSettings.blinkSpeed);

            // Set eyelash thickness
            _leftEyePropertyBlock.SetFloat("_TopEyelashThickness", expressionData.eyeSettings.topEyelashThickness);
            _leftEyePropertyBlock.SetFloat("_BottomEyelashThickness", expressionData.eyeSettings.bottomEyelashThickness);
            _rightEyePropertyBlock.SetFloat("_TopEyelashThickness", expressionData.eyeSettings.topEyelashThickness);
            _rightEyePropertyBlock.SetFloat("_BottomEyelashThickness", expressionData.eyeSettings.bottomEyelashThickness);
        }
    }
}
