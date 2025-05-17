using UnityEngine;

public class VehicleMovement : MonoBehaviour
{
    // --- �� Spawner ���� ---
    public Vector3 moveDirection;      // �ƶ�����
    public float minSpeed = 5f;        // ��С�ٶ�
    public float maxSpeed = 15f;       // ��������ٶ� (ÿ�������Բ�ͬ)
    public float acceleration = 3f;    // ���ٶ�
    public float deceleration = 6f;    // ���ٶ� (ͨ���ȼ��ٶȴ��Ա����ɲ��)
    public float detectionDistance = 20f; // ��ǰ����������
    public float safeDistance = 10f;   // ��ǰ�����ֵİ�ȫ����
    public LayerMask vehicleLayerMask; // ֻ��⳵����
    public float sensorHeightOffset = 0.5f; // ��������������㣩�ĸ߶�ƫ��
    public float sensorForwardOffset = 1.5f; // ��������������㣩��ǰ��ƫ�� (���ڳ�������ǰ��)
    public float sensorRadius = 0.8f;    // ��ⷶΧ�İ뾶 (ʹ��SphereCast)
    public float destroyXCoordinate;   // ���ٱ߽�

    // --- �ڲ�״̬ ---
    private float currentSpeed;       // ��ǰʵ���ٶ�
    private bool movingRight;         // �ƶ�������


    void Start()
    {
        // ��ʼ�ٶȿ�����Ϊ��С�ٶȻ�0
        currentSpeed = minSpeed;
        movingRight = moveDirection.x > 0;

        // ȷ����ȫ����С�ڼ�����
        safeDistance = Mathf.Min(safeDistance, detectionDistance);
    }

    void Update()
    {
        // 1. ��֪ǰ��
        float targetSpeed = maxSpeed; // Ĭ��Ŀ��������ٶ�
        bool obstacleDetected = false;
        float distanceToObstacle = detectionDistance;

        // ���㴫�������λ�� (��΢̧�߲��ڳ�ͷǰ��һ��)
        // ʹ�� transform.TransformPoint ������ƫ��ת��Ϊ��������
        // Vector3 sensorStartPos = transform.TransformPoint(new Vector3(0, sensorHeightOffset, sensorForwardOffset));
        // ���ߣ��������û�и�����ת������ֱ������������ƫ�ƣ�
        Vector3 sensorStartPos = transform.position + Vector3.up * sensorHeightOffset + moveDirection * sensorForwardOffset;


        // ִ������Ͷ����
        RaycastHit hit;
        // Physics.SphereCast(���, �뾶, ����, out hitInfo, ������, ����)
        if (Physics.SphereCast(sensorStartPos, sensorRadius, moveDirection, out hit, detectionDistance - sensorForwardOffset, vehicleLayerMask))
        {
            // ��⵽�˶��� (ֻ�ڳ�������)
            // ȷ�����Ǽ�⵽�Լ� (��Ȼ SphereCast ͨ������)
            if (hit.transform != transform)
            {
                obstacleDetected = true;
                distanceToObstacle = hit.distance; // ���Ǵ�Sensor��㵽��ײ��ľ���

                // 2. ���ݾ������Ŀ���ٶ�
                if (distanceToObstacle < safeDistance)
                {
                    // ����̫������Ҫ����
                    // ����ƥ��ǰ���ٶ� (����ܻ�ȡ��)
                    VehicleMovement carAhead = hit.collider.GetComponent<VehicleMovement>();
                    if (carAhead != null)
                    {
                        // Ŀ���ٶ���ǰ���ĵ�ǰ�ٶȣ������ܵ����Լ�������ٶ�
                        targetSpeed = Mathf.Max(minSpeed, carAhead.currentSpeed - 1.0f); // ��΢��һ����Ա��־���
                    }
                    else
                    {
                        // �����ȡ����ǰ���ű�������ֻ����򵥴������ʹ������
                        // ���Ը��ݾ�����������٣�����Խ���ٶ�Խ��
                        float speedRatio = Mathf.Clamp01(distanceToObstacle / safeDistance); // 0=�Ӵ�, 1=�պ��ڰ�ȫ����
                        targetSpeed = Mathf.Lerp(minSpeed, maxSpeed, speedRatio * speedRatio); // ʹ��ƽ���ü��ٸ�����
                        // ����ֱ����Ϊ����ٶ�
                        // targetSpeed = minSpeed;
                    }
                }
                // else: �ڰ�ȫ����֮�⣬���ڼ�ⷶΧ�ڣ����԰�����ٶ���ʻ (Ĭ��targetSpeed=maxSpeed)
            }
        }
        // else: ǰ����ⷶΧ�����ϰ�������Ŀ��Ϊ����ٶ�

        // 3. ƽ��������ǰ�ٶ�
        float accel = (targetSpeed > currentSpeed) ? acceleration : deceleration;
        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, accel * Time.deltaTime);
        currentSpeed = Mathf.Clamp(currentSpeed, minSpeed, maxSpeed); // ȷ���ٶ���������Χ��

        // 4. Ӧ���ƶ�
        transform.Translate(moveDirection * currentSpeed * Time.deltaTime, Space.World);

        // 5. ������ٱ߽�
        CheckDestroyBoundary();

        // (��ѡ) ���Ի��Ƽ������
        Debug.DrawRay(sensorStartPos, moveDirection * (obstacleDetected ? distanceToObstacle : detectionDistance - sensorForwardOffset), obstacleDetected ? Color.red : Color.green);
    }

    void CheckDestroyBoundary()
    {
        if ((movingRight && transform.position.x >= destroyXCoordinate) ||
            (!movingRight && transform.position.x <= destroyXCoordinate))
        {
            Destroy(gameObject);
        }
    }

    // ��ʼ��������Ҫ���ո������
    public void Initialize(Vector3 direction, float vehicleMaxSpeed, float vehicleMinSpeed, float accel, float decel, float detectDist, float safeDist, LayerMask layerMask, float destroyX, float sensorHOffset, float sensorFOffset, float sensRadius)
    {
        moveDirection = direction.normalized;
        maxSpeed = vehicleMaxSpeed;
        minSpeed = vehicleMinSpeed;
        acceleration = accel;
        deceleration = decel;
        detectionDistance = detectDist;
        safeDistance = safeDist;
        vehicleLayerMask = layerMask;
        destroyXCoordinate = destroyX;
        sensorHeightOffset = sensorHOffset;
        sensorForwardOffset = sensorFOffset;
        sensorRadius = sensRadius;

        // �ٴ����� movingRight ȷ����ȷ
        movingRight = moveDirection.x > 0;
        // ��������������һ������ĳ�ʼ�ٶȣ�
        currentSpeed = Random.Range(minSpeed, (minSpeed + maxSpeed) / 2f); // ���磬��ʼ�ٶ�����ͺ�ƽ��֮��
    }
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("触发器接触对象: " + other.name);
        
        if (other.CompareTag("Player"))
        {
            CatHealth health = other.GetComponent<CatHealth>();
            if (health != null)
            {
                health.AddHealth(-10f);
                Debug.Log("通过触发器扣除生命值");
            }
        }
    }
}