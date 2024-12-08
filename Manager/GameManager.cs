using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using Newtonsoft.Json; // Newtonsoft.Json 추가


public class GameManagerServerSet : MonoBehaviour
{
    public static GameManagerServerSet instance;
    public InputField userInputField;
    public GOAPManager goapManager;
    public AudioSource audioSource; // AudioSource 컴포넌트 추가
    public Animator characterAnimator;

    // 서버 URL
    private string serverUrl = ""; // 실제 서버 URL로 변경하세요
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

        // AudioSource 컴포넌트가 설정되지 않은 경우 추가
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
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
            UpdateChatHistoryWithNPCTalk("Start Chatting.");
        }
    }

    IEnumerator CommunicateWithServer(string userInput)
    {
        Debug.Log("CommunicateWithServer called with input: " + userInput);

        // 사용자 입력을 즉시 표시
        UpdateChatHistoryWithUserInput(userInput);

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
                    UpdateChatHistoryWithNPCTalk("Error.");
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
                                Quest quest = new Quest();
                                quest.questName = questEntry.Key;
                                quest.isCompleted = questEntry.Value;
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

                    // 오디오 데이터 처리
                    if (!string.IsNullOrEmpty(response.audio_file))
                    {
                        Debug.Log("Received audio data from server.");
                        StartCoroutine(PlayAudioFromBase64(response.audio_file));
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

    void UpdateChatHistoryWithUserInput(string userInput)
    {
        // 이미 CafeUIManager에서 SendMessage_F를 호출하므로 여기서는 필요 없음
    }

    void UpdateChatHistoryWithNPCTalk(string talkGoal)
    {
        // 서버 응답 후 NPC 대화 표시
        // 필요에 따라 채팅 히스토리 업데이트 로직 구현
    }

    IEnumerator PlayAudioFromBase64(string base64Audio)
    {
        Debug.Log("PlayAudioFromBase64 called.");
        try
        {
            byte[] audioBytes = Convert.FromBase64String(base64Audio);

            // WAV 파일 헤더 파싱 및 PCM 데이터 추출
            WAV wav = new WAV(audioBytes);

            // AudioClip 생성
            AudioClip audioClip = AudioClip.Create("ServerAudio", wav.SampleCount, wav.ChannelCount, wav.Frequency, false);
            audioClip.SetData(wav.LeftChannel, 0);

            // AudioSource에 할당하고 재생
            audioSource.clip = audioClip;
            audioSource.Play();
        }
        catch (Exception ex)
        {
            Debug.LogError("Error in PlayAudioFromBase64: " + ex.Message);
        }
        yield return null;
    }
}

// WAV 파일 파싱 클래스
public class WAV
{
    public float[] LeftChannel { get; private set; }
    public int ChannelCount { get; private set; }
    public int SampleCount { get; private set; }
    public int Frequency { get; private set; }

    public WAV(byte[] wav)
    {
        // 모노 또는 스테레오인지 확인
        ChannelCount = BitConverter.ToInt16(wav, 22);

        // 주파수 가져오기
        Frequency = BitConverter.ToInt32(wav, 24);

        // 오디오 데이터 위치 지정
        int pos = 12;

        // 데이터 청크를 찾을 때까지 이동
        while (!(wav[pos] == 'd' && wav[pos + 1] == 'a' && wav[pos + 2] == 't' && wav[pos + 3] == 'a'))
        {
            pos += 4;
            int chunkSize = BitConverter.ToInt32(wav, pos);
            pos += 4 + chunkSize;
        }
        pos += 8;

        // 샘플 수 계산
        SampleCount = (wav.Length - pos) / 2 / ChannelCount;

        // 샘플을 저장할 float 배열 생성
        LeftChannel = new float[SampleCount];

        // 바이트 데이터를 float로 변환
        int i = 0;
        while (pos < wav.Length)
        {
            LeftChannel[i] = BytesToFloat(wav[pos], wav[pos + 1]);
            pos += 2;
            if (ChannelCount == 2)
            {
                pos += 2; // 스테레오인 경우 오른쪽 채널 건너뜀
            }
            i++;
        }
    }

    private float BytesToFloat(byte firstByte, byte secondByte)
    {
        // 두 바이트를 하나의 short로 변환 (little endian)
        short s = (short)((secondByte << 8) | firstByte);
        // -1에서 1 사이의 범위로 변환
        return s / 32768.0f;
    }
}
