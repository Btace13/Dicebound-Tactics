using UnityEngine;

public class CombatCameraController : BaseCameraController
{
    protected override void Awake()
    {
        base.Awake();
        _cameraName = "CombatCamera";
    }

    // Additional combat-specific camera functionality can be added here
}
