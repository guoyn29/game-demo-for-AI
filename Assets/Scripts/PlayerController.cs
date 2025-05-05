using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float baseSpeed = 5f;
    public float health = 100f;
    public float rainDamageRate = 5f; // 每秒受到的伤害
    public float shelterHealRate = 10f; // 在避雨处的恢复速度

    private float currentSpeed;
    private bool isInShelter = false;
    private Emotion currentEmotion = Emotion.Neutral;

    public enum Emotion
    {
        Angry, Disgust, Fear, Happy, Sad, Surprise, Neutral
    }

    void Start()
    {
        currentSpeed = baseSpeed;
    }

    void Update()
    {
        HandleMovement();
        HandleRainDamage();
        UpdateSpeedBasedOnEmotion();
    }

    private void HandleMovement()
    {
        if (Input.GetMouseButton(0)) // 鼠标左键前进
        {
            Vector3 moveDirection = Camera.main.transform.forward;
            moveDirection.y = 0; // 保持水平移动
            transform.position += moveDirection.normalized * currentSpeed * Time.deltaTime;
        }

        // 鼠标控制视角
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");
        Camera.main.transform.RotateAround(transform.position, Vector3.up, mouseX * 2f);
        Camera.main.transform.RotateAround(transform.position, Camera.main.transform.right, -mouseY * 2f);
    }

    private void HandleRainDamage()
    {
        if (!isInShelter)
        {
            health -= rainDamageRate * Time.deltaTime;
            health = Mathf.Clamp(health, 0, 100);

            if (health <= 0)
            {
                GameOver();
            }
        }
        else
        {
            health += shelterHealRate * Time.deltaTime;
            health = Mathf.Clamp(health, 0, 100);
        }
    }

    public void SetEmotion(string emotionStr)
    {
        if (System.Enum.TryParse(emotionStr, out Emotion emotion))
        {
            currentEmotion = emotion;
        }
    }

    private void UpdateSpeedBasedOnEmotion()
    {
        switch (currentEmotion)
        {
            case Emotion.Angry:
            case Emotion.Disgust:
            case Emotion.Fear:
            case Emotion.Sad:
                currentSpeed = baseSpeed * 0.7f; // 负面情绪减速
                break;
            case Emotion.Happy:
                currentSpeed = baseSpeed * 1.3f; // 快乐加速
                break;
            default:
                currentSpeed = baseSpeed; // 中性情绪保持
                break;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Shelter"))
        {
            isInShelter = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Shelter"))
        {
            isInShelter = false;
        }
    }

    // 在PlayerController.cs中添加：

    private void OnEnable()
    {
        EmotionEventSystem.Instance.OnEmotionDetected.AddListener(HandleEmotionChange);
    }

    private void OnDisable()
    {
        EmotionEventSystem.Instance.OnEmotionDetected.RemoveListener(HandleEmotionChange);
    }

    private void HandleEmotionChange(string emotion)
    {
        if (System.Enum.TryParse(emotion, out Emotion emotionEnum))
        {
            currentEmotion = emotionEnum;
            UpdateSpeedBasedOnEmotion();

            // 可以在这里添加其他情绪相关的效果
            Debug.Log($"情绪变化: {emotion}");
        }
    }

    private void GameOver()
    {
        // 游戏结束逻辑
        Debug.Log("Game Over - You were knocked down by the rain.");
        // 可以在这里添加重新开始或返回菜单的逻辑
    }
}