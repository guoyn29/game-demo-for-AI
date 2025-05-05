using UnityEngine;

public class ClerkInteraction : MonoBehaviour
{
    [Header("基本设置")]
    public GameObject foodPrefab;  // 拖入食物预制体
    public float throwPower = 12f; // 投掷力度

    [Header("体力设置")]
    public CatHealth playerHealth; // 拖入玩家对象
    public float healthAdd = 20f;  // 每次增加的体力值

    [Header("视角设置")]
    public float rotateSpeed = 4f;        // 旋转平滑速度
    public float maxBodyAngle = 45f;      // 最大身体扭转角度

    private Transform player;      // 自动获取玩家
    private Transform handPoint;   // 自动生成投掷点

    void Start()
    {
        // 自动获取玩家
        player = GameObject.FindWithTag("Player").transform;

        // 自动创建投掷点
        handPoint = new GameObject("ThrowPoint").transform;
        handPoint.SetParent(transform);
        handPoint.localPosition = new Vector3(0.5f, 1f, 0f); // 右手位置

        // 自动获取健康组件
        if (!playerHealth && player)
            playerHealth = player.GetComponent<CatHealth>();
    }

    void Update()
    {
        if (player)
        {
            FacePlayer(); // 持续面向玩家

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
        // 仅水平旋转
        Vector3 lookPos = player.position;
        lookPos.y = transform.position.y;

        // 计算目标朝向
        Quaternion targetRotation = Quaternion.LookRotation(lookPos - transform.position);

        // 限制最大旋转角度
        if (Quaternion.Angle(transform.rotation, targetRotation) > maxBodyAngle)
        {
            targetRotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                maxBodyAngle
            );
        }

        // 平滑旋转
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
            Debug.LogWarning("找不到CatHealth组件！请确认：1.玩家有CatHealth组件 2.已拖入playerHealth引用");
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
