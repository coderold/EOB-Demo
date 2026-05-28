using System;
using UnityEngine;
using UnityEngine.AI;
using Unity.Netcode; // CRITICAL: Added for Netcode for GameObjects

public class SkellyAI : NetworkBehaviour // Changed from MonoBehaviour
{
    // Removed the public Transform player slot since it's evaluated dynamically now
    [HideInInspector] public Transform player; 
    public float aggroRange = 15f;   
    public float stopDistance = 4f;

    public float attackCooldown = 2.0f;
    private float _nextAttackTime = 0;
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
        // 1. Netcode Guard: Only the Server/Host should calculate AI state transitions and movement
        if (!IsServer) return;
        if (isDead) return;

        // 2. Dynamic Target Acquisition: Find the absolute closest network player
        FindClosestNetworkPlayer();

        // 3. If no players are spawned or alive anywhere on the server, default to Idle
        if (!player) {
            GoIdle();
            return;
        }

        float distance = Vector3.Distance(transform.position, player.position);

        // State 1: Player is close enough to attack
        if (distance <= stopDistance) {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;

            Vector3 lookPos = new Vector3(player.position.x, transform.position.y, player.position.z);
            transform.LookAt(lookPos);

            anim.SetFloat("Speed", 0);

            if (Time.time >= _nextAttackTime) {
                Attack();
                _nextAttackTime = Time.time + attackCooldown;
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
            GoIdle();
        }
    }


    private void FindClosestNetworkPlayer()
    {
        if (!NetworkManager.Singleton) return;

        var connectedClients = NetworkManager.Singleton.ConnectedClientsList;
        Transform closestTransform = null;
        float shortestDistance = Mathf.Infinity;
        Vector3 currentPosition = transform.position;

        foreach (var client in connectedClients)
        {
            // Skip clients that haven't instantiated their player prefab yet
            if (client.PlayerObject == null) continue;

            Transform playerTransform = client.PlayerObject.transform;
            float distanceToPlayer = Vector3.Distance(currentPosition, playerTransform.position);

            if (distanceToPlayer < shortestDistance)
            {
                shortestDistance = distanceToPlayer;
                closestTransform = playerTransform;
            }
        }

        // Set the active player focus target
        player = closestTransform;
    }

    private void GoIdle()
    {
        if (agent != null && agent.enabled)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }
        anim.SetFloat("Speed", 0);
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, stopDistance); // Attack Range

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, aggroRange);   // Aggro Range
    }
}