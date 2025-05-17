using UnityEngine;
using TMPro;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class CrowController : MonoBehaviour
{
    // ===== 食物设置 =====
    [Header("Food Settings")]
    public GameObject food;          // 拖入食物Prefab或场景物体
    public float dropHeight = 2f;    // 食物掉落偏移高度

    // ===== UI设置 =====
    [Header("UI Settings")]
    public string smileMessage = "Smile to get food!";
    public string rewardMessage = "Crow: Hey pal, don't starve yourself!";

    private bool hasGivenReward = false;
    private bool playerNearby = false;

    [Header("表情组件")]
    private WebcamDisplay emotionDetector; // 表情识别组件

    [Header("弹窗提示")]
    public GameObject lowHealthWarningPrefab;

    [Header("食物投放")]
    public CatHealth playerHealth; // ������Ҷ���
    public float healthAdd = 18f;  // ÿ�����ӵ�����ֵ

    private Transform player; 

    void Start()
    {
        UpdateHint(""); // 默认隐藏提示
        emotionDetector = FindObjectOfType<WebcamDisplay>();
        player = GameObject.FindWithTag("Player").transform;

        if (emotionDetector == null)
        {
            Debug.LogWarning("WebcamDisplay not found in scene.");
        }

        // 初始化食物状态
        if (food != null)
        {
            food.SetActive(false);
        }
        if (!playerHealth && player)
            playerHealth = player.GetComponent<CatHealth>();
    }

    void Update()
    {
        // 每帧轮询当前表情
        if (emotionDetector != null && playerNearby && !hasGivenReward)
        {
            string emotion = emotionDetector.GetLastDetectedEmotion();  // 你需要确保 WebcamDisplay 脚本实现了这个方法
            HandleEmotionDetection(emotion);
        }

        // 模拟调试键（可删除）
        if (playerNearby && Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("模拟微笑触发");
            UpdateHint(rewardMessage);
            string emotion = emotionDetector.GetLastDetectedEmotion();  // 你需要确保 WebcamDisplay 脚本实现了这个方法
            HandleEmotionDetection(emotion);
            
        }
    }

    // 玩家进入乌鸦感应范围
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasGivenReward)
        {
            playerNearby = true;
            UpdateHint(smileMessage);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;
            if (!hasGivenReward)
            {
                UpdateHint("");
            }
        }
    }

    private void HandleEmotionDetection(string emotion)
    {
        if (emotion == "Happy")
        {
            healthAdd = 28f;
            AddHealth();
            OnSmileDetected();
        }
        else
        {
            healthAdd = 18f;
            AddHealth();
            OnSmileDetected();
        }
    }

    public void OnSmileDetected()
    {
        if (!hasGivenReward)
        {
            GiveReward();
        }
    }

    void GiveReward()
    {
        hasGivenReward = true;
        UpdateHint(rewardMessage);

        if (food != null)
        {
            FoodController fc = food.GetComponent<FoodController>();
            if (fc != null)
            {
                Vector3 dropPos = transform.position + Vector3.up * dropHeight;
                fc.DropFood(dropPos);
            }
            else
            {
                Debug.LogError("Missing FoodController component on food object.");
            }
        }

        // 延时清除提示
        Invoke(nameof(ClearHint), 2f);
    }

    void UpdateHint(string message)
    {
        WarningPopup warningPopup = lowHealthWarningPrefab.GetComponent<WarningPopup>();
        TMP_Text warningText = lowHealthWarningPrefab.GetComponentInChildren<TMP_Text>();
        if (warningPopup && warningText)
        {
            //Debug.LogWarning("Girl: Hope you like the food I brought!2");
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

    void AddHealth()
    {
        if (playerHealth)
            playerHealth.AddHealth(healthAdd);
        else
            Debug.LogWarning("�Ҳ���CatHealth�������ȷ�ϣ�1.�����CatHealth��� 2.������playerHealth����");
    }

    void ClearHint()
    {
        UpdateHint("");
    }
}
