using UnityEngine;
using Echoes.Game.Presentation;

namespace Echoes.Game.Environment
{
    [RequireComponent(typeof(Collider))]
    public class BakuForgeTrigger : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                if (BakuForgeUI.Instance != null)
                {
                    BakuForgeUI.Instance.ToggleForgeZoneAccess(true);
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                if (BakuForgeUI.Instance != null)
                {
                    BakuForgeUI.Instance.ToggleForgeZoneAccess(false);
                }
            }
        }
    }
}