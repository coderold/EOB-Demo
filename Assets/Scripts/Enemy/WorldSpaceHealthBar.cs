using UnityEngine;
using UnityEngine.UI;

public class WorldSpaceHealthBar : MonoBehaviour
{
    [Header("UI Reference")]
    [SerializeField] private Image healthBarFill;

    private Transform _mainCameraTransform;

    private void Start()
    {
        // Cache the camera transform for efficiency
        if (Camera.main != null)
        {
            _mainCameraTransform = Camera.main.transform;
        }
    }

    private void LateUpdate()
    {
        if (_mainCameraTransform == null) return;

        // Force the canvas plane to lock completely flat facing the camera lens vector
        transform.forward = _mainCameraTransform.forward;
    }

    /// <summary>
    /// Updates the enemy fill bar percentage
    /// </summary>
    public void UpdateHealthUI(int currentHealth, int maxHealth)
    {
        if (maxHealth <= 0) return;
        healthBarFill.fillAmount = (float)currentHealth / maxHealth;
    }
}