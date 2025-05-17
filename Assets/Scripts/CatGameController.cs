using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.InputSystem;
using System.Text;
using UnityEngine.SceneManagement;

public class CatGameController : MonoBehaviour
{
    [Header("UI Elements")]
    //public GameObject titleText;
    public GameObject storyButton;
    public GameObject guidanceButton;
    public GameObject startGameButton;
    public GameObject quitGameButton;

    public GameObject storyPanel;
    public TextMeshProUGUI storyText;
    public GameObject guidancePanel;
    public TextMeshProUGUI guidanceText;

    private bool isInSubMenu = false; // 标记是否在子菜单中

    [Header("Game Elements")]
    public Light homeIndicator; // 家的方向指示光点

    [Header("Settings")]
    public float typingSpeed = 0.05f;
    [TextArea(3, 10)]
    public string fullStoryText = "When you woke up, you were a lost cat, placed in a waste cardboard box on the streets of Tokyo. Your head hurts very much. You have forgotten the way home, but you seem to have an impression of the direction of home. The streets at night are very dangerous for a weak and lovely kitten. Please avoid these dangers and find your way home. It seems that there are lights flickering in the distance, at familiar frequencies...";


    void Start()
    {
        // 初始化UI状态
        Cursor.visible = true; // 确保光标可见
        Cursor.lockState = CursorLockMode.None; // 解锁光标
        Cursor.visible = true;
        Cursor.lockState = 0;

        // 显示主菜单元素
        //titleText.SetActive(true);
        storyButton.SetActive(true);
        guidanceButton.SetActive(true);
        startGameButton.SetActive(true);

        // 隐藏面板
        storyPanel.SetActive(false);
        guidancePanel.SetActive(false);
        homeIndicator.gameObject.SetActive(false);

        // 设置按钮点击事件
        storyButton.GetComponent<Button>().onClick.AddListener(ShowStory);
        guidanceButton.GetComponent<Button>().onClick.AddListener(ShowGuidance);
        startGameButton.GetComponent<Button>().onClick.AddListener(StartGame);
        quitGameButton.GetComponent<Button>().onClick.AddListener(QuitGame);
        

        // 设置操作指南文本内容
        guidanceText.text = @"=== CONTROLS ===

Movement:
- Mouse move: Look around
- Left mouse click: Move to clicked position
- A/W/S/D arrow keys: Strafe movement
- Space key: Jump
- E key: Interaction with game characters

Game Tips:
- The blue light in the distance indicates home direction
- Your emotions will affect the game world
- Explore the environment to find your way home";
    }

    // 显示故事背景
    public void ShowStory()
    {
        // 隐藏主菜单按钮
        //titleText.SetActive(false);
        storyButton.SetActive(false);
        guidanceButton.SetActive(false);
        startGameButton.SetActive(false);

        // 显示故事面板
        storyPanel.SetActive(true);
        isInSubMenu = true;
        StartCoroutine(TypeStoryText());
    }

    // 逐字显示故事文本
    IEnumerator TypeStoryText()
    {
        storyText.text = "";
        var sb = new StringBuilder();
        
        for (int i = 0; i < fullStoryText.Length; i++)
        {
            sb.Append(fullStoryText[i]);
            storyText.text = sb.ToString();
            yield return new WaitForSeconds(typingSpeed);
        }
    }

    // 返回主菜单
    public void ReturnToMainMenu()
    {
        // 隐藏所有面板
        storyPanel.SetActive(false);
        guidancePanel.SetActive(false);

        // 显示主菜单元素
        //titleText.SetActive(true);
        storyButton.SetActive(true);
        guidanceButton.SetActive(true);
        startGameButton.SetActive(true);

        isInSubMenu = false;
    }

    // 显示操作指南
    public void ShowGuidance()
    {
        // 隐藏主菜单按钮
        //titleText.SetActive(false);
        storyButton.SetActive(false);
        guidanceButton.SetActive(false);
        startGameButton.SetActive(false);

        // 显示操作指南面板
        guidancePanel.SetActive(true);
        isInSubMenu = true;
    }

    // 开始游戏
    public void StartGame()
    {
        // 隐藏所有UI元素
        //titleText.SetActive(false);
        storyButton.SetActive(false);
        guidanceButton.SetActive(false);
        startGameButton.SetActive(false);
        storyPanel.SetActive(false);
        guidancePanel.SetActive(false);
        quitGameButton.SetActive(false);

        // 激活家的指示光点并开始闪烁
        homeIndicator.gameObject.SetActive(true);
        StartCoroutine(BlinkHomeIndicator());

        // 这里可以添加游戏正式开始的逻辑
        Debug.Log("Game Started!");

                // 隐藏UI
        storyButton.SetActive(false);
        guidanceButton.SetActive(false);
        startGameButton.SetActive(false);
        quitGameButton.SetActive(false);

        Time.timeScale = 1f;

        SceneManager.LoadScene("ai_tokyo");
    }

    // 家的光点闪烁效果
    IEnumerator BlinkHomeIndicator()
    {
        while (true)
        {
            // 使用正弦函数创造平滑的闪烁效果
            float intensity = Mathf.Sin(Time.time * 2f) * 0.5f + 0.8f;
            homeIndicator.intensity = intensity;
            yield return null;
        }
    }

    void Update()
    {
        // 如果在子菜单中且检测到鼠标点击
        if (isInSubMenu && (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2)))
        {
            ReturnToMainMenu();
        }
    }

    public void QuitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}