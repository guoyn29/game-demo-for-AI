using UnityEngine;
using System.Collections.Generic;

public class TrafficSpawner : MonoBehaviour
{
    [Header("车辆 Prefab")]
    public List<GameObject> vehiclePrefabs;


    [Header("基础行为参数")]
    // 移除或注释掉固定的 baseSpeed，因为它将由情绪控制
    // public float baseSpeed = 15.0f;
    [Tooltip("车辆速度在基础速度上的随机变化范围 (+/-)")]
    public float speedVariation = 5.0f;
    [Tooltip("所有车辆允许的最低速度")]
    public float minSpeed = 5.0f;
    public float acceleration = 3f;
    public float deceleration = 6f;

    [Header("情绪检测联动")]
    [Tooltip("将场景中带有 WebcamDisplay 脚本的 GameObject 拖拽到这里")]
    public WebcamDisplay emotionDetector; // 对情绪检测脚本的引用

    [Tooltip("情绪变化导致的速度改变的平滑度（值越小，变化越快，建议0.05到0.2之间）")]
    [Range(0.01f, 1.0f)]
    public float speedChangeSmoothFactor = 0.1f; // 用于平滑速度变化的Lerp因子

    [Header("情绪对应的目标基础速度")]
    public float happySpeed = 22f;
    public float angrySpeed = 28f;   // 例如：愤怒时开快车
    public float sadSpeed = 8f;     // 例如：悲伤时开慢车
    public float neutralSpeed = 15f;
    public float fearSpeed = 10f;
    public float disgustSpeed = 12f;
    public float surpriseSpeed = 18f;
    public float unknownSpeed = 15f; // 当检测不到或情绪未知时的默认速度

    // --- 内部状态变量 ---
    private float timer; // 生成计时器 (沿用之前的)
    private float currentActualBaseSpeed; // 当前实际使用的基础速度（经过平滑处理）
    private float targetEmotionBasedSpeed; // 由当前情绪决定的目标基础速度
    private float emotionCheckTimer = 0f; // 用于控制情绪检查频率的计时器
    private readonly float emotionCheckInterval = 0.5f; // 每隔多少秒检查一次情绪（不需要太频繁）

    [Header("感知与安全距离")]
    public float detectionDistance = 20f;
    public float safeDistance = 10f;
    public LayerMask vehicleLayer; // 确保这个设置了车辆层
    public float sensorHeightOffset = 0.5f;
    public float sensorForwardOffset = 1.5f;
    public float sensorRadius = 0.8f;

    [Header("生成设置")]
    public float spawnInterval = 1.0f;
    public List<Transform> spawnPointsDirection1;
    public float destroyXPositive = 100.0f;
    public List<Transform> spawnPointsDirection2;
    public float destroyXNegative = -100.0f;

    [Header("生成点占用检测")]
    public Vector3 spawnCheckHalfSize = new Vector3(1.0f, 1.0f, 2.5f);

    void Start()
    {
        // 初始化速度
        targetEmotionBasedSpeed = neutralSpeed; // 游戏开始时假设是中性情绪的速度
        currentActualBaseSpeed = targetEmotionBasedSpeed; // 初始实际速度等于目标速度

        // 检查 emotionDetector 是否已在 Inspector 中设置
        if (emotionDetector == null)
        {
            Debug.LogError("错误：情绪检测器(Emotion Detector) 未在 TrafficSpawner 的 Inspector 中分配！", this);
            // 可以选择禁用此脚本或采取其他错误处理
            // this.enabled = false;
        }
    }
    void Update()
    {
        // --- 定期检查情绪并更新目标速度 ---
        emotionCheckTimer += Time.deltaTime;
        if (emotionCheckTimer >= emotionCheckInterval)
        {
            emotionCheckTimer = 0f; // 重置计时器
            UpdateTargetSpeedFromEmotion(); // 更新目标速度
        }

        // --- 平滑地更新当前实际基础速度 ---
        // 使用 Lerp 让速度变化更自然，避免突变
        currentActualBaseSpeed = Mathf.Lerp(currentActualBaseSpeed, targetEmotionBasedSpeed, speedChangeSmoothFactor * Time.deltaTime * (1.0f / emotionCheckInterval));
        // 或者使用 MoveTowards 实现固定速率的变化
        // float speedChangeRate = 5.0f; // 每秒变化多少速度单位
        // currentActualBaseSpeed = Mathf.MoveTowards(currentActualBaseSpeed, targetEmotionBasedSpeed, speedChangeRate * Time.deltaTime);


        // --- 车辆生成计时器 ---
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer -= spawnInterval;
            // 使用当前平滑后的 currentActualBaseSpeed 来生成车辆
            SpawnVehicle();
        }
    }
    void UpdateTargetSpeedFromEmotion()
    {
        if (emotionDetector == null) return; // 如果没有检测器，直接返回

        string detectedEmotion = emotionDetector.GetLastDetectedEmotion(); // 从情绪检测脚本获取最新情绪

        // 根据情绪字符串查找对应的目标速度
        switch (detectedEmotion)
        {
            case "Happy":
                targetEmotionBasedSpeed = happySpeed;
                break;
            case "Angry":
                targetEmotionBasedSpeed = angrySpeed;
                break;
            case "Sad":
                targetEmotionBasedSpeed = sadSpeed;
                break;
            case "Neutral":
                targetEmotionBasedSpeed = neutralSpeed;
                break;
            case "Fear":
                targetEmotionBasedSpeed = fearSpeed;
                break;
            case "Disgust":
                targetEmotionBasedSpeed = disgustSpeed;
                break;
            case "Surprise":
                targetEmotionBasedSpeed = surpriseSpeed;
                break;
            case "Unknown": // 处理未识别或初始状态
            default:        // 其他任何未明确列出的情况
                targetEmotionBasedSpeed = unknownSpeed;
                break;
        }
         Debug.Log($"检测到情绪: {detectedEmotion}, 目标基础速度更新为: {targetEmotionBasedSpeed}"); // 可选的调试信息
    }
    void SpawnVehicle()
    {
        // 1. --- 前置检查 ---
        if (vehiclePrefabs == null || vehiclePrefabs.Count == 0)
        {
            Debug.LogError("错误：车辆 Prefab 列表 (Vehicle Prefabs) 未设置或为空！无法生成车辆。", this);
            return;
        }
        if (vehicleLayer == 0) // LayerMask 未设置时的默认值是 0 (Nothing)
        {
            Debug.LogError("错误: 车辆层 (Vehicle Layer) 未在 TrafficSpawner 的 Inspector 中设置!", this);
            return;
        }

        // 2. --- 选择生成方向和对应的参数 ---
        bool spawnInDirection1 = Random.Range(0, 2) == 0; // 随机选择方向
        List<Transform> pointsToCheck = null;
        Vector3 moveDirection = Vector3.zero;
        float destroyX = 0;

        if (spawnInDirection1)
        {
            if (spawnPointsDirection1 != null && spawnPointsDirection1.Count > 0)
            {
                pointsToCheck = spawnPointsDirection1;
                moveDirection = Vector3.right; // 方向 (1, 0, 0)
                destroyX = destroyXPositive;
            }
            else { return; } // 方向1没有有效的生成点列表
        }
        else // Spawn in Direction 2
        {
            if (spawnPointsDirection2 != null && spawnPointsDirection2.Count > 0)
            {
                pointsToCheck = spawnPointsDirection2;
                moveDirection = Vector3.left; // 方向 (-1, 0, 0)
                destroyX = destroyXNegative;
            }
            else { return; } // 方向2没有有效的生成点列表
        }

        // 过滤掉列表中可能的 null 元素，并创建索引列表用于随机化
        List<int> validIndices = new List<int>();
        for (int i = 0; i < pointsToCheck.Count; ++i)
        {
            if (pointsToCheck[i] != null) // 确保 Transform 不是 null
            {
                validIndices.Add(i);
            }
        }

        if (validIndices.Count == 0) return; // 没有有效的非空生成点 Transform

        // 3. --- 随机尝试找到一个未被占用的生成点 ---
        // 使用 Fisher-Yates shuffle 算法打乱有效索引列表，确保公平性
        int n = validIndices.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n + 1);
            int value = validIndices[k];
            validIndices[k] = validIndices[n];
            validIndices[n] = value;
        }

        Transform selectedSpawnPoint = null; // 用于存储找到的安全生成点
        foreach (int index in validIndices)
        {
            Transform potentialPoint = pointsToCheck[index]; // 获取当前尝试的生成点Transform

            // 使用 OverlapBox 检查该点周围是否有车辆碰撞体
            // 参数: 中心点, 半尺寸 (需要乘以0.5), 旋转, 检测层
            Collider[] hitColliders = Physics.OverlapBox(potentialPoint.position, spawnCheckHalfSize, potentialPoint.rotation, vehicleLayer);

            // 如果 hitColliders 数组长度为 0，说明该区域是安全的
            if (hitColliders.Length == 0)
            {
                selectedSpawnPoint = potentialPoint; // 找到了！
                break; // 不再检查其他点
            }
            // 可选：输出哪个点被阻塞
            // else { Debug.Log($"生成点 {potentialPoint.name} 被阻挡。"); }
        }

        // 4. --- 如果找到了安全的生成点，则生成车辆 ---
        if (selectedSpawnPoint != null)
        {
            // 4a. 随机选择一个车辆 Prefab
            int prefabIndex = Random.Range(0, vehiclePrefabs.Count);
            GameObject prefabToSpawn = vehiclePrefabs[prefabIndex];

            // 健壮性检查：防止列表中有空元素
            if (prefabToSpawn == null)
            {
                Debug.LogWarning($"警告：车辆 Prefab 列表索引 {prefabIndex} 处的元素为空，跳过此次生成。", this);
                return;
            }

            // 4b. 在选定的安全位置实例化 Prefab
            GameObject newVehicle = Instantiate(prefabToSpawn, selectedSpawnPoint.position, selectedSpawnPoint.rotation);

            // 4c. 获取车辆的移动脚本组件
            VehicleMovement vehicleScript = newVehicle.GetComponent<VehicleMovement>();

            if (vehicleScript != null)
            {
                // 4d. 计算这辆车的最大速度 (基于当前情绪影响的基础速度 + 随机变化)
                // 使用 currentActualBaseSpeed (已经过平滑处理)
                float randomMaxSpeed = currentActualBaseSpeed + Random.Range(-speedVariation, speedVariation);
                randomMaxSpeed = Mathf.Max(minSpeed, randomMaxSpeed); // 确保速度不低于设定的最低速度
                Debug.Log($"[生成车辆] 情绪影响的基础速度: {currentActualBaseSpeed:F2}, 赋予车辆 MaxSpeed: {randomMaxSpeed:F2}");
                // 4e. 调用车辆脚本的 Initialize 方法，传递所有必要的参数
                vehicleScript.Initialize(
                    moveDirection,
                    randomMaxSpeed,       // 计算出的最大速度
                    minSpeed,             // 全局最低速度
                    acceleration,         // 全局加速度
                    deceleration,         // 全局减速度
                    detectionDistance,    // 感知距离
                    safeDistance,         // 安全距离
                    vehicleLayer,         // 车辆层 (用于车辆自身检测)
                    destroyX,             // 销毁边界坐标
                    sensorHeightOffset,   // 传感器高度偏移
                    sensorForwardOffset,  // 传感器前向偏移
                    sensorRadius          // 传感器半径
                );

                // 注意：根据之前的讨论，这里的旋转逻辑已被移除。
                // 如果你的Prefab朝向不是默认就匹配行驶方向，可能需要在这里重新添加旋转逻辑。
            }
            else
            {
                // 如果实例化的 Prefab 上没有找到 VehicleMovement 脚本
                Debug.LogError($"错误：生成的车辆 '{prefabToSpawn.name}' 上没有找到 VehicleMovement 脚本！请确保脚本已添加到所有车辆 Prefab 上。", newVehicle);
                // 销毁这个不完整的车辆，避免潜在问题
                Destroy(newVehicle);
            }
        }
        else
        {
            // 如果在所有有效的生成点中都没有找到安全的空位
            // Debug.Log("在方向 " + (spawnInDirection1 ? "1" : "2") + " 上没有找到安全的生成点，本次跳过生成。"); // 可选调试信息
        }
    }

    // (可选) OnDrawGizmosSelected 可以保持不变，用于绘制生成点检查框
    void OnDrawGizmosSelected()
    {
        if (vehicleLayer == 0) Gizmos.color = Color.yellow; // 提醒设置LayerMask
        else Gizmos.color = Color.cyan;

        DrawGizmosForPoints(spawnPointsDirection1);
        DrawGizmosForPoints(spawnPointsDirection2);
    }
    void DrawGizmosForPoints(List<Transform> points)
    {
        if (points != null)
        {
            foreach (Transform spawnPoint in points)
            {
                if (spawnPoint != null)
                {
                    // 保存当前 Gizmos 矩阵
                    Matrix4x4 currentMatrix = Gizmos.matrix;
                    // 设置 Gizmos 矩阵为生成点的世界变换
                    Gizmos.matrix = Matrix4x4.TRS(spawnPoint.position, spawnPoint.rotation, Vector3.one);
                    // 绘制本地坐标下的线框立方体，尺寸是完整尺寸 (半尺寸 * 2)
                    Gizmos.DrawWireCube(Vector3.zero, spawnCheckHalfSize * 2f);
                    // 恢复之前的 Gizmos 矩阵
                    Gizmos.matrix = currentMatrix;
                }
            }
        }
    }
}