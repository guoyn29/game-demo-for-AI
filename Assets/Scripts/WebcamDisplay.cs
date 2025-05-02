using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using OpenCvSharp;
using OpenCvSharp.Demo;
using OpenCvSharp.Util;
using OpenCvSharp.Aruco;
using OpenCvSharp.Flann;
using OpenCvSharp.Detail;
using OpenCvSharp.Tracking;
using OpenCvSharp.XFeatures2D;
using OpenCvSharp.ML;
using System.Linq;
using System.Collections.Generic;
using Unity.Sentis;

public class WebcamDisplay : MonoBehaviour
{
    [Header("UI Components")]
    public RawImage cameraDisplay;
    public Button recognizeButton;
    public Text resultText;

    [Header("Model Settings")]
    public ModelAsset modelAsset;  // 需要将 .onnx 模型拖到此字段
    public string inputName = "input_1"; // 模型的输入层名称
    public string outputName = "predictions"; // 模型的输出层名称
    public string[] emotionLabels = new string[] { "Angry", "Disgust", "Fear", "Happy", "Sad", "Surprise", "Neutral" };

    private WebCamTexture webCamTexture;
    private Worker worker;

    // 初始化和按钮点击事件
    void Start()
    {
        webCamTexture = new WebCamTexture();
        cameraDisplay.texture = webCamTexture;
        webCamTexture.Play();

        // 加载模型并初始化工作者
        var model = ModelLoader.Load(modelAsset);
        worker = new Worker(model, BackendType.CPU);
        //worker = WorkerFactory.CreateWorker(modelAsset);

        recognizeButton.onClick.AddListener(() => StartCoroutine(RunInference()));
        StartCoroutine(DetectEvery15Seconds());
    }

    // 每15秒进行一次检测的协程
    IEnumerator DetectEvery15Seconds()
    {
        while (true)  // 无限循环
        {
            // 执行情绪检测
            yield return StartCoroutine(RunInference());  // 执行推理，等待其完成

            // 等待15秒后再执行下一次推理
            yield return new WaitForSeconds(15f);
        }
    }

    IEnumerator RunInference()
    {
        // 从摄像头获取图像
        Texture2D snap = new Texture2D(webCamTexture.width, webCamTexture.height, TextureFormat.RGB24, false);
        snap.SetPixels(webCamTexture.GetPixels());
        snap.Apply();

        // 将图片裁剪并缩放成模型输入大小
        Texture2D resized = ScaleTexture(snap, 64, 64);  // 调整为模型需要的输入尺寸
        var inputTensor = TextureToTensor(resized);  // 将图片转换为 Tensor

        // 创建输入字典并开始推理
        var inputs = new Dictionary<string, Tensor> { { inputName, inputTensor } };
        worker.Schedule(inputTensor);  // 执行推理
        Tensor<float> output = (worker.PeekOutput() as Tensor<float>);  // 获取输出结果
        output.CompleteAllPendingOperations();  // 等待推理完成
        TensorShape shape = output.shape;
        Debug.Log("Output Tensor Shape: " + shape.ToString());
        Unity.Collections.NativeArray<float>.ReadOnly outputArray = output.AsReadOnlyNativeArray();  // 将 Tensor 转换为数组
        float maxProbability = -1f;
        int maxIndex = -1; 
        // 处理输出，输出每个情绪的概率
        // 输出每个情绪类别的概率
        for (int i = 0; i < output.shape[1]; i++)
        {
            Debug.Log($"Probability for {emotionLabels[i]}: {outputArray[i].ToString("F3")}");
            // 寻找最大概率
            if (outputArray[i] > maxProbability)
            {
                maxProbability = outputArray[i];
                maxIndex = i;
            }
        }

        // 显示预测的情绪
        //resultText.text = resultStr;
         if (recognizeButton != null)
            recognizeButton.GetComponentInChildren<TextMeshProUGUI>().text = $"Emotion: {emotionLabels[maxIndex]}";

        // 清理资源
        inputTensor.Dispose();
        output.Dispose();

        yield return null;
    }

    private Texture2D ScaleTexture(Texture2D source, int targetWidth, int targetHeight)
    {
        RenderTexture rt = RenderTexture.GetTemporary(targetWidth, targetHeight);
        Graphics.Blit(source, rt);
        RenderTexture.active = rt;
        Texture2D result = new Texture2D(targetWidth, targetHeight);
        result.ReadPixels(new UnityEngine.Rect(0, 0, targetWidth, targetHeight), 0, 0);
        result.Apply();
        RenderTexture.ReleaseTemporary(rt);
        return result;
    }

    private Tensor TextureToTensor(Texture2D texture)
    {
        // 将 Texture 转换为 Tensor，假设需要单通道灰度图
        float[] pixelValues = new float[texture.width * texture.height];
        for (int i = 0; i < texture.width; i++)
        {
            for (int j = 0; j < texture.height; j++)
            {
                Color pixelColor = texture.GetPixel(i, j);
                float grayscale = pixelColor.grayscale;  // 获取灰度值
                pixelValues[i * texture.height + j] = grayscale;
            }
        }

        // 创建并返回 Tensor
        Tensor<float> tensor = new Tensor<float>(new TensorShape(1, texture.height, texture.width, 1), pixelValues);
        //Tensor tensor = new Tensor(1, texture.height, texture.width, 1, pixelValues);
        return tensor;
    }

    private void OnDestroy()
    {
        webCamTexture.Stop();
        worker?.Dispose();  // 释放工作者资源
    }
}