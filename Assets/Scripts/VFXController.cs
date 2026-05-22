using UnityEngine;

namespace Echoes.Game.Presentation
{
    public class VFXController : MonoBehaviour
    {
        [Header("VFX Prefabs")]
        [SerializeField] private GameObject vfxPrefab;

        [Header("Spawn References")]
        [SerializeField] private Transform tip;

        [Header("Tweak Alignment")]
        [Tooltip("Manually offset the VFX rotation (e.g., X: 90 if it spawns flat on the ground)")]
        [SerializeField] private Vector3 rotationOffset;

        public void TriggerSlashVFX()
        {
            if (vfxPrefab == null || tip == null) return;

            Quaternion cleanRotation = tip.rotation * Quaternion.Euler(rotationOffset);

            GameObject vfxInstance = Instantiate(vfxPrefab, tip.position, cleanRotation);
            
            Destroy(vfxInstance, 1.5f);
            Debug.Log($"VFX: {vfxPrefab.name} played");
        }
    }
}