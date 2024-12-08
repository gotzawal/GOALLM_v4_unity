using UnityEngine;

public class LookAt : MonoBehaviour
{
    [Header("Transforms")]
    public Transform head; // 캐릭터의 머리 Transform
    public Transform neck; // 캐릭터의 목 Transform
    public Transform lookAtTarget; // 카메라나 타겟 Transform

    [Header("Settings")]
    public float rotationSpeed = 5f; // 회전 속도
    public float maxHeadTurnAngle = 60f; // 머리 수평 회전 최대 각도
    public float maxHeadTiltAngle = 20f; // 머리 수직 기울기 최대 각도 (값 축소)
    public float neckContribution = 0.5f; // 목이 머리 각도의 몇 %를 따라갈지 설정
    public float neutralHeadTilt = 10f; // 기본적으로 머리가 약간 숙여진 각도

    // 현재 머리의 회전 각도
    private float currentHeadYaw = 0f; // 머리의 수평 회전 (좌우)
    private float currentHeadPitch = 0f; // 머리의 수직 회전 (위아래)

    // 현재 목의 회전 각도
    private float currentNeckYaw = 0f; // 목의 수평 회전 (좌우)
    private float currentNeckPitch = 0f; // 목의 수직 회전 (위아래)

    void LateUpdate()
    {
        if (head == null || neck == null || lookAtTarget == null)
            return;

        // 타겟까지의 방향 벡터 계산
        Vector3 directionToTarget = lookAtTarget.position - head.position;

        // 머리의 로컬 좌표계에서의 방향 계산
        Vector3 localDirection = head.InverseTransformDirection(directionToTarget);

        // 수평 및 수직 각도 계산
        float targetYaw = Mathf.Atan2(localDirection.x, localDirection.z) * Mathf.Rad2Deg; // 좌우(Yaw) 각도
        float horizontalDistance = new Vector2(localDirection.x, localDirection.z).magnitude; // XZ 평면 거리
        float targetPitch = Mathf.Atan2(localDirection.y, horizontalDistance) * Mathf.Rad2Deg; // 위아래(Pitch) 각도

        // 각도 제한 적용
        targetYaw = Mathf.Clamp(targetYaw, -maxHeadTurnAngle, maxHeadTurnAngle);

        // 머리의 수직 각도를 기본 숙임(neutralHeadTilt) 기준으로 조정
        targetPitch = Mathf.Clamp(targetPitch, -maxHeadTiltAngle, maxHeadTiltAngle) + neutralHeadTilt;

        // 머리의 목표 각도로 부드럽게 보간
        currentHeadYaw = Mathf.LerpAngle(currentHeadYaw, targetYaw, Time.deltaTime * rotationSpeed);
        currentHeadPitch = Mathf.LerpAngle(currentHeadPitch, targetPitch, Time.deltaTime * rotationSpeed);

        // 목의 각도는 머리 각도의 일부만 따라가도록 설정
        currentNeckYaw = Mathf.LerpAngle(currentNeckYaw, currentHeadYaw * neckContribution, Time.deltaTime * rotationSpeed);
        currentNeckPitch = Mathf.LerpAngle(currentNeckPitch, currentHeadPitch * neckContribution, Time.deltaTime * rotationSpeed);

        // 머리와 목 회전 적용
        neck.localRotation = Quaternion.Euler(currentNeckPitch, currentNeckYaw, 0f);
        head.localRotation = Quaternion.Euler(currentHeadPitch - currentNeckPitch, currentHeadYaw - currentNeckYaw, 0f);
    }
}