using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathGuide : MonoBehaviour 
{
    [Header("粒子设置")]
    public GameObject pathParticlePrefab;
    [Tooltip("按顺序设置路径关键点")]
    public Transform[] pathPoints;

    private List<GameObject> activeParticles = new List<GameObject>();

    void Start()
    {
        // 自动获取子路径点（可选）
        // pathPoints = GetComponentsInChildren<Transform>();
    }

    public void ShowPath() 
    {
        StartCoroutine(SpawnPath());
    }

    IEnumerator SpawnPath() 
    {
        // 清除旧粒子
        HidePath();

        foreach (var point in pathPoints) 
        {
            if(point == null) continue;
            
            var particle = Instantiate(pathParticlePrefab, 
                                     point.position, 
                                     Quaternion.identity);
            activeParticles.Add(particle);
            
            // 添加上下浮动动画
            StartCoroutine(FloatAnimation(particle));
            
            yield return new WaitForSeconds(0.3f);
        }
    }

    IEnumerator FloatAnimation(GameObject particle)
    {
        float startY = particle.transform.position.y;
        while(particle != null)
        {
            float newY = startY + Mathf.Sin(Time.time * 3f) * 0.2f;
            particle.transform.position = new Vector3(
                particle.transform.position.x,
                newY,
                particle.transform.position.z
            );
            yield return null;
        }
    }

    public void HidePath() 
    {
        foreach (var p in activeParticles) 
        {
            if(p != null) Destroy(p);
        }
        activeParticles.Clear();
    }
}