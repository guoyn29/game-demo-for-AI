using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarningPopup : MonoBehaviour
{
    private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
        animator.SetBool("show", false); // 默认隐藏
    }

    // 显示弹窗（播放 PopIn 动画）
    public void Show()
    {
        animator.SetBool("show", true);
    }

    // 隐藏弹窗（播放 PopOut 动画）
    public void Hide()
    {
        animator.SetBool("show", false);
    }
}
