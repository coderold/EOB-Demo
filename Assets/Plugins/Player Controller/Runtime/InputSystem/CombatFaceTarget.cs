using UnityEngine;
using StarterAssets;

public class CombatFaceTarget : MonoBehaviour
{
    private ThirdPersonController _controller;
    private Camera _mainCamera;

    void Start()
    {
        _controller = GetComponent<ThirdPersonController>();
        _mainCamera = Camera.main;
    }

    // Call this method at the start of your Attack animation/logic
    public void FaceCenter()
    {
        // 1. Find the center of the screen
        Vector3 screenCenter = new Vector3(Screen.width / 2, Screen.height / 2, 0);
        Ray ray = _mainCamera.ScreenPointToRay(screenCenter);
        
        Vector3 targetPosition;

        // 2. Raycast to find what the crosshair is pointing at
        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            targetPosition = hit.point;
        }
        else
        {
            targetPosition = ray.GetPoint(100f);
        }

        // 3. Calculate rotation (ignore Y to prevent character tilting up/down)
        Vector3 lookDir = targetPosition - transform.position;
        lookDir.y = 0; 

        if (lookDir != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDir);
            transform.rotation = targetRotation;
        }
    }
}