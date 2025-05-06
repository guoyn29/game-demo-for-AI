using UnityEngine;
using UnityEngine.UI;
using TMPro;
using StarterAssets;
using System.Collections;

public class CatHealth : MonoBehaviour
{
    // 原有字段保持不变
    [Header("Base Settings")]
    public float maxHealth = 100f;
    [Tooltip("Initial health value")]
    public float initialHealth = 60f;
    private float currentHealth;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;

    [Header("UI Components")]
    public Slider staminaBar;
    public Image fillImage;
    public TMP_Text staminaText;

    // 新增机制相关字段
    [Header("Health Decay Settings")]
    [Tooltip("每秒下降的健康值")] 
    public float healthDecayRate = 1f / 60f; // 每60秒下降1点
    [Tooltip("低健康阈值")]
    public float lowHealthThreshold = 20f;
    [Tooltip("低健康时速度乘数")]
    [Range(0.1f, 1f)] public float lowHealthSpeedMultiplier = 0.6f;

    [Header("References")]
    public FirstPersonController fpsController;
    public GameObject lowHealthWarningPrefab;

    private float originalMoveSpeed;
    private bool isLowHealth;

    void Start()
    {
        // 原有初始化
        currentHealth = initialHealth;
        if (staminaBar != null)
        {
            staminaBar.maxValue = maxHealth;
            staminaBar.value = currentHealth;
        }
        UpdateHealthUI();

        // 新增初始化
        if (fpsController != null)
        {
            originalMoveSpeed = fpsController.MoveSpeed;
        }
    }

    void Update()
    {
        // 新增健康衰减逻辑
        currentHealth -= healthDecayRate * Time.deltaTime;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHealthUI();

        // 检查低健康状态
        CheckLowHealthEffects();
    }

    // 原有方法保持不变
    public void AddHealth(float amount)
    {
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        UpdateHealthUI();
    }

    private void UpdateHealthUI()
    {
        if (staminaBar == null) return;

        staminaBar.value = currentHealth;
        fillImage.color = Color.Lerp(Color.red, Color.green, currentHealth / maxHealth);

        if (staminaText != null)
        {
            staminaText.text = $"HEALTH: {currentHealth:F0}/{maxHealth}";
        }
    }

    // 新增低健康逻辑
    private void CheckLowHealthEffects()
    {
        bool shouldBeLowHealth = currentHealth < lowHealthThreshold;

        if (shouldBeLowHealth && !isLowHealth)
        {
            EnterLowHealthState();
        }
        else if (!shouldBeLowHealth && isLowHealth)
        {
            ExitLowHealthState();
        }
    }

    private void EnterLowHealthState()
    {
        isLowHealth = true;
        
        // 调整移动速度
        if (fpsController != null)
        {
            fpsController.MoveSpeed = originalMoveSpeed * lowHealthSpeedMultiplier;
        }

        // 显示警告提示
        if (lowHealthWarningPrefab != null)
        {
            ShowWarningPopup();
        }
    }

    private void ExitLowHealthState()
    {
        isLowHealth = false;
        
        // 恢复移动速度
        if (fpsController != null)
        {
            fpsController.MoveSpeed = originalMoveSpeed;
        }
    }

    private void ShowWarningPopup()
    {
        if (lowHealthWarningPrefab == null) return;

        WarningPopup warningPopup = lowHealthWarningPrefab.GetComponent<WarningPopup>();
        if (warningPopup != null)
        {
            TMP_Text warningText = lowHealthWarningPrefab.GetComponentInChildren<TMP_Text>();
            if (warningText != null)
            {
                warningText.text = "Warning! Health Value is too low!";
            }

            warningPopup.Show();
            StartCoroutine(HidePopupAfterDelay(warningPopup, 5f));
        }
    }

    private IEnumerator HidePopupAfterDelay(WarningPopup popup, float delay)
    {
        yield return new WaitForSeconds(delay);
        popup.Hide();
    }

}