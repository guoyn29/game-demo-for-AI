using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CatHealth : MonoBehaviour
{
    [Header("Base Settings")]
    public float maxHealth = 100f;
    [Tooltip("Initial health value")]
    public float initialHealth = 60f;
    private float currentHealth;

    // ÐÂÔöÊôÐÔ
    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;

    [Header("UI Components")]
    public Slider staminaBar;
    public Image fillImage;
    public TMP_Text staminaText;

    void Start()
    {
        currentHealth = initialHealth;
        if (staminaBar != null)
        {
            staminaBar.maxValue = maxHealth;
            staminaBar.value = currentHealth;
        }
        UpdateHealthUI();
    }

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
}
