using UnityEngine;
using System.Collections.Generic;

public class TrafficSpawner : MonoBehaviour
{
    [Header("���� Prefab")]
    public List<GameObject> vehiclePrefabs;


    [Header("������Ϊ����")]
    // �Ƴ���ע�͵��̶��� baseSpeed����Ϊ��������������
    // public float baseSpeed = 15.0f;
    [Tooltip("�����ٶ��ڻ����ٶ��ϵ�����仯��Χ (+/-)")]
    public float speedVariation = 5.0f;
    [Tooltip("���г������������ٶ�")]
    public float minSpeed = 5.0f;
    public float acceleration = 3f;
    public float deceleration = 6f;

    [Header("�����������")]
    [Tooltip("�������д��� WebcamDisplay �ű��� GameObject ��ק������")]
    public WebcamDisplay emotionDetector; // ���������ű�������

    [Tooltip("�����仯���µ��ٶȸı��ƽ���ȣ�ֵԽС���仯Խ�죬����0.05��0.2֮�䣩")]
    [Range(0.01f, 1.0f)]
    public float speedChangeSmoothFactor = 0.1f; // ����ƽ���ٶȱ仯��Lerp����

    [Header("������Ӧ��Ŀ������ٶ�")]
    public float happySpeed = 22f;
    public float angrySpeed = 28f;   // ���磺��ŭʱ���쳵
    public float sadSpeed = 8f;     // ���磺����ʱ������
    public float neutralSpeed = 15f;
    public float fearSpeed = 10f;
    public float disgustSpeed = 12f;
    public float surpriseSpeed = 18f;
    public float unknownSpeed = 15f; // ����ⲻ��������δ֪ʱ��Ĭ���ٶ�

    // --- �ڲ�״̬���� ---
    private float timer; // ���ɼ�ʱ�� (����֮ǰ��)
    private float currentActualBaseSpeed; // ��ǰʵ��ʹ�õĻ����ٶȣ�����ƽ������
    private float targetEmotionBasedSpeed; // �ɵ�ǰ����������Ŀ������ٶ�
    private float emotionCheckTimer = 0f; // ���ڿ����������Ƶ�ʵļ�ʱ��
    private readonly float emotionCheckInterval = 0.5f; // ÿ����������һ������������Ҫ̫Ƶ����

    [Header("��֪�밲ȫ����")]
    public float detectionDistance = 20f;
    public float safeDistance = 10f;
    public LayerMask vehicleLayer; // ȷ����������˳�����
    public float sensorHeightOffset = 0.5f;
    public float sensorForwardOffset = 1.5f;
    public float sensorRadius = 0.8f;

    [Header("��������")]
    public float spawnInterval = 1.0f;
    public List<Transform> spawnPointsDirection1;
    public float destroyXPositive = 100.0f;
    public List<Transform> spawnPointsDirection2;
    public float destroyXNegative = -100.0f;

    [Header("���ɵ�ռ�ü��")]
    public Vector3 spawnCheckHalfSize = new Vector3(1.0f, 1.0f, 2.5f);

    void Start()
    {
        // ��ʼ���ٶ�
        targetEmotionBasedSpeed = neutralSpeed; // ��Ϸ��ʼʱ�����������������ٶ�
        currentActualBaseSpeed = targetEmotionBasedSpeed; // ��ʼʵ���ٶȵ���Ŀ���ٶ�

        // ��� emotionDetector �Ƿ����� Inspector ������
        if (emotionDetector == null)
        {
            Debug.LogError("�������������(Emotion Detector) δ�� TrafficSpawner �� Inspector �з��䣡", this);
            // ����ѡ����ô˽ű����ȡ����������
            // this.enabled = false;
        }
    }
    void Update()
    {
        // --- ���ڼ������������Ŀ���ٶ� ---
        emotionCheckTimer += Time.deltaTime;
        if (emotionCheckTimer >= emotionCheckInterval)
        {
            emotionCheckTimer = 0f; // ���ü�ʱ��
            UpdateTargetSpeedFromEmotion(); // ����Ŀ���ٶ�
        }

        // --- ƽ���ظ��µ�ǰʵ�ʻ����ٶ� ---
        // ʹ�� Lerp ���ٶȱ仯����Ȼ������ͻ��
        currentActualBaseSpeed = Mathf.Lerp(currentActualBaseSpeed, targetEmotionBasedSpeed, speedChangeSmoothFactor * Time.deltaTime * (1.0f / emotionCheckInterval));
        // ����ʹ�� MoveTowards ʵ�̶ֹ����ʵı仯
        // float speedChangeRate = 5.0f; // ÿ��仯�����ٶȵ�λ
        // currentActualBaseSpeed = Mathf.MoveTowards(currentActualBaseSpeed, targetEmotionBasedSpeed, speedChangeRate * Time.deltaTime);


        // --- �������ɼ�ʱ�� ---
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer -= spawnInterval;
            // ʹ�õ�ǰƽ����� currentActualBaseSpeed �����ɳ���
            SpawnVehicle();
        }
    }
    void UpdateTargetSpeedFromEmotion()
    {
        if (emotionDetector == null) return; // ���û�м������ֱ�ӷ���

        string detectedEmotion = emotionDetector.GetLastDetectedEmotion(); // ���������ű���ȡ��������

        // ���������ַ������Ҷ�Ӧ��Ŀ���ٶ�
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
            case "Unknown": // ����δʶ����ʼ״̬
            default:        // �����κ�δ��ȷ�г������
                targetEmotionBasedSpeed = unknownSpeed;
                break;
        }
         Debug.Log($"��⵽����: {detectedEmotion}, Ŀ������ٶȸ���Ϊ: {targetEmotionBasedSpeed}"); // ��ѡ�ĵ�����Ϣ
    }
    void SpawnVehicle()
    {
        // 1. --- ǰ�ü�� ---
        if (vehiclePrefabs == null || vehiclePrefabs.Count == 0)
        {
            Debug.LogError("���󣺳��� Prefab �б� (Vehicle Prefabs) δ���û�Ϊ�գ��޷����ɳ�����", this);
            return;
        }
        if (vehicleLayer == 0) // LayerMask δ����ʱ��Ĭ��ֵ�� 0 (Nothing)
        {
            Debug.LogError("����: ������ (Vehicle Layer) δ�� TrafficSpawner �� Inspector ������!", this);
            return;
        }

        // 2. --- ѡ�����ɷ���Ͷ�Ӧ�Ĳ��� ---
        bool spawnInDirection1 = Random.Range(0, 2) == 0; // ���ѡ����
        List<Transform> pointsToCheck = null;
        Vector3 moveDirection = Vector3.zero;
        float destroyX = 0;

        if (spawnInDirection1)
        {
            if (spawnPointsDirection1 != null && spawnPointsDirection1.Count > 0)
            {
                pointsToCheck = spawnPointsDirection1;
                moveDirection = Vector3.right; // ���� (1, 0, 0)
                destroyX = destroyXPositive;
            }
            else { return; } // ����1û����Ч�����ɵ��б�
        }
        else // Spawn in Direction 2
        {
            if (spawnPointsDirection2 != null && spawnPointsDirection2.Count > 0)
            {
                pointsToCheck = spawnPointsDirection2;
                moveDirection = Vector3.left; // ���� (-1, 0, 0)
                destroyX = destroyXNegative;
            }
            else { return; } // ����2û����Ч�����ɵ��б�
        }

        // ���˵��б��п��ܵ� null Ԫ�أ������������б����������
        List<int> validIndices = new List<int>();
        for (int i = 0; i < pointsToCheck.Count; ++i)
        {
            if (pointsToCheck[i] != null) // ȷ�� Transform ���� null
            {
                validIndices.Add(i);
            }
        }

        if (validIndices.Count == 0) return; // û����Ч�ķǿ����ɵ� Transform

        // 3. --- ��������ҵ�һ��δ��ռ�õ����ɵ� ---
        // ʹ�� Fisher-Yates shuffle �㷨������Ч�����б�ȷ����ƽ��
        int n = validIndices.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n + 1);
            int value = validIndices[k];
            validIndices[k] = validIndices[n];
            validIndices[n] = value;
        }

        Transform selectedSpawnPoint = null; // ���ڴ洢�ҵ��İ�ȫ���ɵ�
        foreach (int index in validIndices)
        {
            Transform potentialPoint = pointsToCheck[index]; // ��ȡ��ǰ���Ե����ɵ�Transform

            // ʹ�� OverlapBox ���õ���Χ�Ƿ��г�����ײ��
            // ����: ���ĵ�, ��ߴ� (��Ҫ����0.5), ��ת, ����
            Collider[] hitColliders = Physics.OverlapBox(potentialPoint.position, spawnCheckHalfSize, potentialPoint.rotation, vehicleLayer);

            // ��� hitColliders ���鳤��Ϊ 0��˵���������ǰ�ȫ��
            if (hitColliders.Length == 0)
            {
                selectedSpawnPoint = potentialPoint; // �ҵ��ˣ�
                break; // ���ټ��������
            }
            // ��ѡ������ĸ��㱻����
            // else { Debug.Log($"���ɵ� {potentialPoint.name} ���赲��"); }
        }

        // 4. --- ����ҵ��˰�ȫ�����ɵ㣬�����ɳ��� ---
        if (selectedSpawnPoint != null)
        {
            // 4a. ���ѡ��һ������ Prefab
            int prefabIndex = Random.Range(0, vehiclePrefabs.Count);
            GameObject prefabToSpawn = vehiclePrefabs[prefabIndex];

            // ��׳�Լ�飺��ֹ�б����п�Ԫ��
            if (prefabToSpawn == null)
            {
                Debug.LogWarning($"���棺���� Prefab �б����� {prefabIndex} ����Ԫ��Ϊ�գ������˴����ɡ�", this);
                return;
            }

            // 4b. ��ѡ���İ�ȫλ��ʵ���� Prefab
            GameObject newVehicle = Instantiate(prefabToSpawn, selectedSpawnPoint.position, selectedSpawnPoint.rotation);

            // 4c. ��ȡ�������ƶ��ű����
            VehicleMovement vehicleScript = newVehicle.GetComponent<VehicleMovement>();

            if (vehicleScript != null)
            {
                // 4d. ����������������ٶ� (���ڵ�ǰ����Ӱ��Ļ����ٶ� + ����仯)
                // ʹ�� currentActualBaseSpeed (�Ѿ���ƽ������)
                float randomMaxSpeed = currentActualBaseSpeed + Random.Range(-speedVariation, speedVariation);
                randomMaxSpeed = Mathf.Max(minSpeed, randomMaxSpeed); // ȷ���ٶȲ������趨������ٶ�
                Debug.Log($"[���ɳ���] ����Ӱ��Ļ����ٶ�: {currentActualBaseSpeed:F2}, ���賵�� MaxSpeed: {randomMaxSpeed:F2}");
                // 4e. ���ó����ű��� Initialize �������������б�Ҫ�Ĳ���
                vehicleScript.Initialize(
                    moveDirection,
                    randomMaxSpeed,       // �����������ٶ�
                    minSpeed,             // ȫ������ٶ�
                    acceleration,         // ȫ�ּ��ٶ�
                    deceleration,         // ȫ�ּ��ٶ�
                    detectionDistance,    // ��֪����
                    safeDistance,         // ��ȫ����
                    vehicleLayer,         // ������ (���ڳ���������)
                    destroyX,             // ���ٱ߽�����
                    sensorHeightOffset,   // �������߶�ƫ��
                    sensorForwardOffset,  // ������ǰ��ƫ��
                    sensorRadius          // �������뾶
                );

                // ע�⣺����֮ǰ�����ۣ��������ת�߼��ѱ��Ƴ���
                // ������Prefab������Ĭ�Ͼ�ƥ����ʻ���򣬿�����Ҫ���������������ת�߼���
            }
            else
            {
                // ���ʵ������ Prefab ��û���ҵ� VehicleMovement �ű�
                Debug.LogError($"�������ɵĳ��� '{prefabToSpawn.name}' ��û���ҵ� VehicleMovement �ű�����ȷ���ű�����ӵ����г��� Prefab �ϡ�", newVehicle);
                // ��������������ĳ���������Ǳ������
                Destroy(newVehicle);
            }
        }
        else
        {
            // �����������Ч�����ɵ��ж�û���ҵ���ȫ�Ŀ�λ
            // Debug.Log("�ڷ��� " + (spawnInDirection1 ? "1" : "2") + " ��û���ҵ���ȫ�����ɵ㣬�����������ɡ�"); // ��ѡ������Ϣ
        }
    }

    // (��ѡ) OnDrawGizmosSelected ���Ա��ֲ��䣬���ڻ������ɵ����
    void OnDrawGizmosSelected()
    {
        if (vehicleLayer == 0) Gizmos.color = Color.yellow; // ��������LayerMask
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
                    // ���浱ǰ Gizmos ����
                    Matrix4x4 currentMatrix = Gizmos.matrix;
                    // ���� Gizmos ����Ϊ���ɵ������任
                    Gizmos.matrix = Matrix4x4.TRS(spawnPoint.position, spawnPoint.rotation, Vector3.one);
                    // ���Ʊ��������µ��߿������壬�ߴ��������ߴ� (��ߴ� * 2)
                    Gizmos.DrawWireCube(Vector3.zero, spawnCheckHalfSize * 2f);
                    // �ָ�֮ǰ�� Gizmos ����
                    Gizmos.matrix = currentMatrix;
                }
            }
        }
    }
}