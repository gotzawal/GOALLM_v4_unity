# GOALLM_v4_unity
Creative Commons NonCommercial (CC BY-NC) (비상업적 용도로 이용가능)
![GOALLM_v4-Unity](https://github.com/user-attachments/assets/0f0f8c03-3a0c-4c8c-aafe-f5f1ac388b59)




## Rhythm 시스템
어떤 객체의 시간에 따른 상태변화를 총괄하고 동기화하는 시스템.
파라미터와 상태로 이루어진다.


### NPC Rhythm 시스템 (RhythmManager.cs)
Player와 NPC간의 대화가 진행중인지 여부에 따라 **Talkmode**와 **automode**를 나눔.

**talkmode**시 NPC와 대화할 수 있음
- Player가 NPC에게 말하거나, 다가가면 talkmode로 전환됨. (다가가면 player의 대사 없이 서버로 전송됨)
- 서버에서 NPC는 그에 대한 '대답 한 문장' + 확률적으로 생성되는 '스토리 진행 한 문장' 반환함. 
- 서버에서 NPC는 '스토리 진행 문장' 생성시 **대화모드**를 유지할지/종료할지 자발적 선택
- NPC가 생성하는 '문장'이 재생되기 전에 Player가 새로운 입력을 보내면, 준비중인 '문장'은 버림. 

**automode**시 NPC와 대화가 불가능하고, NPC는 자율적으로 행동함.
- Player가 NPC로부터 멀어지거나, 대답을 10초 이상 안할시 **Automode**로 전환
- automode에서는 NPC는 10~15초마다 서버로 automode, input은 ""으로 전송
- NPC가 자신에게 주어진 퀘스트를 클리어하는 방향으로 진행됨
- 과거 대화기록을 바탕으로 회상하여 스스로 퀘스트를 만듦: 그냥 Generative Agent(나중에 개발)

- maintain이 yes면 talkmode로 바꾸고, no면 automode로 바꿈.
- 서버 응답이 오기 전까지는 새로 서버에 요청하지 않음
- 

### Player Rhythm 시스템 (GameManager.cs)
게임의 진행에 관련된 정보를 player, NPC, LLM server, UI끼리 동기화한다.
- 퀘스트/호감도/멘탈의 상시 업데이트와 동기화
- 유저 name, key, token, history를 동기화


### (추후개발) NPC Micro-Rhythm 시스템 (MicroRhythmManager.cs)
미세한 행동/표정의 습관을 부여함.


### NPC 리듬이 추가된 서버 입출력 형태
#### 서버입력
- clientid: '0234209'
- npcstatus:  {'Location': 'picture', 'Inventory': 'none', 'Pose': 'stand', 'Holding': 'none', 'Health': '100', 'Mental': '100'}
- worldstatus: {"Places":{"piano":{"Name":"piano","Inventory":[],"State":{}},"picture":{"Name":"picture","Inventory":[],"State":{}},"tv":{"Name":"tv","Inventory":[],"State":{"tv_state":"off"}},"meja":{"Name":"meja","Inventory":["lance","snack","Lance","Pillow","Snack"],"State":{}},"sofa":{"Name":"sofa","Inventory":["pillow"],"State":{}},"player":{"Name":"player","Inventory":[],"State":{}}},"Items":{"snack":{"Name":"snack"},"lance":{"Name":"lance"},"pillow":{"Name":"pillow"}}}
- npcmode: 'talkmode'
- userInput: 'hi, how are you?'

#### 서버출력
- Gesture: Bashful
- TalkGoal1: I'm fine. Welcome to cafe stella!
- TalkGoal2: Do you want to order something?
- Maintain: yes
- MoveGoal: sofa
- ItemGoal: none
- ActionGoal: none
- Likeability: 105
- Mental: 100
- Quests: {'Make First Conversation with Hoshikawa': 'Cleared'}
- Audio file


### 기존 서버 입출력 형태
#### 서버 입력
- DEBUG:main:Received data: {'clientid': '', 'npcstatus': {'Location': 'picture', 'Inventory': 'none', 'Pose': 'stand', 'Holding': 'none', 'Health': '100', 'Mental': '100'}, 'userInput': 'hi, how are you?'}
- DEBUG:main:Generated new clientid: 2c6ec28f-0055-4bfc-b196-bd2b334ecf21
- DEBUG:main:Creating new session for client 2c6ec28f-0055-4bfc-b196-bd2b334ecf21
- DEBUG:_main:User Message: hi, how are you?
- DEBUG:main:NPC Status: {'Location': 'picture', 'Inventory': 'none', 'Pose': 'stand', 'Holding': 'none', 'Health': '100', 'Mental': '100'}


#### 서버 출력
- DEBUG:openai.baseclient:requestid: req1853123d0e6a31b116d64af77c02e2fa
- DEBUG:main:NPC Gesture: Bashful
- DEBUG:main:NPC Talk Goal: ようこそ、メイドカフェ「Stella」へ！私は星川です。ごゆっくりどうぞ！
- DEBUG:main:NPC Move Goal: sofa
- DEBUG:main:NPC Item Goal: none
- DEBUG:main:NPC Action Goal: none
- DEBUG:main:NPC Likeability: 105
- DEBUG:main:NPC Mental: 100
- DEBUG:main:NPC Quests: {'Make First Conversation with Hoshikawa': 'Cleared'}
- generating TTS
- DEBUG:urllib3.connectionpool:Starting new HTTPS connection (1): 47db-157-82-13-201.ngrok-free.app:443
- DEBUG:urllib3.connectionpool:https://XXXXXXXXXXXXXX.ngrok-free.app/ "POST /tts HTTP/11" 200 631852
- DEBUG:__main:Audio Binary: UklGRpB/AwBXQVZFZm10IBAAAAABAAEAgD4AAAB9AAACABAAZGF0YWx/AwAAAPz//f/8//3//f8AAP7//P///wEAAAD///3///8B



## 유니티에서 할당 방법
![image](https://github.com/user-attachments/assets/64d81d2c-bf17-49be-9806-6623ec632dc1)

![image](https://github.com/user-attachments/assets/8a1a30b6-ec7f-4e98-82de-857bf7fc4ad6)

![image](https://github.com/user-attachments/assets/fdc0f18e-8138-4c94-bb7e-5e4209db1f90)


## 기존 스크립트 구성
=======


## 주요 스크립트 구성


---

### **CafeUIManager.cs**
- 사용자 입력을 받아 `SendMessage_F`를 통해 **GameManager**의 서버 통신 함수를 호출.

---

### **GameManager.cs**

#### **`IEnumerator CommunicateWithServer(string userInput)`**
1. **사용자 입력 즉시 표시**:
   - `UpdateChatHistoryWithUserInput(userInput)` 호출.

2. **서버 요청 생성**:
   - `ServerRequest` 객체를 생성하여 현재 NPC 상태와 `client_id` 포함.
   - `webRequest.SendWebRequest()`로 요청 전송 후 응답 대기.

3. **응답 처리**:
   - **채팅 메시지**:
     - `CafeUIManager.instance.ReceiveMessage(response.TalkGoal)` 호출로 UI에 표시.
   - **NPC 표정 설정**:
     - `characterAnimator.SetTrigger(response.Expression)` 호출.
   - **GOAP 목표 할당**:
     - `goapManager.SetGoals` 호출로 응답 데이터 기반 목표 설정.
   - **호감도, 멘탈, 퀘스트 업데이트**:
     - `CafeUIManager.instance.SetFriendShipSlider(response.Likeability)`
     - `CafeUIManager.instance.SetHeartSlider(response.Mental)`
     - `CafeUIManager.instance.UpdateQuestDisplay(questList.ToArray())` 호출.
   - **음성 재생**:
     - `StartCoroutine(PlayAudioFromBase64(response.audio_file))` 호출.

---

### **GOAPManager.cs**

#### **`SetGoals(string gesture, string moveGoal, string itemGoal, string actionGoal)`**
1. **목표 생성**:
   - `GoalParser.ParseSentenceToGoal`을 사용해 각 목표를 GOAP 형태로 파싱.
     - 예: `GoalParser.ParseSentenceToGoal($"Do {gesture}", actions, worldState, weight: 1f)`.

2. **계획 생성**:
   - `GOAPPlanner` 객체 생성 및 계획 수립:
     - `planner.Plan(npcState, worldState)` 호출.

3. **계획 실행**:
   - `StartCoroutine(ExecutePlan(planResult))` 호출로 계획 순차 실행.
     - **캐릭터 이동 및 모션**: `CharacterControl.cs` 호출.
     - **아이템 관련 작업**: `InteractionControl.cs` 호출.

---

### **주요 동작 흐름**
1. 사용자 입력 ➡️ **`CafeUIManager`** ➡️ **`GameManager`**:
   - 서버와 통신하여 입력 처리 및 응답 수신.

2. 응답 처리 ➡️ **UI 업데이트**:
   - 채팅, 표정, 호감도, 멘탈, 퀘스트 등 실시간 반영.

3. 응답 기반 목표 설정 ➡️ **GOAP 계획 실행**:
   - NPC 동작 및 상태 변화 제어.

---

## 기존 코드 일부
```csharp
[CafeUIManager.cs]
- 입력이 들어오면 SendMessage_F 함수를 통해 GameManager의 서버 통신 함수를 호출

[GameManager.cs]
    IEnumerator CommunicateWithServer(string userInput)
    {
        // 사용자 입력을 즉시 표시:
        UpdateChatHistoryWithUserInput(userInput);
        // 현재 NPC 상태와 client_id를 사용하여 ServerRequest 생성:
        ServerRequest request = new ServerRequest(clientId, goapManager.CurrentNPCStatus, userInput);
            // 요청을 보내고 응답을 기다림:
            yield return webRequest.SendWebRequest();
                    // 채팅 UI에 띄우기:
                    CafeUIManager.instance.ReceiveMessage(response.TalkGoal);
                        // 표정 설정:
                        characterAnimator.SetTrigger(response.Expression);
                        // GOAP에 목표 할당:
                        goapManager.SetGoals(response.Gesture, response.MoveGoal, response.ItemGoal, response.ActionGoal);
                        // 호감도+멘탈+퀘스트 표시: 
                        CafeUIManager.instance.SetFriendShipSlider(response.Likeability);
                        CafeUIManager.instance.SetHeartSlider(response.Mental);
                                questList.Add(quest);
                            CafeUIManager.instance.UpdateQuestDisplay(questList.ToArray());
                        // 음성 재생 :
                        StartCoroutine(PlayAudioFromBase64(response.audio_file));
    }

[GOAPManager.cs]
    public void SetGoals(string gesture, string moveGoal, string itemGoal, string actionGoal)
    {
            // 각 목표에 맞는 goal의 형태 파싱:
            Goal gestureGoal = GoalParser.ParseSentenceToGoal($"Do {gesture}", actions, worldState, weight: 1f);
            Goal moveGoalObj = GoalParser.ParseSentenceToGoal($"Go to {moveGoal}", actions, worldState, weight: 1f);
            Goal itemGoalObj = GoalParser.ParseSentenceToGoal(itemGoal, actions, worldState, weight: 1f);
            Goal actionGoalObj = GoalParser.ParseSentenceToGoal($"Do {actionGoal}", actions, worldState, weight: 1f);
        // Initialize Planner:
        GOAPPlanner planner = new GOAPPlanner(parsedGoals, actions);
        // Execute Plan:
        var planResult = planner.Plan(npcState, worldState);
            // Start executing the plan:
            StartCoroutine(ExecutePlan(planResult));
​    }
    \\ ExecutePlan함수는 계획을 순차적으로 실행하며, 캐릭터의 이동/모션은 CharacterControl.cs, 캐릭터의 item관련은 InteractionControl.cs호출

'''
