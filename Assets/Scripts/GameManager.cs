using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("UI Elements")]
    public GameObject gameOverPanel;
    public GameObject successPanel;
    public Text resultText;

    private bool gameEnded = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayerReachedHome()
    {
        if (gameEnded) return;

        gameEnded = true;
        successPanel.SetActive(true);
        resultText.text = "Congratulations on your successful return home!";
        Time.timeScale = 0f; // 暂停游戏
    }

    public void PlayerGameOver()
    {
        if (gameEnded) return;

        gameEnded = true;
        gameOverPanel.SetActive(true);
        resultText.text = "The game is over - you failed to get home safely";
        Time.timeScale = 0f; // 暂停游戏
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}