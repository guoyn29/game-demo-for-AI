using UnityEngine;

public class ClerkInteraction : MonoBehaviour
{
    [Header("��������")]
    public GameObject foodPrefab;  // ����ʳ��Ԥ����
    public float throwPower = 12f; // Ͷ������

    [Header("��������")]
    public CatHealth playerHealth; // ������Ҷ���
    public float healthAdd = 20f;  // ÿ�����ӵ�����ֵ

    [Header("�ӽ�����")]
    public float rotateSpeed = 4f;        // ��תƽ���ٶ�
    public float maxBodyAngle = 45f;      // �������Ťת�Ƕ�

    private Transform player;      // �Զ���ȡ���
    private Transform handPoint;   // �Զ�����Ͷ����

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

            if (Input.GetKeyDown(KeyCode.E) && IsPlayerClose())
            {
                ThrowFood();
                AddHealth();
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
}
