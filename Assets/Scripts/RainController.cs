using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RainController : MonoBehaviour
{
    public float rainDuration = 600f; // 10分钟
    private float timer = 0f;
    public bool isRaining = false;

    void Start()
    {
        StartRain();
    }

    void Update()
    {
        if (isRaining)
        {
            timer += Time.deltaTime;

            if (timer >= rainDuration)
            {
                StopRain();
            }
        }
    }

    public void StartRain()
    {
        isRaining = true;
        timer = 0f;
        // 激活雨粒子效果等
        Debug.Log("It's starting to rain!");
    }

    public void StopRain()
    {
        isRaining = false;
        // 停止雨粒子效果等
        Debug.Log("The rain has stopped!");

        // 这里可以触发关卡完成逻辑
    }
}