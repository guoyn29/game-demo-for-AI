using UnityEngine;
using UnityEngine.UI;
using TMPro;
using StarterAssets;
using System.Collections;

public class ClerkInteraction : MonoBehaviour
{
    [Header("��������")]
    public GameObject foodPrefab;  // ����ʳ��Ԥ����
    public float throwPower = 12f; // Ͷ������

    [Header("��������")]
    public CatHealth playerHealth; // ������Ҷ���
    public float healthAdd = 30f;  // ÿ�����ӵ�����ֵ

    [Header("�ӽ�����")]
    public float rotateSpeed = 4f;        // ��תƽ���ٶ�
    public float maxBodyAngle = 45f;      // �������Ťת�Ƕ�

    private Transform player;      // �Զ���ȡ���
    private Transform handPoint;   // �Զ�����Ͷ����

    [Header("弹窗提示")]
    public GameObject lowHealthWarningPrefab;

    private bool hasShownPopup = false;

    void Start()
    {
        // �Զ���ȡ���
        player = GameObject.FindWithTag("Player").transform;

        // �Զ�����Ͷ����
        handPoint = new GameObject("ThrowPoint").transform;
        handPoint.SetParent(transform);
        handPoint.localPosition = new Vector3(0.5f, 1f, 0f); // ����λ��

        // �Զ���ȡ�������
        if (!playerHealth && player)
            playerHealth = player.GetComponent<CatHealth>();
    }

    void Update()
    {
        if (player)
        {
            FacePlayer(); // �����������
            if (IsPlayerClose())
            {
                if (Input.GetKeyDown(KeyCode.E) && !hasShownPopup)
                {
                    ThrowFood();
                    AddHealth();
                    ShowPopupMessage("Girl: Hope you like the food I brought!");
                    Debug.LogWarning("Girl: Hope you like the food I brought!1");
                    hasShownPopup = true;
                }
                else if (!hasShownPopup)
                {
                    ShowPopupMessage("Girl: Don't be scared. Here, have some food.");
                }
            }
        }
    }

    bool IsPlayerClose() =>
        player && Vector3.Distance(transform.position, player.position) < 3f;

    void FacePlayer()
    {
        // ��ˮƽ��ת
        Vector3 lookPos = player.position;
        lookPos.y = transform.position.y;

        // ����Ŀ�곯��
        Quaternion targetRotation = Quaternion.LookRotation(lookPos - transform.position);

        // ���������ת�Ƕ�
        if (Quaternion.Angle(transform.rotation, targetRotation) > maxBodyAngle)
        {
            targetRotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                maxBodyAngle
            );
        }

        // ƽ����ת
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotateSpeed * Time.deltaTime
        );
    }

    void ThrowFood()
    {
        if (!foodPrefab) return;

        GameObject food = Instantiate(foodPrefab, handPoint.position, Quaternion.identity);
        Vector3 dir = (player.position - handPoint.position).normalized;
        food.GetComponent<Rigidbody>().AddForce(dir * throwPower, ForceMode.Impulse);
        Destroy(food, 3f);
    }

    void AddHealth()
    {
        if (playerHealth)
            playerHealth.AddHealth(healthAdd);
        else
            Debug.LogWarning("�Ҳ���CatHealth�������ȷ�ϣ�1.�����CatHealth��� 2.������playerHealth����");
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (handPoint)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(handPoint.position, 0.1f);
        }
    }
#endif

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
}
