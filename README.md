# GOALLM_v4_unity

![NPC_LLM_Rhythm_GOAP](https://github.com/user-attachments/assets/c91febf0-0d72-43ac-9fa0-ae3730554f0b)



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

2. **플래너 초기화**:
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
