using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class test_emotion : MonoBehaviour
{
    public Button testEmotionButton; 
    public WebcamDisplay webcamDisplay;
    // Start is called before the first frame update
    void Start()
    {
        testEmotionButton.onClick.AddListener(OnTestEmotionButtonClicked);
    }

    // Update is called once per frame
    void OnTestEmotionButtonClicked()
    {
        string emotion = webcamDisplay.GetLastDetectedEmotion();
        Debug.Log($"{emotion} success!");
    }
}
