using UnityEngine;
using UnityEngine.AI;

public class SkellyAI : MonoBehaviour 
{
    public Transform player;
    public float stopDistance = 3f;

    public float attackCooldown = 2.0f; // Seconds between attacks
    private float nextAttackTime = 0;
    
    private NavMeshAgent agent;
    private Animator anim;

    void Start() {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
    }

    void Update() {
        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= stopDistance && Time.time >= nextAttackTime) {
            // 1. Stop the Agent completely
            agent.velocity = Vector3.zero; // Remove any sliding momentum
            
            // 2. Face the player (Y remains at skeleton's height to prevent tilting)
            Vector3 lookPos = new Vector3(player.position.x, transform.position.y, player.position.z);
            transform.LookAt(lookPos);

            // 3. Only trigger attack if we aren't already playing it
            Attack();
            nextAttackTime = Time.time + attackCooldown;
            
        } 
        else {
            // 4. Resume chasing
            agent.isStopped = false;
            agent.SetDestination(player.position);
            
            // Set speed for the Walk/Run animation
            anim.SetFloat("Speed", agent.velocity.magnitude);
        }
    }

    void Attack()
    {
        // This checks if the Animator is currently in the 'Attack' state
        if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
        {
            agent.isStopped = true;
            anim.SetTrigger("Attack");
            anim.SetFloat("Speed", 0); // Ensure we stay in Idle/Attack pose
        }
    }
}