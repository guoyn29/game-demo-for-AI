using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class WarningPopupButtonHandler : MonoBehaviour
{
    [Header("弹窗引用")]
    public WarningPopup warningPopup; // 拖入弹窗 GameObject 上的 WarningPopup 脚本

    [Header("是否自动隐藏")]
    public bool autoHide = true;

    [Header("自动隐藏延迟（秒）")]
    public float hideDelay = 2f;

    public Button testEmotionButton;

    void Start()
    {
        testEmotionButton.onClick.AddListener(OnButtonClick);
    }

    // 这个函数会被绑定到 Button 的 OnClick 事件
    public void OnButtonClick()
    {
        // Debug.LogWarning("点击show1");
        if (warningPopup != null)
        {
            warningPopup.Show();
            // Debug.LogWarning("点击show2");

            if (autoHide)
            {
                StartCoroutine(HideAfterDelay(hideDelay));
            }
        }
        else
        {
            Debug.LogWarning("未分配 WarningPopup 引用！");
        }
    }

    private IEnumerator HideAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        warningPopup.Hide();
    }
}
