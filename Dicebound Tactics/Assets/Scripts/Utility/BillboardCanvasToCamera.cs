using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Canvas))]
public class BillboardCanvasToCamera : MonoBehaviour
{
    Camera cam;
    public bool ShouldBillboard = true;
    public bool FlipHorizontal = false;

    static Transform flippedTransformLookAt;

    void Start()
    {
        cam = Camera.main;

        if (flippedTransformLookAt == null)
        {
            flippedTransformLookAt = new GameObject("FlippedLookAt").transform;
            DontDestroyOnLoad(flippedTransformLookAt);
        }
    }

    void LateUpdate()
    {
        if (!ShouldBillboard) return;

        if (cam == null)
            cam = Camera.main;

        if (!FlipHorizontal)
            transform.LookAt(cam.transform);
        else
        {
            flippedTransformLookAt.position = cam.transform.position + cam.transform.forward * 100;
            transform.LookAt(flippedTransformLookAt);
        }
    }
}
