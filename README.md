# GOALLM_v4_unity


## Rhythm 시스템
어떤 객체의 시간에 따른 상태변화를 총괄하고 동기화하는 시스템.
파라미터와 상태로 이루어진다.


## NPC Rhythm 시스템 (RhythmManager.cs)
Player와 NPC간의 대화가 진행중인지 여부에 따라 **대화모드**와 **자율모드**를 나눔.

**대화모드**시 NPC와 대화할 수 있음
- Player가 NPC에게 인사하거나, 다가가면 **대화모드**로 전환됨.
- 말을 하면 NPC는 그에 대한 '대답 한 문장' + 확률적으로 생성되는 '스토리 진행 한 문장' 함. 
- NPC는 '스토리 진행 문장' 생성시 **대화모드**를 유지할지/종료할지 자발적 선택
- NPC가 생성하는 '문장'이 재생되기 전에 Player가 새로운 입력을 보내면, 준비중인 '문장'은 버림.

**자율모드**시 NPC와 대화가 불가능하고, NPC는 자율적으로 행동함.
- Player가 NPC로부터 멀어지거나, 대답을 10초 이상 안할시 **자율모드**로 전환
- **자율모드**에서는 NPC는 접촉을 유지할지/종료할지 자발적 선택
- NPC가 자신에게 주어진 퀘스트를 클리어하는 방향으로 진행됨
- 과거 대화기록을 바탕으로 회상하여 스스로 퀘스트를 만듦: 그냥 Generative Agent(나중에 개발)



## Game Rhythm 시스템 (GameManager.cs)
게임의 진행에 관련된 정보를 player, NPC, LLM server, UI끼리 동기화한다.
- 퀘스트/호감도/멘탈의 상시 업데이트와 동기화
- 유저 name, key, token, history를 동기화




## 기존 스크립트 구성

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

## 실제 코드 일부
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
