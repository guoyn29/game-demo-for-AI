using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;

public class EmotionEventSystem : MonoBehaviour
{
    public static EmotionEventSystem Instance { get; private set; }

    [System.Serializable]
    public class EmotionEvent : UnityEvent<string> { }

    public EmotionEvent OnEmotionDetected = new EmotionEvent();

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
}
