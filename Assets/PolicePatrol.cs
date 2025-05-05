using UnityEngine;
using UnityEngine.AI;

public class PolicePatrol : MonoBehaviour
{
    public Transform pointA;
    public Transform pointB;
    public float chaseRange = 8f;
    public float keepDistance = 2f;

    private NavMeshAgent agent;
    private Transform player;
    private bool isChasing;
    private bool targetIsA;
    private float chaseTimer;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        agent.SetDestination(pointA.position);
        targetIsA = true;
    }

    void Update()
    {
        if (isChasing)
        {
            chaseTimer += Time.deltaTime;

            // ±£³Ö¾àÀë×·Öð
            Vector3 targetPos = player.position -
                              (player.position - transform.position).normalized * keepDistance;
            agent.SetDestination(targetPos);

            // Ç¿ÖÆ8Ãë·µ»Ø
            if (chaseTimer >= 8f)
            {
                isChasing = false;
                agent.SetDestination(targetIsA ? pointA.position : pointB.position);
            }
        }
        else
        {
            // ¼òµ¥Ñ²ÂßÂß¼­
            if (agent.remainingDistance < 0.5f)
            {
                targetIsA = !targetIsA;
                agent.SetDestination(targetIsA ? pointA.position : pointB.position);
            }

            // ¼ì²âÍæ¼Ò
            if (Vector3.Distance(transform.position, player.position) <= chaseRange)
            {
                isChasing = true;
                chaseTimer = 0;
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
    }
}
