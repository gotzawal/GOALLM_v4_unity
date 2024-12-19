using UnityEngine;
using System.Collections;

public class RhythmManager : MonoBehaviour
{
    public static RhythmManager instance;
    public GOAPManager goapManager;
    public Animator characterAnimator;

    [Header("Mode Settings")]
    public string CurrentMode = "automode"; // Default mode
    public float inactivityTimeout = 25f;
    public float autonomousDecisionIntervalMin = 10f;
    public float autonomousDecisionIntervalMax = 15f;

    [HideInInspector]
    public bool IsCommunicatingWithServer = false;

    // Coroutines
    private Coroutine inactivityCoroutine;
    private Coroutine autonomousModeDelayCoroutine;

    // Distance Detection
    public Transform playerTransform;
    public Transform npcTransform;
    public float talkModeDistance = 1.5f;

    private bool isInTalkModeByProximity = false;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("RhythmManager: Duplicate instance found. Destroying...");
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (playerTransform == null)
            Debug.LogError("RhythmManager: Player Transform is not assigned.");

        if (npcTransform == null)
            Debug.LogError("RhythmManager: NPC Transform is not assigned.");

        // 초기 상태가 automode라면 autonomous delay 시작
        if (CurrentMode == "auto" || CurrentMode == "automode")
        {
            ResetAutonomousModeDelay();
        }
    }

    private void Update()
    {
        HandleProximityDetection();
    }

    /// <summary>
    /// 플레이어와 NPC 사이 거리를 감지해서 Talk Mode 전환
    /// </summary>
    private void HandleProximityDetection()
    {
        if (playerTransform == null || npcTransform == null)
            return;

        float distance = Vector3.Distance(playerTransform.position, npcTransform.position);

        // 플레이어가 NPC 근처에 접근 시
        if (distance <= talkModeDistance)
        {
            if (!isInTalkModeByProximity && !IsCommunicatingWithServer && CurrentMode == "automode")
            {
                SwitchToTalkMode();
                isInTalkModeByProximity = true;
                TriggerTalkModeByProximity();
            }
        }
        else
        {
            if (isInTalkModeByProximity)
                isInTalkModeByProximity = false;
        }
    }

    /// <summary>
    /// 플레이어가 근접했을 때 Talk Mode를 트리거
    /// </summary>
    private void TriggerTalkModeByProximity()
    {
        if (GameManager.instance != null && !IsCommunicatingWithServer)
        {
            GameManager.instance.SendEmptyInput("Player get close to npc.");
            IsCommunicatingWithServer = true;
            ResetInactivityTimer();
            // Talk mode로 전환 시 AutonomousModeDelay는 필요없으니 리셋(=정지)
            ResetAutonomousModeDelay();
        }
    }

    /// <summary>
    /// 서버 응답 처리
    /// </summary>
    public void HandleServerResponse(GameManager.ServerResponse response)
    {
        if (response == null)
        {
            IsCommunicatingWithServer = false;
            return;
        }

        // NPC 표정 업데이트
        if (!string.IsNullOrEmpty(response.Expression))
            characterAnimator.SetTrigger(response.Expression);

        // GOAP 목표 업데이트
        if (goapManager != null)
            goapManager.SetGoals(response.Gesture, response.MoveGoal, response.ItemGoal, response.ActionGoal);

        // 응답 유지 여부 확인
        if (!string.IsNullOrEmpty(response.Maintain))
        {
            // Maintain이 "yes"면 Talk Mode 유지
            if (response.Maintain.ToLower() == "yes")
            {
                SwitchToTalkMode();
                ResetInactivityTimer();
                // Talk Mode이므로 autonomous delay를 리셋할 때 재시작하지 않음(실제로는 Stop기능)
                ResetAutonomousModeDelay();
            }
            else
            {
                // Maintain이 "no"면 Autonomous Mode 전환
                SwitchToAutonomousMode();
                ResetInactivityTimer();
                // Autonomous Mode이므로 autonomous delay 리셋 후 다시 시작
                ResetAutonomousModeDelay();
            }
        }

        IsCommunicatingWithServer = false;
    }

    /// <summary>
    /// AutonomousModeDelay 타이머를 정지하고, 현재 모드에 따라 재시작
    /// CurrentMode가 automode일 때만 AutonomousModeDelay 재시작
    /// talkmode일 때는 재시작하지 않고 그냥 정지 상태 유지.
    /// </summary>
    private void ResetAutonomousModeDelay()
    {
        StopAutonomousModeDelay();
        if (CurrentMode == "automode")
        {
            autonomousModeDelayCoroutine = StartCoroutine(AutonomousModeDelay(autonomousDecisionIntervalMin, autonomousDecisionIntervalMax));
        }
    }

    /// <summary>
    /// AutonomousModeDelay 코루틴 중지 메서드
    /// </summary>
    private void StopAutonomousModeDelay()
    {
        if (autonomousModeDelayCoroutine != null)
        {
            StopCoroutine(autonomousModeDelayCoroutine);
            autonomousModeDelayCoroutine = null;
        }
    }

    /// <summary>
    /// 일정 시간 후 Autonomous Mode 동작
    /// </summary>
    IEnumerator AutonomousModeDelay(float minDelay, float maxDelay)
    {
        float waitTime = UnityEngine.Random.Range(minDelay, maxDelay);
        yield return new WaitForSeconds(waitTime);

        if (!IsCommunicatingWithServer && GameManager.instance != null && CurrentMode == "automode")
        {
            GameManager.instance.SendEmptyInput("NPC need next goal and consider talk to player.");
            IsCommunicatingWithServer = true;
            ResetInactivityTimer();
            ResetAutonomousModeDelay();
        }
    }

    /// <summary>
    /// Talk Mode로 전환
    /// </summary>
    private void SwitchToTalkMode()
    {
        if (CurrentMode != "talkmode")
        {
            CurrentMode = "talkmode";
            ResetInactivityTimer();
            // Talk Mode에서는 AutonomousDelay 필요 없으니 리셋 -> 정지 상태로
            ResetAutonomousModeDelay();
        }
    }

    /// <summary>
    /// Autonomous Mode로 전환
    /// </summary>
    public void SwitchToAutonomousMode()
    {
        if (CurrentMode != "automode")
        {
            CurrentMode = "automode";
            ResetInactivityTimer();
            ResetAutonomousModeDelay(); // 모드 변경 후 autonomous mode delay 재시작
        }
    }

    /// <summary>
    /// Inactivity Timer 리셋
    /// </summary>
    public void ResetInactivityTimer()
    {
        if (inactivityCoroutine != null)
            StopCoroutine(inactivityCoroutine);

        inactivityCoroutine = StartCoroutine(InactivityTimer());
    }

    /// <summary>
    /// 일정 시간 대기 후 응답 없으면 Autonomous Mode로 전환
    /// </summary>
    IEnumerator InactivityTimer()
    {
        yield return new WaitForSeconds(inactivityTimeout);

        // 플레이어가 답변 없는 경우 Autonomous Mode로 전환
        SwitchToAutonomousMode();

        if (!IsCommunicatingWithServer && GameManager.instance != null)
        {
            GameManager.instance.SendEmptyInput("Player don't answer to npc talk.");
            IsCommunicatingWithServer = true;
            ResetInactivityTimer();
            // 모드 전환했으니 autonomous delay도 리셋하여 재시작
            ResetAutonomousModeDelay();
        }
    }

    /// <summary>
    /// 플레이어가 대화를 시작할 때 호출
    /// </summary>
    public void StartConversation()
    {
        if (CurrentMode != "automode")
        {
            SwitchToTalkMode();
            ResetInactivityTimer();
            // Talk Mode 진입 시 autonomous delay는 중단
            ResetAutonomousModeDelay();
        }
    }

    /// <summary>
    /// Talk Mode로 리셋
    /// </summary>
    public void ResetToTalkMode()
    {
        SwitchToTalkMode();
        ResetInactivityTimer();
        // Talk Mode 유지 시 autonomous delay는 다시 중단
        ResetAutonomousModeDelay();
    }
}
