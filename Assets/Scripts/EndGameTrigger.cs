using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // 需要引入UI命名空间
using UnityEngine.SceneManagement;

public class EndGameTrigger : MonoBehaviour
{
    public GameObject endGameCanvas; // 拖拽你的Canvas或Panel到这里

    public float delayBeforeLoad = 5f; // 延迟加载时间（秒）
    
    private bool hasTriggered = false; // 防止重复触发

    private void OnTriggerEnter(Collider other)
    {
        // 检查是否已经触发过或者碰撞对象不是玩家
        if (hasTriggered || !other.CompareTag("Player")) 
            return;

        hasTriggered = true;
        
        // 显示结束游戏UI
        if (endGameCanvas != null)
        {
            endGameCanvas.SetActive(true);
        }

        // 暂停游戏（可选）
        Time.timeScale = 0f;
        
        // 解锁并显示鼠标（如果需要）
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // 启动延迟加载协程
        StartCoroutine(DelayedSceneLoad());
    }

    IEnumerator DelayedSceneLoad()
    {
        // 等待指定时间（使用unscaledDeltaTime因为Time.timeScale可能是0）
        float elapsed = 0f;
        while (elapsed < delayBeforeLoad)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        // 恢复时间缩放（如果需要）
        Time.timeScale = 1f;
        
        // 加载结束菜单场景
        SceneManager.LoadScene("exit_menu");
    }

    // 如果添加了关闭按钮，可以调用这个方法
    // public void ClosePopup()
    // {
    //     if (endGameCanvas != null)
    //     {
    //         endGameCanvas.SetActive(false);
    //         Time.timeScale = 1f; // 恢复游戏时间
    //     }
    // }
}