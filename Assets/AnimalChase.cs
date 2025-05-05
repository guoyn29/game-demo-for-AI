using UnityEngine;

public class AnimalChase : MonoBehaviour
{
    [Header("chase setting")]
    public Transform player;    
    public float chaseSpeed = 3.5f;
    public float detectionRange = 10f;

    private bool isChasing = false;

    void Update()
    {
        if (player == null) return;

        // calculate distance
        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= detectionRange || isChasing)
        {
            isChasing = true;
            transform.position = Vector3.MoveTowards(
                transform.position,
                player.position,
                chaseSpeed * Time.deltaTime
            );
            transform.LookAt(player);
        }
    }

    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
