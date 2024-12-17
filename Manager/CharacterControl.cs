
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CharacterControl : MonoBehaviour
{
    public string motionType = "Standing Idle"; // 초기 상태를 'Standing Idle'로 설정

    public Animator animator;
    private Inertializer inertializer;
    private Vector3 rootMotionDeltaPosition;
    private Quaternion rootMotionDeltaRotation;

    public Vector3 targetPoint = Vector3.zero; // 이동 목표 지점

    // 유효한 제스처 목록
    private List<string> validGestures = new List<string>
    {
        "Bashful",
        "Happy",
        "Crying",
        "Thinking",
        "Talking",
        "Looking",
        "No",
        "Fist Pump",
        "Agreeing",
        "Arguing",
        "Thankful",
        "Excited",
        "Clapping",
        "Rejected",
        "Look Around" // Ensure Animator has triggers with these exact names
    };
    // NavMeshAgent
    private NavMeshAgent navAgent;

    // 이동 상태를 외부에서 확인할 수 있도록 public 프로퍼티 추가
    public bool IsMoving
    {
        get
        {
            if (navAgent == null)
                return false;
            bool moving = navAgent.pathPending || navAgent.remainingDistance > navAgent.stoppingDistance;
            //Debug.Log($"IsMoving: {moving}, RemainingDistance: {navAgent.remainingDistance}");
            return moving;
        }
    }

    // 이동 목표 지점 설정 메서드
    public void SetDestination(Vector3 destination)
    {
        if (navAgent != null)
        {
            navAgent.stoppingDistance = 0.2f; // 원하는 거리로 설정 (필요에 따라 조정)
            navAgent.SetDestination(destination);
            navAgent.updatePosition = false; // 루트 모션 사용 시, NavMeshAgent의 위치 업데이트를 비활성화
            navAgent.updateRotation = false;
            targetPoint = destination;
            Debug.Log($"SetDestination: {destination}");
        }
        else
        {
            Debug.LogWarning("CharacterControl: NavMeshAgent 컴포넌트가 없습니다.");
        }
    }

    void Start()
    {
        inertializer = GetComponent<Inertializer>();
        if (inertializer != null)
        {
            inertializer.InitializeInertializer();
        }
        else
        {
            Debug.LogWarning("CharacterControl: Inertializer 컴포넌트가 없습니다.");
        }

        // NavMeshAgent 초기화
        navAgent = GetComponent<NavMeshAgent>();
        if (navAgent == null)
        {
            Debug.LogError("CharacterControl: NavMeshAgent 컴포넌트가 없습니다.");
        }

        UpdateMotionType("Standing Idle"); // 초기 상태 설정
    }

    void Update()
    {
        if (navAgent != null)
        {
            if (motionType != "Gesture") // Gesture 상태에서는 motionType 변경을 방지
            {
                if (IsMoving)
                {
                    if (motionType != "walk")
                    {
                        motionType = "walk";
                        UpdateMotionType("walk");
                        Debug.Log("CharacterControl: Switching to walk.");
                    }
                }
                else
                {
                    if (motionType != "Standing Idle")
                    {
                        motionType = "Standing Idle";
                        UpdateMotionType("Standing Idle");
                        Debug.Log("CharacterControl: Switching to Standing Idle.");
                    }
                }
            }
        }
    }


    void LateUpdate()
    {
        ApplyRootMotion();
        if (navAgent != null && !navAgent.updatePosition)
        {
            navAgent.nextPosition = transform.position; // NavMeshAgent의 위치를 캐릭터의 실제 위치와 동기화
        }
    }

    public void PlayClipFromFrame(AnimationClip clip, float frame)
    {
        if (clip != null && inertializer != null)
        {
            float normalizedTime = frame / clip.frameRate / clip.length;
            inertializer.InertializedTransition(clip, normalizedTime);
        }
    }

    public void PlayClipFromTime(AnimationClip clip, float startTime)
    {
        if (clip != null && inertializer != null)
        {
            float normalizedTime = startTime / clip.length;
            inertializer.InertializedTransition(clip, normalizedTime);
        }
    }

    public AnimationClip FindAnimationClip(string clipName)
    {
        if (animator == null)
        {
            Debug.LogError("CharacterControl: Animator 컴포넌트가 없습니다.");
            return null;
        }

        foreach (var clip in animator.runtimeAnimatorController.animationClips)
        {
            if (clip.name.Equals(clipName, StringComparison.OrdinalIgnoreCase))
            {
                return clip;
            }
        }
        Debug.LogWarning($"CharacterControl: Animation clip '{clipName}' not found in the Animator's RuntimeAnimatorController.");
        return null;
    }
    private string previousMotionType;

    public void PerformGesture(string gestureName)
    {
        if (string.IsNullOrEmpty(gestureName))
        {
            Debug.LogError("CharacterControl: Gesture name is null or empty.");
            return;
        }

        string normalizedGesture = gestureName.ToLower();

        if (validGestures.Contains(CapitalizeFirstLetter(normalizedGesture)))
        {
            // 현재 motionType 저장
            previousMotionType = motionType;

            // 제스처 애니메이션 트리거
            animator.SetTrigger(CapitalizeFirstLetter(normalizedGesture));

            Debug.Log($"CharacterControl: Performing gesture '{normalizedGesture}'. Previous motionType: '{previousMotionType}'.");

            // 제스처 수행 중 motionType을 "Gesture"로 설정하여 Update에서 변경되지 않도록 함
            UpdateMotionType("Gesture");

            // 제스처 애니메이션이 끝난 후 motionType을 이전 상태로 복원
            StartCoroutine(WaitForGestureAnimation(normalizedGesture));
        }
        else
        {
            Debug.LogError($"CharacterControl: Invalid gesture '{CapitalizeFirstLetter(normalizedGesture)}'.");
        }
    }

    private IEnumerator WaitForGestureAnimation(string gestureName)
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        // 제스처 애니메이션 상태로 진입할 때까지 대기
        while (!stateInfo.IsName(CapitalizeFirstLetter(gestureName)))
        {
            yield return null;
            stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        }

        // 제스처 애니메이션이 끝날 때까지 대기
        while (stateInfo.normalizedTime < 1.0f)
        {
            yield return null;
            stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        }

        // 이전 motionType으로 복원
        UpdateMotionType(previousMotionType);
        Debug.Log($"CharacterControl: Gesture '{gestureName}' completed. Restored motionType to '{previousMotionType}'.");
    }

    // motionType에 따라 애니메이션 트리거 설정 메서드
    private void UpdateMotionType(string newMotionType)
    {
        Debug.Log($"CharacterControl: Updating motionType to '{newMotionType}'.");

        switch (newMotionType)
        {
            case "walk":
                animator.SetTrigger("walk");
                animator.applyRootMotion = true; // 루트 모션 활성화
                break;
            case "Standing Idle":
                animator.SetTrigger("Standing_Idle");
                animator.applyRootMotion = false; // 루트 모션 비활성화
                break;
            // 추가적인 motionType에 대한 케이스를 여기에 추가
            default:
                Debug.LogWarning($"CharacterControl: Unknown motionType '{newMotionType}'.");
                break;
        }
    }

    // 첫 글자를 대문자로 변환하는 도우미 메서드
    private string CapitalizeFirstLetter(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;
        return char.ToUpper(input[0]) + input.Substring(1);
    }

    // Root Motion 적용
    void OnAnimatorMove()
    {
        if (animator)
        {
            if (motionType == "walk")
            {
                rootMotionDeltaPosition = animator.deltaPosition;
                rootMotionDeltaRotation = animator.deltaRotation;
            }
            else
            {
                rootMotionDeltaPosition = Vector3.zero;
                rootMotionDeltaRotation = Quaternion.identity;
            }
        }
    }

    void ApplyRootMotion()
    {
        if (motionType == "walk")
        {
            transform.position += rootMotionDeltaPosition;
            transform.rotation = rootMotionDeltaRotation * transform.rotation;
        }
    }
}
