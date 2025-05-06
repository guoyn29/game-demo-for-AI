using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // 如果需要重新加载场景

public class GameEndTrigger : MonoBehaviour
{
    public GameObject gameEndUI; // 游戏结束UI界面

    //private void OnTriggerEnter2D(Collider2D other) // 如果是2D游戏
    private void OnTriggerEnter(Collider other) // 如果是3D游戏
    {
        // 检查触发的是不是玩家（小猫）
        if (other.CompareTag("Player"))
        {
            // 显示游戏结束UI
            if (gameEndUI != null)
            {
                gameEndUI.SetActive(true);
            }

            // 暂停游戏时间（可选）
            Time.timeScale = 0f;

            // 禁用玩家控制（如果有需要）
            // other.GetComponent<PlayerController>().enabled = false;
        }
    }

    // 重新开始游戏的方法（绑定到UI按钮）
    public void RestartGame()
    {
        Time.timeScale = 1f; // 恢复时间
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

}