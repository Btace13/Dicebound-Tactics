using UnityEngine;

[CreateAssetMenu(fileName = "CharacterExpressionData", menuName = "Dice/Character Expression Data")]
public class CharacterExpressionData : ScriptableObject
{
    public class EyeSettings
    {
        public Color irisColor = Color.blue;
        public Vector2 pupilSize = new Vector2(0.15f, 0.15f);
        public float blinkSpeed = 0.5f;
        public float topEyelashThickness = 0.1f;
        public float bottomEyelashThickness = 0.05f;
    }

    public EyeSettings eyeSettings;
    public Sprite eyebrowSprite;
    public Sprite mouthSprite;
}
