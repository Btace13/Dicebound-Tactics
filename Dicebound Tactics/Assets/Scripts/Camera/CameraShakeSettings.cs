using UnityEngine;

[CreateAssetMenu(fileName = "CameraShakeSettings", menuName = "ScriptableObjects/CameraShakeSettings", order = 1)]
public class CameraShakeSettings : ScriptableObject
{
    [Header("Shake Settings")]
    [SerializeField, Range(0f, 5f)] private float intensity = 0.5f;
    [SerializeField, Range(0f, 5f)] private float frequency = 1f;
    [SerializeField] private float duration = 0.3f;
    [SerializeField] private AnimationCurve shakeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    public float Intensity => intensity;
    public float Frequency => frequency;
    public float Duration => duration;
    public AnimationCurve ShakeCurve => shakeCurve;
}
