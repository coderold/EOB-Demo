using UnityEngine;

public class CanvasFollowCamera : MonoBehaviour
{
    private Transform _mainCameraTransform;

    void Start()
    {
        // Cache the main camera's transform to save performance
        if (Camera.main != null)
        {
            _mainCameraTransform = Camera.main.transform;
        }
    }

    void LateUpdate()
    {
        if (_mainCameraTransform)
        {
            // Force the canvas to point in the same direction as the camera
            transform.rotation = _mainCameraTransform.rotation;
        }
    }
}
