using UnityEngine;
using System.Collections;
using UnityEngine.UI; // 如果使用 UI Image

public class PlayerCollisionHandler : MonoBehaviour
{
    [Header("重生设置")]
    public Transform respawnPoint;

    [Header("撞击效果 (可选)")]
    public AudioClip hitSound;
    public UnityEngine.UI.Image hitFlashImage;
    public float flashDuration = 0.15f;
    public Color flashColor = new Color(1f, 0f, 0f, 0.7f);

    // --- 组件引用 ---
    private AudioSource audioSource;
    private CharacterController characterController;
    private Rigidbody playerRigidbody;

    private bool isHit = false;

    void Start()
    {
        // ... (Start 方法保持不变，获取组件，检查设置) ...
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && hitSound != null) { audioSource = gameObject.AddComponent<AudioSource>(); }
        characterController = GetComponent<CharacterController>();
        playerRigidbody = GetComponent<Rigidbody>();
        if (respawnPoint == null) { Debug.LogError("错误：重生点未设置！", this); }
        if (hitFlashImage != null) { /* ... 初始化闪烁图片 ... */ }
    }

    // --- 碰撞检测方法 ---
    // 保留你正在使用的那个 (OnCollisionEnter 或 OnControllerColliderHit)
    // **重要**: 在调用 StartCoroutine 之前设置 isHit = true

    // 例如: Rigidbody 版本
    void OnCollisionEnter(Collision collision)
    {
        if (isHit) return; // 如果正在处理，则忽略

        int vehicleLayer = LayerMask.NameToLayer("Vehicles"); // 或用 Tag
        if (collision.gameObject.layer == vehicleLayer) // 或 CompareTag("Vehicles")
        {
            Debug.Log("玩家被车辆碰撞！(Rigidbody)");
            isHit = true; // 在启动协程前标记，防止重入
            StartCoroutine(HandleHitSequence());
        }
    }

    // 例如: CharacterController 版本
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (isHit) return; // 如果正在处理，则忽略

        int vehicleLayer = LayerMask.NameToLayer("Vehicles"); // 或用 Tag
        if (hit.gameObject.layer == vehicleLayer) // 或 CompareTag("Vehicles")
        {
            Debug.Log("玩家被车辆碰撞！(CharacterController)");
            isHit = true; // 在启动协程前标记，防止重入
            StartCoroutine(HandleHitSequence());
        }
    }


    // --- 处理撞击、暂停、效果、重生、恢复的协程 ---
    IEnumerator HandleHitSequence()
    {
        // 1. 暂停游戏 (在协程开始时立即执行)
        Time.timeScale = 0f; // !!! 冻结游戏时间 !!!
        Debug.Log("游戏已暂停 (等待效果和重生)");

        // 2. 触发效果 (使用不受时间缩放影响的方式)
        // 播放音效 (PlayOneShot 通常不受暂停影响，但 AudioListener 可能受影响，检查Audio设置)
        if (audioSource != null && hitSound != null)
        {
            // 如果声音在暂停时听不到，可能需要在 AudioSource 上设置 ignoreListenerPause = true
            // 或者检查 Edit -> Project Settings -> Audio -> Disable Unity Audio 的设置
            audioSource.PlayOneShot(hitSound);
        }

        // 屏幕闪烁效果 (使用不受时间影响的版本)
        if (hitFlashImage != null)
        {
            yield return StartCoroutine(FlashScreenUnscaled()); // 等待这个不受时间影响的协程完成
        }
        else
        {
            // 如果没有闪烁效果，也稍微等待一下真实的秒数
            yield return new WaitForSecondsRealtime(0.1f); // 使用 Realtime
        }

        // (其他需要在暂停期间播放的效果也放在这里，并使用 Realtime 等待)

        // 3. 重置玩家位置和状态 (仍然在 Time.timeScale = 0 时执行)
        if (respawnPoint != null)
        {
            // 根据你的角色控制器类型执行重生
            if (characterController != null)
            {
                characterController.enabled = false;
                transform.position = respawnPoint.position;
                transform.rotation = respawnPoint.rotation;
                characterController.enabled = true;
            }
            else if (playerRigidbody != null)
            {
                playerRigidbody.velocity = Vector3.zero;
                playerRigidbody.angularVelocity = Vector3.zero;
                // 对于 Rigidbody，最好在下一物理帧应用位置变化，但直接设置通常也行
                // Rigidbody 的位置变化在 Time.timeScale=0 时可能需要特别注意，直接设置 transform 通常可以
                transform.position = respawnPoint.position;
                transform.rotation = respawnPoint.rotation;
            }
            else
            {
                transform.position = respawnPoint.position;
                transform.rotation = respawnPoint.rotation;
            }
            Debug.Log("玩家已重生 (暂停状态下)");
        }
        else
        {
            Debug.LogError("无法重生玩家，因为重生点未设置！", this);
        }

        // 4. 恢复游戏时间 (在重生完成后立即执行)
        Time.timeScale = 1f; // !!! 恢复时间流动 !!!
        Debug.Log("游戏已恢复运行");

        // 5. 短暂延迟后解除处理标记 (可选的短暂无敌时间)
        // 同样需要使用 Realtime
        yield return new WaitForSecondsRealtime(0.5f); // 例如半秒真实时间后才能再次被撞
        isHit = false;
        Debug.Log("isHit 标志已重置");
    }


    // --- 屏幕闪烁协程 (确保使用 Unscaled Time) ---
    // (这个协程应该和你之前用的那个不受时间影响的版本一样)
    IEnumerator FlashScreenUnscaled()
    {
        if (hitFlashImage == null) yield break;

        hitFlashImage.enabled = true;
        hitFlashImage.color = flashColor;

        yield return new WaitForSecondsRealtime(flashDuration * 0.5f); // 使用 Realtime

        float timer = 0f;
        float fadeDuration = flashDuration * 0.5f;
        Color startColor = flashColor;
        Color endColor = new Color(flashColor.r, flashColor.g, flashColor.b, 0f);

        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime; // 使用 Unscaled Delta Time
            hitFlashImage.color = Color.Lerp(startColor, endColor, timer / fadeDuration);
            yield return null; // 等待下一帧 (不受 timeScale 影响)
        }

        hitFlashImage.color = endColor;
        hitFlashImage.enabled = false;
    }

}