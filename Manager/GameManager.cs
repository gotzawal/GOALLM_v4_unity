using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.Networking;
using System;
using System.Collections.Generic;
using Newtonsoft.Json; // Ensure Newtonsoft.Json is properly referenced
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public InputField userInputField; // Assign via Inspector
    public GOAPManager goapManager;
    public RhythmManager rhythmManager;

    // 서버 URL
    private string serverUrl = "https://e157-35-197-71-62.ngrok-free.app"; // 실제 서버 URL로 변경하세요
    private bool isServerUrlSet = true; // 서버 URL 입력을 건너뛰려면 true로 설정

    private string clientId; // client_id 저장

    [System.Serializable]
    public class ServerRequest
    {
        public string client_id;
        public NPCStatus npc_status;
        public string userInput;
        public WorldStatus world_status; // WorldStatus 필드 추가
        public string npc_mode; // Mode field

        public ServerRequest(string clientId, NPCStatus status, string input, WorldStatus worldStatus, string mode)
        {
            this.client_id = clientId;
            this.npc_status = status;
            this.userInput = input;
            this.world_status = worldStatus;
            this.npc_mode = mode;
        }
    }

    [System.Serializable]
    public class ServerResponse // Changed from private to public
    {
        public string client_id;
        public string audio_file; // Base64로 인코딩된 WAV 오디오 데이터
        public string Expression;
        public string Gesture;
        public string Think;
        public string TalkGoal1;
        public string TalkGoal2;
        public string Maintain;
        public string MoveGoal;
        public string ItemGoal;
        public string ActionGoal;
        public float Likeability; // Unity에서의 Friendship
        public float Mental;      // Unity에서의 Health
        public Dictionary<string, string> Quests; // Quest[]에서 Dictionary로 변경
    }

    [System.Serializable]
    public class Quest
    {
        public string questName;
        public string isCompleted;
    }

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("GameManager: Instance already exists. Destroying duplicate.");
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        Debug.Log("GameManager: Instance has been set.");

        // Set up input listener
        if (userInputField != null)
        {
            userInputField.onEndEdit.AddListener(OnInputFieldSubmit);
            Debug.Log("GameManager: InputField listener added.");
        }
        else
        {
            Debug.LogError("GameManager: userInputField is not assigned.");
        }
    }

    private void Start()
    {
        // Ensure RhythmManager is present in the scene
        if (rhythmManager == null)
        {
            Debug.LogError("GameManager: RhythmManager is not assigned.");
        }
        else
        {
            Debug.Log("GameManager: RhythmManager is assigned.");
        }

        // Ensure CafeUIManager is present
        if (CafeUIManager.instance == null)
        {
            Debug.LogError("GameManager: CafeUIManager instance is not found in the scene.");
        }
        else
        {
            Debug.Log("GameManager: CafeUIManager instance is found.");
        }

        // Ensure AudioManager is present in the scene
        if (AudioManager.instance == null)
        {
            Debug.LogError("GameManager: AudioManager instance is not found in the scene.");
        }
        else
        {
            Debug.Log("GameManager: AudioManager instance is found.");
        }
    }

    private void OnDestroy()
    {
        if (userInputField != null)
        {
            userInputField.onEndEdit.RemoveListener(OnInputFieldSubmit);
            Debug.Log("GameManager: InputField listener removed.");
        }
    }

    /// <summary>
    /// Handles input submission from the userInputField.
    /// </summary>
    /// <param name="input">The input text submitted by the user.</param>
    public void OnInputFieldSubmit(string input)
    {
        // Check if the user pressed Enter
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            Debug.Log("GameManager: Enter key pressed. Processing user input.");
            ProcessUserInput(input);
        }
    }

    /// <summary>
    /// Processes the user's input: updates the UI and communicates with the server.
    /// </summary>
    /// <param name="userInput">The input text from the user.</param>
    private void ProcessUserInput(string userInput)
    {
        if (isServerUrlSet)
        {
            if (!string.IsNullOrWhiteSpace(userInput))
            {
                // Display player's message in the UI
                if (CafeUIManager.instance != null)
                {
                    CafeUIManager.instance.DisplayPlayerMessage(userInput);
                    Debug.Log("GameManager: Displayed player message.");
                }
                else
                {
                    Debug.LogError("GameManager: CafeUIManager.instance is null.");
                }

                // Send input to the server
                if (!rhythmManager.IsCommunicatingWithServer)
                {
                    Debug.Log("GameManager: Starting server communication with user input.");
                    StartCoroutine(CommunicateWithServer(userInput));
                }

                // Clear the input field
                userInputField.text = "";
                userInputField.ActivateInputField(); // Keep the InputField focused
                Debug.Log("GameManager: Input field cleared and focused.");

                // Reset inactivity timer in RhythmManager to maintain talk mode
                rhythmManager.ResetToTalkMode();
                Debug.Log("GameManager: Reset inactivity timer in RhythmManager.");
            }
        }
        else
        {
            serverUrl = userInput.TrimEnd('/');
            isServerUrlSet = true;
            Debug.Log("GameManager: Server URL set to " + serverUrl);
        }
    }

    /// <summary>
    /// Triggers Talk Mode by proximity and sends an empty input to the server.
    /// </summary>
    public void SendEmptyInput()
    {
        Debug.Log("GameManager: Send Empty Input to Server");

        // Send empty input to the server
        if (!rhythmManager.IsCommunicatingWithServer)
        {
            Debug.Log("GameManager: Starting server communication with empty input.");
            StartCoroutine(CommunicateWithServer(""));
        }
        else
        {
            Debug.LogWarning("GameManager: Already communicating with server. Cannot send empty input.");
        }
    }

    /// <summary>
    /// Coroutine to communicate with the server.
    /// </summary>
    /// <param name="userInput">The user input to send to the server.</param>
    /// <returns></returns>
    IEnumerator CommunicateWithServer(string userInput)
    {
        Debug.Log("GameManager: CommunicateWithServer called with input: \"" + userInput + "\"");

        rhythmManager.IsCommunicatingWithServer = true;

        // 현재 월드 상태를 GOAPManager에서 가져옴
        WorldStatus currentWorldStatus = goapManager.CurrentWorldStatus;

        // 서버 요청 객체 생성 (월드 상태 포함)
        ServerRequest request = new ServerRequest(clientId, goapManager.CurrentNPCStatus, userInput, currentWorldStatus, rhythmManager.CurrentMode);

        Debug.Log("GameManager: Serialized ServerRequest: " + JsonConvert.SerializeObject(request));

        // Newtonsoft.Json을 사용하여 직렬화
        string jsonRequest = JsonConvert.SerializeObject(request);

        using (UnityWebRequest webRequest = new UnityWebRequest(serverUrl + "/api/game", "POST"))
        {
            byte[] jsonToSend = System.Text.Encoding.UTF8.GetBytes(jsonRequest);
            webRequest.uploadHandler = new UploadHandlerRaw(jsonToSend);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");

            Debug.Log("GameManager: Sending request to server.");

            // 요청을 보내고 응답을 기다림
            yield return webRequest.SendWebRequest();

            // 응답 처리
            try
            {
                if (webRequest.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("GameManager: Error communicating with server: " + webRequest.error);
                }
                else
                {
                    string jsonResponse = webRequest.downloadHandler.text;
                    Debug.Log("GameManager: Received response from server: " + jsonResponse);

                    // Newtonsoft.Json을 사용하여 응답 파싱
                    ServerResponse response = null;
                    try
                    {
                        response = JsonConvert.DeserializeObject<ServerResponse>(jsonResponse);
                        Debug.Log("GameManager: Successfully deserialized server response.");
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError("GameManager: Error parsing server response: " + ex.Message);
                        Debug.LogError("GameManager: Server response: " + jsonResponse);
                        yield break;
                    }

                    // client_id 저장
                    if (!string.IsNullOrEmpty(response.client_id))
                    {
                        clientId = response.client_id;
                        Debug.Log("GameManager: Updated clientId to " + clientId);
                    }

                    Debug.Log("GameManager: Received TalkGoals: " + response.TalkGoal1 + ", " + response.TalkGoal2);
                    if (CafeUIManager.instance != null)
                    {
                        CafeUIManager.instance.ReceiveMessage(response.TalkGoal1 + "\n" + response.TalkGoal2);
                        Debug.Log("GameManager: Displayed NPC messages.");
                    }
                    else
                    {
                        Debug.LogWarning("GameManager: CafeUIManager.instance is not set.");
                    }

                    // Pass the response to RhythmManager
                    if (rhythmManager != null)
                    {
                        rhythmManager.HandleServerResponse(response);
                        Debug.Log("GameManager: Passed server response to RhythmManager.");
                    }
                    else
                    {
                        Debug.LogError("GameManager: RhythmManager reference is not set.");
                    }

                    // Friendship과 Health UI 업데이트
                    if (CafeUIManager.instance != null)
                    {
                        CafeUIManager.instance.SetFriendShipSlider(response.Likeability);
                        CafeUIManager.instance.SetHeartSlider(response.Mental);
                        Debug.Log("GameManager: Updated Friendship and Health sliders.");

                        // 퀘스트 업데이트
                        if (response.Quests != null)
                        {
                            List<Quest> questList = new List<Quest>();
                            foreach (var questEntry in response.Quests)
                            {
                                Quest quest = new Quest
                                {
                                    questName = questEntry.Key,
                                    isCompleted = questEntry.Value
                                };
                                questList.Add(quest);
                            }

                            CafeUIManager.instance.UpdateQuestDisplay(questList.ToArray());
                            Debug.Log("GameManager: Quests received and processed.");
                        }
                        else
                        {
                            Debug.Log("GameManager: No quests received from server.");
                        }
                    }
                    else
                    {
                        Debug.LogWarning("GameManager: CafeUIManager.instance is not set.");
                    }

                    // 오디오 데이터 처리 using AudioManager
                    if (!string.IsNullOrEmpty(response.audio_file))
                    {
                        Debug.Log("GameManager: Received audio data from server.");
                        if (AudioManager.instance != null)
                        {
                            AudioManager.instance.PlayAudioFromBase64(response.audio_file);
                            Debug.Log("GameManager: Played audio from server.");
                        }
                        else
                        {
                            Debug.LogError("GameManager: AudioManager instance is not found. Cannot play audio.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("GameManager: Error in CommunicateWithServer: " + ex.Message);
            }
            finally
            {
                rhythmManager.IsCommunicatingWithServer = false;
                Debug.Log("GameManager: Communication with server has ended.");
            }
        }
    }
}
