using UnityEngine;

public class MeleeAttack : MonoBehaviour
{
    [Header("Attack Properties")]
    [SerializeField] private int attackDamage = 25;
    [SerializeField] private float attackRadius = 1.5f;
    
    [Header("Detection Settings")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private LayerMask targetLayer; 

    public void PerformHitDetection()
    {

        Collider[] hitTargets = Physics.OverlapSphere(attackPoint.position, attackRadius, targetLayer);

        foreach (Collider target in hitTargets)
        {

            if (target.TryGetComponent(out IDamageable damageable))
            {
                damageable.TakeDamage(attackDamage);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
    }
}