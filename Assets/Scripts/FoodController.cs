using UnityEngine;

public class FoodController : MonoBehaviour
{
    [Header("Settings")]
    public float lifetime = 5f; // 食物存在时间（秒）

    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }

        rb.useGravity = true;
        rb.isKinematic = true; // 初始禁用刚体模拟
        gameObject.SetActive(false); // 初始隐藏
    }

    // 由乌鸦调用以掉落食物
    public void DropFood(Vector3 dropPosition)
    {
        transform.position = dropPosition;
        gameObject.SetActive(true);
        rb.isKinematic = false; // 启用物理
        Invoke(nameof(DestroySelf), lifetime);
    }

    private void DestroySelf()
    {
        Destroy(gameObject);
    }
}
