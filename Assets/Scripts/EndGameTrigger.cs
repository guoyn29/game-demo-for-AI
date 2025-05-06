using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // 需要引入UI命名空间

public class EndGameTrigger : MonoBehaviour
{
    public GameObject endGameCanvas; // 拖拽你的Canvas或Panel到这里

    private void OnTriggerEnter(Collider other)
    {
        // 检查碰撞对象是否是玩家
        if (other.CompareTag("Player"))
        {
            // 显示结束游戏UI
            if (endGameCanvas != null)
            {
                endGameCanvas.SetActive(true);

                // 暂停游戏（可选）
                Time.timeScale = 0f;
            }

            // 可以在这里添加其他结束游戏逻辑
        }
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