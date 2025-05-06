using UnityEngine;

public class VehicleMovement : MonoBehaviour
{
    // --- 由 Spawner 设置 ---
    public Vector3 moveDirection;      // 移动方向
    public float minSpeed = 5f;        // 最小速度
    public float maxSpeed = 15f;       // 最大期望速度 (每辆车可以不同)
    public float acceleration = 3f;    // 加速度
    public float deceleration = 6f;    // 减速度 (通常比加速度大，以便更快刹车)
    public float detectionDistance = 20f; // 向前检测的最大距离
    public float safeDistance = 10f;   // 与前车保持的安全距离
    public LayerMask vehicleLayerMask; // 只检测车辆层
    public float sensorHeightOffset = 0.5f; // 传感器（射线起点）的高度偏移
    public float sensorForwardOffset = 1.5f; // 传感器（射线起点）的前向偏移 (基于车辆自身前方)
    public float sensorRadius = 0.8f;    // 检测范围的半径 (使用SphereCast)
    public float destroyXCoordinate;   // 销毁边界

    // --- 内部状态 ---
    private float currentSpeed;       // 当前实际速度
    private bool movingRight;         // 移动方向标记

    void Start()
    {
        // 初始速度可以设为最小速度或0
        currentSpeed = minSpeed;
        movingRight = moveDirection.x > 0;

        // 确保安全距离小于检测距离
        safeDistance = Mathf.Min(safeDistance, detectionDistance);
    }

    void Update()
    {
        // 1. 感知前方
        float targetSpeed = maxSpeed; // 默认目标是最大速度
        bool obstacleDetected = false;
        float distanceToObstacle = detectionDistance;

        // 计算传感器起点位置 (稍微抬高并在车头前方一点)
        // 使用 transform.TransformPoint 将本地偏移转换为世界坐标
        // Vector3 sensorStartPos = transform.TransformPoint(new Vector3(0, sensorHeightOffset, sensorForwardOffset));
        // 或者，如果物体没有复杂旋转，可以直接用世界坐标偏移：
        Vector3 sensorStartPos = transform.position + Vector3.up * sensorHeightOffset + moveDirection * sensorForwardOffset;


        // 执行球形投射检测
        RaycastHit hit;
        // Physics.SphereCast(起点, 半径, 方向, out hitInfo, 最大距离, 检测层)
        if (Physics.SphereCast(sensorStartPos, sensorRadius, moveDirection, out hit, detectionDistance - sensorForwardOffset, vehicleLayerMask))
        {
            // 检测到了东西 (只在车辆层检测)
            // 确保不是检测到自己 (虽然 SphereCast 通常不会)
            if (hit.transform != transform)
            {
                obstacleDetected = true;
                distanceToObstacle = hit.distance; // 这是从Sensor起点到碰撞点的距离

                // 2. 根据距离调整目标速度
                if (distanceToObstacle < safeDistance)
                {
                    // 距离太近，需要减速
                    // 尝试匹配前车速度 (如果能获取到)
                    VehicleMovement carAhead = hit.collider.GetComponent<VehicleMovement>();
                    if (carAhead != null)
                    {
                        // 目标速度是前车的当前速度，但不能低于自己的最低速度
                        targetSpeed = Mathf.Max(minSpeed, carAhead.currentSpeed - 1.0f); // 稍微慢一点点以保持距离
                    }
                    else
                    {
                        // 如果获取不到前车脚本，或者只是想简单处理，就大幅减速
                        // 可以根据距离比例来减速，距离越近速度越低
                        float speedRatio = Mathf.Clamp01(distanceToObstacle / safeDistance); // 0=接触, 1=刚好在安全距离
                        targetSpeed = Mathf.Lerp(minSpeed, maxSpeed, speedRatio * speedRatio); // 使用平方让减速更明显
                        // 或者直接设为最低速度
                        // targetSpeed = minSpeed;
                    }
                }
                // else: 在安全距离之外，但在检测范围内，可以按最大速度行驶 (默认targetSpeed=maxSpeed)
            }
        }
        // else: 前方检测范围内无障碍，保持目标为最大速度

        // 3. 平滑调整当前速度
        float accel = (targetSpeed > currentSpeed) ? acceleration : deceleration;
        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, accel * Time.deltaTime);
        currentSpeed = Mathf.Clamp(currentSpeed, minSpeed, maxSpeed); // 确保速度在允许范围内

        // 4. 应用移动
        transform.Translate(moveDirection * currentSpeed * Time.deltaTime, Space.World);

        // 5. 检查销毁边界
        CheckDestroyBoundary();

        // (可选) 调试绘制检测射线
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

    // 初始化方法需要接收更多参数
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

        // 再次设置 movingRight 确保正确
        movingRight = moveDirection.x > 0;
        // 可以在这里设置一个随机的初始速度？
        currentSpeed = Random.Range(minSpeed, (minSpeed + maxSpeed) / 2f); // 例如，初始速度在最低和平均之间
    }
}