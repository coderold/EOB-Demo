using System;
using UnityEngine;
using UnityEngine.AI;

public class SkellyAI : MonoBehaviour 
{
    public Transform player;
    public float aggroRange = 15f;   // <-- NEW: How far away the skeleton can notice the player
    public float stopDistance = 4f;

    public float attackCooldown = 2.0f;
    private float nextAttackTime = 0;
    public event Action OnTargetDestroyed;
    
    private NavMeshAgent agent;
    private Animator anim;
    private Health health;
    private bool isDead = false;

    void Awake() {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        health = GetComponent<Health>();
    }

    void OnEnable() {
        if (health != null) {
            health.OnDie += HandleDeath;
        }
    }

    void OnDisable() {
        if (health != null) {
            health.OnDie -= HandleDeath;
        }
    }

    void Update() {
        if (isDead) return;

        float distance = Vector3.Distance(transform.position, player.position);

        // State 1: Player is close enough to attack
        if (distance <= stopDistance) {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;

            Vector3 lookPos = new Vector3(player.position.x, transform.position.y, player.position.z);
            transform.LookAt(lookPos);

            anim.SetFloat("Speed", 0);

            if (Time.time >= nextAttackTime) {
                Attack();
                nextAttackTime = Time.time + attackCooldown;
            }
        } 
        // State 2: Player is out of attack range, but inside aggro range (CHASE)
        else if (distance <= aggroRange) {
            agent.isStopped = false;
            agent.SetDestination(player.position);
            
            anim.SetFloat("Speed", agent.velocity.magnitude);
        }
        // State 3: Player is too far away (IDLE)
        else {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
            anim.SetFloat("Speed", 0);
        }
    }

    void Attack()
    {
        if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
        {
            anim.SetTrigger("Attack");
        }
    }

    private void HandleDeath()
    {
        if (isDead) return;
        isDead = true;

        anim.SetTrigger("Die");
        anim.SetFloat("Speed", 0);

        if (agent != null)
        {
            agent.velocity = Vector3.zero;
            agent.isStopped = true;
            agent.enabled = false;
        }

        if (TryGetComponent<Collider>(out var col))
        {
            col.enabled = false;
        }

        Transform canvasHead = transform.Find("WorldSpaceCanvas");
        if (canvasHead != null)
        {
            canvasHead.gameObject.SetActive(false);
        }

        TriggerLootDrop();
        OnTargetDestroyed?.Invoke();
        Destroy(gameObject, 5f);
    }

    private void TriggerLootDrop()
    {
        Invoke("CallDropLoot", 3f);
    }

    private void CallDropLoot()
    {
        health.DropLoot();
    }

    // Helper to visualize the ranges in the Unity Editor Scene view
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, stopDistance); // Attack Range

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, aggroRange);   // Aggro Range
    }
}