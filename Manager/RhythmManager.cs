using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class RhythmManager : MonoBehaviour
{
    public static RhythmManager instance;
    public GOAPManager goapManager;
    public Animator characterAnimator;

    [Header("Mode Settings")]
    public string CurrentMode = "autonomousmode"; // Default mode
    public float inactivityTimeout = 15f; // Time in seconds to switch to autonomous mode
    public float autonomousDecisionIntervalMin = 10f; // Changed to 10 seconds
    public float autonomousDecisionIntervalMax = 15f; // Changed to 15 seconds

    [HideInInspector]
    public bool IsCommunicatingWithServer = false;

    private Coroutine inactivityCoroutine;

    // 거리 판별을 위한 필드
    public Transform playerTransform; // Assign via Inspector
    public Transform npcTransform;    // Assign via Inspector
    public float talkModeDistance = 1.5f; // 거리 임계값

    private bool isInTalkModeByProximity = false;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("RhythmManager: Instance already exists. Destroying duplicate.");
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        Debug.Log("RhythmManager: Instance has been set.");
    }

    private void Start()
    {
        // 초기 모드가 'autonomousmode'인 경우 추가 초기화가 필요 없다면 호출을 제거합니다.
        // 현재 요구사항에 따라, 초기 모드 설정은 이미 Done 되었습니다.

        // Validate player and NPC Transforms
        if (playerTransform == null)
        {
            Debug.LogError("RhythmManager: Player Transform is not assigned.");
        }
        else
        {
            Debug.Log("RhythmManager: Player Transform is assigned.");
        }

        if (npcTransform == null)
        {
            Debug.LogError("RhythmManager: NPC Transform is not assigned.");
        }
        else
        {
            Debug.Log("RhythmManager: NPC Transform is assigned.");
        }

        StartCoroutine(AutonomousModeDelay(autonomousDecisionIntervalMin, autonomousDecisionIntervalMax));
    }


    private void Update()
    {
        HandleProximityDetection();
    }

    /// <summary>
    /// Handles proximity detection between Player and NPC.
    /// </summary>
    private void HandleProximityDetection()
    {
        if (playerTransform == null || npcTransform == null)
            return;

        float distance = Vector3.Distance(playerTransform.position, npcTransform.position);

        if (distance <= talkModeDistance)
        {
            if (!isInTalkModeByProximity && !IsCommunicatingWithServer && CurrentMode != "talkmode")
            {
                SwitchToTalkMode();
                isInTalkModeByProximity = true;
                Debug.Log("RhythmManager: Player is within talk mode distance. Triggering Talk Mode by proximity.");
                TriggerTalkModeByProximity();
            }
        }
        else
        {
            if (isInTalkModeByProximity)
            {
                isInTalkModeByProximity = false;
                Debug.Log("RhythmManager: Player moved out of talk mode distance.");
            }
        }
    }

    /// <summary>
    /// Triggers Talk Mode by proximity by informing GameManager.
    /// </summary>
    private void TriggerTalkModeByProximity()
    {
        if (GameManager.instance != null)
        {
            Debug.Log("RhythmManager: Calling GameManager.TriggerTalkModeByProximity().");
            GameManager.instance.SendEmptyInput();
        }
        else
        {
            Debug.LogError("RhythmManager: GameManager instance is not found.");
        }
    }

    /// <summary>
    /// Handles the server response by updating NPC's state and mode.
    /// </summary>
    /// <param name="response">The server response.</param>
    public void HandleServerResponse(GameManager.ServerResponse response)
    {
        if (response == null)
        {
            Debug.LogError("RhythmManager: Received null response.");
            return;
        }

        // Update NPC Expression
        if (!string.IsNullOrEmpty(response.Expression))
        {
            characterAnimator.SetTrigger(response.Expression);
            Debug.Log("RhythmManager: Updated NPC Expression.");
        }

        // Update GOAP Goals
        if (goapManager != null)
        {
            goapManager.SetGoals(response.Gesture, response.MoveGoal, response.ItemGoal, response.ActionGoal);
            Debug.Log("RhythmManager: Updated GOAP Goals.");
        }
        else
        {
            Debug.LogError("RhythmManager: GOAPManager reference is not set.");
        }

        // Manage Mode based on server's 'Maintain' field
        if (!string.IsNullOrEmpty(response.Maintain))
        {
            if (response.Maintain.ToLower() == "yes")
            {
                Debug.Log("RhythmManager: Server maintain is 'yes'. Staying in Talk Mode.");
                // Optionally, reset inactivity timer if needed
                ResetInactivityTimer();
            }
            else
            {
                Debug.Log("RhythmManager: Server maintain is 'no'. Scheduling switch to Autonomous Mode.");
                StartCoroutine(AutonomousModeDelay(autonomousDecisionIntervalMin, autonomousDecisionIntervalMax));
            }
        }

        ResetInactivityTimer();


    }

    /// <summary>
    /// Coroutine to switch to Autonomous Mode after a random delay between minDelay and maxDelay seconds.
    /// </summary>
    /// <param name="minDelay">Minimum delay in seconds.</param>
    /// <param name="maxDelay">Maximum delay in seconds.</param>
    /// <returns></returns>
    IEnumerator AutonomousModeDelay(float minDelay, float maxDelay)
    {
        float waitTime = UnityEngine.Random.Range(minDelay, maxDelay);
        Debug.Log($"RhythmManager: Waiting for {waitTime} seconds during Autonomous Mode.");
        yield return new WaitForSeconds(waitTime);


        // Send empty input to server after switching to Autonomous Mode
        if (!IsCommunicatingWithServer && GameManager.instance != null)
        {
            Debug.Log("RhythmManager: Sending empty input to server after switching to Autonomous Mode.");
            GameManager.instance.SendEmptyInput(); // This sends empty input
        }
        else
        {
            Debug.LogWarning("RhythmManager: Cannot send empty input to server. Either already communicating or GameManager.instance is null.");
        }
    }

    /// <summary>
    /// Switches the NPC to Talk Mode.
    /// </summary>
    private void SwitchToTalkMode()
    {
        if (CurrentMode != "talkmode")
        {
            CurrentMode = "talkmode";
            Debug.Log("RhythmManager: Switched to Talk Mode.");

            // Reset inactivity timer
            ResetInactivityTimer();
        }
    }

    /// <summary>
    /// Switches the NPC to Autonomous Mode.
    /// </summary>
    public void SwitchToAutonomousMode()
    {
        if (CurrentMode != "autonomousmode")
        {
            CurrentMode = "autonomousmode";
            Debug.Log("RhythmManager: Switched to Autonomous Mode.");

            // Autonomous Mode transitions are now driven by server responses or proximity
        }
    }

    /// <summary>
    /// Resets the inactivity timer to maintain talk mode.
    /// </summary>
    public void ResetInactivityTimer()
    {
        if (inactivityCoroutine != null)
        {
            StopCoroutine(inactivityCoroutine);
            Debug.Log("RhythmManager: Stopped existing inactivity timer coroutine.");
        }
        inactivityCoroutine = StartCoroutine(InactivityTimer());
        Debug.Log("RhythmManager: Started new inactivity timer coroutine.");
    }

    /// <summary>
    /// Coroutine that waits for inactivityTimeout seconds before switching to autonomous mode.
    /// </summary>
    /// <returns></returns>
    IEnumerator InactivityTimer()
    {
        Debug.Log($"RhythmManager: Inactivity timer started for {inactivityTimeout} seconds.");
        yield return new WaitForSeconds(inactivityTimeout);
        Debug.Log("RhythmManager: Inactivity timeout reached. Switching to Autonomous Mode.");
        SwitchToAutonomousMode();

        // Send empty input to server after switching to Autonomous Mode
        if (!IsCommunicatingWithServer && GameManager.instance != null)
        {
            Debug.Log("RhythmManager: Sending empty input to server after switching to Autonomous Mode.");
            GameManager.instance.SendEmptyInput(); // This sends empty input
        }
        else
        {
            Debug.LogWarning("RhythmManager: Cannot send empty input to server. Either already communicating or GameManager.instance is null.");
        }
    }

    /// <summary>
    /// Resets the mode to Talk Mode and stops autonomous behavior.
    /// </summary>
    public void ResetToTalkMode()
    {
        if (CurrentMode != "talkmode")
        {
            SwitchToTalkMode();
        }

        // Reset inactivity timer
        ResetInactivityTimer();
        Debug.Log("RhythmManager: Reset inactivity timer during ResetToTalkMode.");
    }
}
