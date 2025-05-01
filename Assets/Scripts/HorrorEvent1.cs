using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarterAssets;

[System.Serializable]
public class HorrorEvent1 : MonoBehaviour
{
    [Header("horror setting")]
    public AudioClip scareSound;          // 触发时的音效（如尖叫）
    public ParticleSystem visualEffect;   // 触发时的粒子特效（如血迹）
    public float fearIncrease = 0.3f;     // 每次触发增加的恐惧值

    [Header("player finding")]
    public FirstPersonController playerController; // 拖入你的玩家控制器

    private bool isTriggered = false;     // 防止重复触发

    void OnTriggerEnter(Collider other)
    {
        // 打印所有进入触发器的物体信息
        Debug.Log($"进入触发器的物体：{other.name} | 层级：{LayerMask.LayerToName(other.gameObject.layer)} | 标签：{other.tag}");
        // 当玩家进入触发区域且未触发过
        if (!isTriggered && other.CompareTag("Player") && playerController != null)
        {
            // 播放音效
            if (scareSound != null)
            {
                Debug.Log($"音效长度：{scareSound.length}秒");
                AudioSource.PlayClipAtPoint(scareSound, transform.position);
            }

            // 播放粒子特效
            if (visualEffect != null)
            {
                visualEffect.Play();
            }

            // 增加玩家恐惧值
            playerController.fearLevel = Mathf.Clamp(playerController.fearLevel + fearIncrease, 0f, 1f);

            // 标记为已触发（可选：Destroy(gameObject)直接销毁触发器）
            isTriggered = true;
            
            Debug.Log($"恐怖事件触发！当前恐惧值：{playerController.fearLevel}");
        }
    }

    // 可视化触发器范围（仅在编辑器中显示）
    void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 0, 0, 0.3f);
        Gizmos.DrawCube(transform.position, GetComponent<BoxCollider>().size);
    }
        // Use this for initialization
    void Start () {
        Debug.Log($"控制台测试");
    }
        
    // Update is called once per frame
    void Update () {
        
    }
}
