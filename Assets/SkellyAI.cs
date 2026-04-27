using UnityEngine;
using UnityEngine.AI;

public class SkellyAI : MonoBehaviour 
{
    public Transform player;
    public float stopDistance = 2f;
    
    private NavMeshAgent agent;
    private Animator anim;

    void Start() {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
    }

    void Update() {
        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= stopDistance) {
            // 1. Stop the Agent completely
            agent.isStopped = true;
            agent.velocity = Vector3.zero; // Remove any sliding momentum
            
            // 2. Face the player (Y remains at skeleton's height to prevent tilting)
            Vector3 lookPos = new Vector3(player.position.x, transform.position.y, player.position.z);
            transform.LookAt(lookPos);

            // 3. Only trigger attack if we aren't already playing it
            // This checks if the Animator is currently in the 'Attack' state
            if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Attack")) {
                anim.SetTrigger("Attack");
                anim.SetFloat("Speed", 0); // Ensure we stay in Idle/Attack pose
            }
        } 
        else {
            // 4. Resume chasing
            agent.isStopped = false;
            agent.SetDestination(player.position);
            
            // Set speed for the Walk/Run animation
            anim.SetFloat("Speed", agent.velocity.magnitude);
        }
    }
}