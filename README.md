# GOALLM_v4_unity

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

