using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using Newtonsoft.Json; // Ensure Newtonsoft.Json is properly referenced

public class GameManagerServerSet : MonoBehaviour
{
    public static GameManagerServerSet instance;
    public InputField userInputField;
    public GOAPManager goapManager;
    public Animator characterAnimator;

    // 서버 URL
    private string serverUrl = "https://5654-34-81-159-163.ngrok-free.app"; // 실제 서버 URL로 변경하세요
    private bool isServerUrlSet = true; // 서버 URL 입력을 건너뛰려면 true로 설정

    private string clientId; // client_id 저장

    [System.Serializable]
    public class ServerRequest
    {
        public string client_id;
        public NPCStatus npc_status;
        public string userInput;

        public ServerRequest(string clientId, NPCStatus status, string input)
        {
            this.client_id = clientId;
            npc_status = status;
            userInput = input;
        }
    }

    [System.Serializable]
    private class ServerResponse
    {
        public string client_id;
        public string audio_file; // Base64로 인코딩된 WAV 오디오 데이터
        public string Expression;
        public string Gesture;
        public string Think;
        public string TalkGoal;
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
        if (instance != null) return;
        instance = this;

        // Ensure AudioManager is present in the scene
        if (AudioManager.instance == null)
        {
            Debug.LogError("GameManagerServerSet: AudioManager instance is not found in the scene.");
        }
    }

    public void OnInputFieldSubmit(string input)
    {
        if (isServerUrlSet) // 서버 URL이 이미 설정된 경우
        {
            if (!IsInvoking("CommunicateWithServer"))
            {
                StartCoroutine(CommunicateWithServer(input));
            }
        }
        else
        {
            serverUrl = input.TrimEnd('/');
            isServerUrlSet = true;
        }
    }

    IEnumerator CommunicateWithServer(string userInput)
    {
        Debug.Log("CommunicateWithServer called with input: " + userInput);


        // 현재 NPC 상태와 client_id를 사용하여 ServerRequest 생성
        ServerRequest request = new ServerRequest(clientId, goapManager.CurrentNPCStatus, userInput);

        Debug.Log("NPC Status: " + goapManager.CurrentNPCStatus);

        string jsonRequest = JsonUtility.ToJson(request);

        using (UnityWebRequest webRequest = new UnityWebRequest(serverUrl + "/api/game", "POST"))
        {
            byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonRequest);
            webRequest.uploadHandler = new UploadHandlerRaw(jsonToSend);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");

            // 요청을 보내고 응답을 기다림
            yield return webRequest.SendWebRequest();

            // 응답 처리
            try
            {
                if (webRequest.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("Error: " + webRequest.error);
                }
                else
                {
                    string jsonResponse = webRequest.downloadHandler.text;

                    // Newtonsoft.Json을 사용하여 응답 파싱
                    ServerResponse response = null;
                    try
                    {
                        response = JsonConvert.DeserializeObject<ServerResponse>(jsonResponse);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError("Error parsing server response: " + ex.Message);
                        Debug.LogError("Server response: " + jsonResponse);
                        yield break;
                    }

                    // client_id 저장
                    if (!string.IsNullOrEmpty(response.client_id))
                    {
                        clientId = response.client_id;
                    }

                    Debug.Log("TalkGoal: " + response.TalkGoal);
                    CafeUIManager.instance.ReceiveMessage(response.TalkGoal);

                    if (goapManager != null)
                    {
                        Debug.Log("Expression: " + response.Expression);
                        Debug.Log("Gesture: " + response.Gesture);
                        Debug.Log("MoveGoal: " + response.MoveGoal);
                        Debug.Log("ItemGoal: " + response.ItemGoal);
                        Debug.Log("ActionGoal: " + response.ActionGoal);
                        characterAnimator.SetTrigger(response.Expression);

                        goapManager.SetGoals(response.Gesture, response.MoveGoal, response.ItemGoal, response.ActionGoal);
                    }
                    else
                    {
                        Debug.LogError("GOAPManager reference is not set in GameManager.");
                    }

                    // Friendship과 Health UI 업데이트
                    if (CafeUIManager.instance != null)
                    {
                        CafeUIManager.instance.SetFriendShipSlider(response.Likeability);
                        CafeUIManager.instance.SetHeartSlider(response.Mental);
                        Debug.Log("Likability: " + response.Likeability + " Mental: " + response.Mental);

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
                            Debug.Log("Quests received and processed.");
                        }
                        else
                        {
                            Debug.Log("No quests received from server.");
                        }
                    }

                    // 오디오 데이터 처리 using AudioManager
                    if (!string.IsNullOrEmpty(response.audio_file))
                    {
                        Debug.Log("Received audio data from server.");
                        if (AudioManager.instance != null)
                        {
                            AudioManager.instance.PlayAudioFromBase64(response.audio_file);
                        }
                        else
                        {
                            Debug.LogError("AudioManager instance is not found. Cannot play audio.");
                        }
                    }

                    userInputField.text = ""; // 입력 필드 초기화
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("Error in CommunicateWithServer: " + ex.Message);
            }
        }
    }


}
