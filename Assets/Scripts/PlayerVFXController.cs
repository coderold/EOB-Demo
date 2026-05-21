using UnityEngine;

namespace Echoes.Game.Presentation
{
    public class PlayerVFXController : MonoBehaviour
    {
        [Header("VFX Prefabs")]
        [SerializeField] private GameObject slashVfxPrefab;

        [Header("Spawn References")]
        [SerializeField] private Transform weaponTipOrHand;

        [Header("Tweak Alignment")]
        [Tooltip("Manually offset the VFX rotation (e.g., X: 90 if it spawns flat on the ground)")]
        [SerializeField] private Vector3 rotationOffset;

        public void TriggerSlashVFX()
        {
            if (slashVfxPrefab == null || weaponTipOrHand == null) return;

            // Combine the weapon's current rotation with your custom visual offset
            Quaternion cleanRotation = weaponTipOrHand.rotation * Quaternion.Euler(rotationOffset);

            // Instantiate with the corrected arc rotation
            GameObject vfxInstance = Instantiate(slashVfxPrefab, weaponTipOrHand.position, cleanRotation);
            
            Destroy(vfxInstance, 1.5f);
        }
    }
}