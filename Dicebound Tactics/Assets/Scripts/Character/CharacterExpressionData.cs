using UnityEngine;

[CreateAssetMenu(fileName = "CharacterExpressionData", menuName = "Dice/Character Expression Data")]
public class CharacterExpressionData : ScriptableObject
{
    [System.Serializable]
    public class EyeSettings
    {
        public Color irisColor = Color.blue;
        public Vector2 pupilSize = new Vector2(0.15f, 0.15f);
        public float blinkIntervalMin = 2f;
        public float blinkIntervalMax = 4f;
        public float topEyelashThickness = 0.1f;
        public float bottomEyelashThickness = 0.05f;
    }

    public EyeSettings eyeSettings;
    public Sprite eyebrowSprite;
    public Sprite mouthSprite;
}
