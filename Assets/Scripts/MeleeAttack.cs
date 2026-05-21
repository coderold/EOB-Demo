using UnityEngine;

public class MeleeAttack : MonoBehaviour
{
    [Header("Attack Properties")]
    [SerializeField] private int attackDamage = 25;
    [SerializeField] private float attackRadius = 1.5f;
    
    [Header("Detection Settings")]
    [SerializeField] private Transform attackPoint; // Empty GameObject placed at the tip/front of the character's weapon range
    [SerializeField] private LayerMask targetLayer;   // For Player, select "Enemy". For Enemy, select "Player".

    // This public method will be explicitly triggered by Animation Events
    public void PerformHitDetection()
    {
        // Fire a temporary detection sphere into the target layer
        Collider[] hitTargets = Physics.OverlapSphere(attackPoint.position, attackRadius, targetLayer);

        foreach (Collider target in hitTargets)
        {
            // Check if the hit object implements IDamageable
            if (target.TryGetComponent(out IDamageable damageable))
            {
                damageable.TakeDamage(attackDamage);
            }
        }
    }

    // Allows you to visually calibrate the attack range in the Unity Editor Scene View
    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
    }
}