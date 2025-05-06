using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using TMPro;
using StarterAssets;
using System.Collections;

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

    public GameObject lowHealthWarningPrefab;

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

            // ���־���׷��
            Vector3 targetPos = player.position -
                              (player.position - transform.position).normalized * keepDistance;
            agent.SetDestination(targetPos);

            // ǿ��8�뷵��
            if (chaseTimer >= 8f)
            {
                isChasing = false;
                agent.SetDestination(targetIsA ? pointA.position : pointB.position);
            }
        }
        else
        {
            // ��Ѳ���߼�
            if (agent.remainingDistance < 0.5f)
            {
                targetIsA = !targetIsA;
                agent.SetDestination(targetIsA ? pointA.position : pointB.position);
            }

            // ������
            if (Vector3.Distance(transform.position, player.position) <= chaseRange)
            {
                isChasing = true;
                chaseTimer = 0;

                ShowPopupMessage("Police: I see you! Don’t even think about running!");
            }
        }
    }

    void ShowPopupMessage(string message)
    {
        WarningPopup warningPopup = lowHealthWarningPrefab.GetComponent<WarningPopup>();
        TMP_Text warningText = lowHealthWarningPrefab.GetComponentInChildren<TMP_Text>();
        if (warningPopup && warningText)
        {
            Debug.LogWarning("Girl: Hope you like the food I brought!2");
            warningText.text = message;
            warningPopup.Show();
            StartCoroutine(HidePopupDelayed(warningPopup, 5f)); // 5秒后自动隐藏
        }
    }

    IEnumerator HidePopupDelayed(WarningPopup popup, float delay)
    {
        yield return new WaitForSeconds(delay);
        popup.Hide();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
    }
}
