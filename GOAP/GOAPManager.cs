using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using System.Linq; // LINQ 사용을 위해 추가

// Assuming Place, Item, GOAPAction, NPCState, WorldState, GOAPPlanner, ActionFactory, GoalParser, NPCStatus are defined elsewhere

public class GOAPManager : MonoBehaviour
{
    // Inspector-assigned references
    public CharacterControl characterControl; // Inspector에서 할당 필요

    // Inspector-assigned target objects
    public GameObject[] targetObjects;

    // Places definition
    public Dictionary<string, Place> Places { get; private set; }
    private Dictionary<string, List<string>> placeConnections;

    // Items definition
    public Dictionary<string, Item> items { get; private set; }

    // Actions definition
    private List<GOAPAction> actions;

    // NPC and World State
    [SerializeField] private NPCState npcState;

    public NPCState NpcState
    {
        get { return npcState; }
    }
    private WorldState worldState;

    // Execution control
    private bool isExecutingPlan = false;

    // Gesture names list
    private List<string> gestureNames = new List<string>
    {
        "Bashful",
        "Happy Gesture",
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

    void Awake()
    {
        // Initialize Places
        InitializePlaces();

        // Initialize Items
        InitializeItems();

        // Initialize NPC State
        InitializeNPCState();

        // Initialize World State
        worldState = new WorldState(Places, items);

        // Initialize Actions
        InitializeActions();
    }

    void Start()
    {
        // Any additional initialization if necessary
        Debug.Log("GOAPExample: Start() called.");
    }

    private void InitializePlaces()
    {
        Places = new Dictionary<string, Place>(StringComparer.OrdinalIgnoreCase);
        placeConnections = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        // Find Place GameObjects in the scene
        GameObject pianoGO = GameObject.Find("Piano");
        GameObject pictureGO = GameObject.Find("Picture");
        GameObject tvGO = GameObject.Find("TV");
        GameObject mejaGO = GameObject.Find("Meja");
        GameObject sofaGO = GameObject.Find("Sofa");

        if (pianoGO == null || pictureGO == null || tvGO == null || mejaGO == null || sofaGO == null)
        {
            Debug.LogError("GOAPExample: One or more Place GameObjects not found in the scene. Please ensure they are named correctly.");
            return;
        }

        // Create Place instances
        Place piano = new Place("piano", pianoGO, new List<string>(), new Dictionary<string, object>());
        Place picture = new Place("picture", pictureGO, new List<string>(), new Dictionary<string, object>());
        Place tv = new Place("tv", tvGO, new List<string>(), new Dictionary<string, object> { { "tv_state", "off" } });
        Place meja = new Place("meja", mejaGO, new List<string> { "lance", "snack" }, new Dictionary<string, object>());
        Place sofa = new Place("sofa", sofaGO, new List<string> { "pillow" }, new Dictionary<string, object>());

        // Add to Places dictionary
        Places.Add("piano", piano);
        Places.Add("picture", picture);
        Places.Add("tv", tv);
        Places.Add("meja", meja);
        Places.Add("sofa", sofa);

        // Define place connections
        placeConnections.Add("piano", new List<string> { "picture", "tv", "meja", "sofa" });
        placeConnections.Add("picture", new List<string> { "piano", "tv", "meja", "sofa" });
        placeConnections.Add("tv", new List<string> { "piano", "picture", "meja", "sofa" });
        placeConnections.Add("meja", new List<string> { "picture", "tv", "piano", "sofa" });
        placeConnections.Add("sofa", new List<string> { "picture", "tv", "meja", "piano" });

        Debug.Log("GOAPExample: Places initialized successfully.");
    }

    private void InitializeItems()
    {
        Item snack = new Item(
            "snack",
            new Dictionary<string, Dictionary<string, object>>
            {
                {
                    "use", new Dictionary<string, object>
                    {
                        { "health", 10 }
                    }
                }
            }
        );

        Item lance = new Item(
            "lance",
            new Dictionary<string, Dictionary<string, object>>
            {
                {
                    "use", new Dictionary<string, object>
                    {
                        { "health", 10 }
                    }
                }
            }
        );
        Item pillow = new Item("pillow", new Dictionary<string, Dictionary<string, object>>());

        items = new Dictionary<string, Item>(StringComparer.OrdinalIgnoreCase)
        {
            { "snack", snack },
            { "lance", lance },
            { "pillow", pillow }
        };

        Debug.Log("GOAPExample: Items initialized successfully.");
    }

    private void InitializeNPCState()
    {
        npcState = new NPCState(
            upperBody: new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase) { { "hold", "none" } },
            lowerBody: new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase) { { "location", "picture" }, { "pose", "stand" } },
            resources: new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase) { { "time", 0f }, { "health", 100f }, { "mental", 100f } },
            inventory: new List<string>(),
            stateData: new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
        );
        npcState.GameObject = this.gameObject; // Assign NPC's GameObject

        Debug.Log("GOAPExample: NPCState initialized successfully.");
    }

    private void InitializeActions()
    {
        actions = new List<GOAPAction>
        {
            new GOAPAction(
                name: "use_snack",
                conditions: new Dictionary<string, Func<NPCState, WorldState, bool>>(StringComparer.OrdinalIgnoreCase)
                {
                    { "has_snack", (npc, world) => 
                        npc.Inventory.Any(i => i.Equals("snack", StringComparison.OrdinalIgnoreCase)) 
                    }
                },
                effects: new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
                {
                    { "health", 10 }, // Increase health by 10
                    { "used_snack", true } // Flag indicating the snack was used
                },
                cost: new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase)
                {
                    { "time", 0.5f },
                    { "health", 0f },
                    { "mental", 0f }
                }
            ),
            new GOAPAction(
                name: "sit_sofa",
                conditions: new Dictionary<string, Func<NPCState, WorldState, bool>>(StringComparer.OrdinalIgnoreCase)
                {
                    { "location", (npc, world) => npc.LowerBody.ContainsKey("location") && npc.LowerBody["location"].ToString().Equals("sofa", StringComparison.OrdinalIgnoreCase) },
                    { "pose", (npc, world) => npc.LowerBody.ContainsKey("pose") && npc.LowerBody["pose"].ToString().Equals("stand", StringComparison.OrdinalIgnoreCase) }
                },
                effects: new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
                {
                    { "pose", "sit" }
                },
                cost: new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase)
                {
                    { "time", 1f },
                    { "health", 1f },
                    { "mental", 1f }
                }
            ),
            new GOAPAction(
                name: "stand_sofa",
                conditions: new Dictionary<string, Func<NPCState, WorldState, bool>>(StringComparer.OrdinalIgnoreCase)
                {
                    { "location", (npc, world) => npc.LowerBody.ContainsKey("location") && npc.LowerBody["location"].ToString().Equals("sofa", StringComparison.OrdinalIgnoreCase) },
                    { "pose", (npc, world) => npc.LowerBody.ContainsKey("pose") && npc.LowerBody["pose"].ToString().Equals("sit", StringComparison.OrdinalIgnoreCase) }
                },
                effects: new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
                {
                    { "pose", "stand" }
                },
                cost: new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase)
                {
                    { "time", 1f },
                    { "health", 3f },
                    { "mental", 3f }
                }
            ),
            // 'use_lance' 액션 추가
            new GOAPAction(
                name: "use_lance",
                conditions: new Dictionary<string, Func<NPCState, WorldState, bool>>(StringComparer.OrdinalIgnoreCase)
                {
                    { "has_lance", (npc, world) => npc.Inventory.Any(i => i.Equals("lance", StringComparison.OrdinalIgnoreCase)) },
                    { "is_holding_lance", (npc, world) => npc.UpperBody.ContainsKey("hold") && npc.UpperBody["hold"].ToString().Equals("lance", StringComparison.OrdinalIgnoreCase) }
                },
                effects: new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
                {
                    { "used_lance", true }
                     // 란스 사용 완료 플래그
                },
                cost: new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase)
                {
                    { "time", 1f }, // 사용 시간
                    { "health", 0f }, // 건강에 미치는 영향
                    { "mental", 0f } // 정신에 미치는 영향
                }
            ),
            // 'set_tv_state_on' 액션 추가
            new GOAPAction(
                name: "set_tv_state_on",
                conditions: new Dictionary<string, Func<NPCState, WorldState, bool>>(StringComparer.OrdinalIgnoreCase)
                {
                    { "is_near_tv", (npc, world) => npc.LowerBody.ContainsKey("location") && npc.LowerBody["location"].ToString().Equals("tv", StringComparison.OrdinalIgnoreCase) }
                },
                effects: new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
                {
                    { "place_state:tv:tv_state", "on" }
                },
                cost: new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase)
                {
                    { "time", 0.5f },
                    { "health", 0f },
                    { "mental", 0f }
                }
            ),
            // 'set_tv_state_off' 액션 추가
            new GOAPAction(
                name: "set_tv_state_off",
                conditions: new Dictionary<string, Func<NPCState, WorldState, bool>>(StringComparer.OrdinalIgnoreCase)
                {
                    { "is_near_tv", (npc, world) => npc.LowerBody.ContainsKey("location") && npc.LowerBody["location"].ToString().Equals("tv", StringComparison.OrdinalIgnoreCase) }
                },
                effects: new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
                {
                    { "place_state:tv:tv_state", "off" }
                },
                cost: new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase)
                {
                    { "time", 0.5f },
                    { "health", 0f },
                    { "mental", 0f }
                }
            )
        };

        // Create move actions based on place connections
        foreach (var place in placeConnections)
        {
            string fromPlace = place.Key.ToLower();
            foreach (var toPlace in place.Value)
            {
                string toPlaceLower = toPlace.ToLower();
                float timeCost = 1.5f;
                float healthCost = 1.5f;

                if ((fromPlace == "sofa" && toPlaceLower == "meja") ||
                    (fromPlace == "meja" && toPlaceLower == "sofa") ||
                    (fromPlace == "tv" && toPlaceLower == "piano") ||
                    (fromPlace == "piano" && toPlaceLower == "tv"))
                {
                    timeCost = 0.5f;
                    healthCost = 0.5f;
                }

                actions.Add(ActionFactory.CreateMoveAction(fromPlace, toPlaceLower, timeCost, healthCost));
            }
        }

        // Initialize gesture actions
        foreach (var gesture in gestureNames)
        {
            GOAPAction gestureAction = ActionFactory.CreateGestureAction(gesture.ToLower());
            actions.Add(gestureAction);
        }

        // Add item actions (Pick and Drop)
        foreach (var item in items.Keys)
        {
            string itemLower = item.ToLower();
            GOAPAction pickAction = ActionFactory.CreatePickAction(itemLower);
            GOAPAction dropAction = ActionFactory.CreateDropAction(itemLower);
            actions.Add(pickAction);
            actions.Add(dropAction);
        }

        Debug.Log("GOAPExample: Actions initialized successfully.");
    }

    /// <summary>
    /// Resets gesture flags in NPCState to allow repeated gestures
    /// </summary>
    private void ResetNPCState()
    {
        foreach (var gesture in gestureNames)
        {
            string flag = $"did_{gesture.ToLower()}";
            if (npcState.StateData.ContainsKey(flag))
            {
                npcState.StateData[flag] = false;
                Debug.Log($"GOAPExample: NPCState {flag} reset to false.");
            }
        }
    }

    /// <summary>
    /// Sets goals based on server response and initiates planning and execution.
    /// 이제 네 번째 매개변수 actionGoal을 추가하여 처리합니다.
    /// </summary>
    public void SetGoals(string gesture, string moveGoal, string itemGoal, string actionGoal)
    {
        if (isExecutingPlan)
        {
            Debug.LogWarning("GOAPExample: A plan is already being executed. Please wait until it's finished.");
            return;
        }

        // Reset NPCState before planning
        ResetNPCState();

        // Define goals based on input
        List<Goal> parsedGoals = new List<Goal>();

        // 1. Validate gesture
        if (!string.IsNullOrEmpty(gesture) && !gesture.Equals("none", StringComparison.OrdinalIgnoreCase))
        {
            string processedGesture = gesture.ToLower();
            if (!gestureNames.Any(g => g.Equals(processedGesture, StringComparison.OrdinalIgnoreCase)))
            {
                Debug.LogError($"GOAPExample: Invalid gesture '{gesture}'.");
                return;
            }

            Goal gestureGoal = GoalParser.ParseSentenceToGoal($"Do {gesture}", actions, worldState, weight: 1f);
            if (gestureGoal != null)
                parsedGoals.Add(gestureGoal);

            Debug.Log("GOAPExample: Gesture goal set.");
        }

        // 2. Validate move goal
        if (!string.IsNullOrEmpty(moveGoal) && !moveGoal.Equals("none", StringComparison.OrdinalIgnoreCase))
        {
            string normalizedMoveGoal = moveGoal.ToLower();
            if (!Places.ContainsKey(normalizedMoveGoal))
            {
                Debug.LogError($"GOAPExample: Invalid move goal. Place '{moveGoal}' does not exist.");
                return;
            }

            Goal moveGoalObj = GoalParser.ParseSentenceToGoal($"Go to {moveGoal}", actions, worldState, weight: 1f);
            if (moveGoalObj != null)
                parsedGoals.Add(moveGoalObj);

            Debug.Log("GOAPExample: Move goal set.");
        }

        // 3. Validate item goal
        if (!string.IsNullOrEmpty(itemGoal) && !itemGoal.Equals("none", StringComparison.OrdinalIgnoreCase))
        {
            Goal itemGoalObj = GoalParser.ParseSentenceToGoal(itemGoal, actions, worldState, weight: 1f);
            if (itemGoalObj != null)
                parsedGoals.Add(itemGoalObj);

            Debug.Log("GOAPExample: Item goal set.");
        }

        // 4. Validate action goal
        if (!string.IsNullOrEmpty(actionGoal) && !actionGoal.Equals("none", StringComparison.OrdinalIgnoreCase))
        {
            string processedActionGoal = actionGoal.ToLower();
            // 여기서는 actionGoal의 유효성을 추가로 검증할 수 있습니다.
            // 예: 특정 액션만 허용하거나, 액션 이름이 사전에 정의되어 있는지 확인 등

            // 예를 들어, 허용된 액션 목록이 있다면 다음과 같이 검증할 수 있습니다:
            // List<string> allowedActions = new List<string> { "dance", "jump", "wave" };
            // if (!allowedActions.Contains(processedActionGoal))
            // {
            //     Debug.LogError($"GOAPExample: Invalid action goal '{actionGoal}'.");
            //     return;
            // }

            // 여기서는 단순히 "Do {actionGoal}" 형태로 파싱합니다.
            Goal actionGoalObj = GoalParser.ParseSentenceToGoal($"Do {actionGoal}", actions, worldState, weight: 1f);
            if (actionGoalObj != null)
                parsedGoals.Add(actionGoalObj);

            Debug.Log("GOAPExample: Action goal set.");
        }

        if (parsedGoals.Count == 0)
        {
            Debug.LogWarning("GOAPExample: No valid goals provided.");
            return;
        }

        // Initialize Planner
        GOAPPlanner planner = new GOAPPlanner(parsedGoals, actions);

        // Execute Plan
        var planResult = planner.Plan(npcState, worldState);

        if (planResult != null)
        {
            Debug.Log("GOAPExample: Plan successfully created:");
            foreach (var action in planResult)
            {
                Debug.Log($"- {action.Name}");
            }

            // Start executing the plan
            StartCoroutine(ExecutePlan(planResult));
        }
        else
        {
            Debug.Log("GOAPExample: Failed to create a plan for the given goals.");
        }
    }

    // Property to get the current NPC status
    public NPCStatus CurrentNPCStatus
    {
        get
        {
            string location = npcState.LowerBody.ContainsKey("location") ? npcState.LowerBody["location"].ToString() : "unknown";
            string inventory = npcState.Inventory != null && npcState.Inventory.Count > 0 ? string.Join(", ", npcState.Inventory) : "none";
            string pose = npcState.LowerBody.ContainsKey("pose") ? npcState.LowerBody["pose"].ToString() : "unknown";
            string holding = npcState.UpperBody.ContainsKey("hold") ? npcState.UpperBody["hold"].ToString() : "none";
            string health = npcState.Resources.ContainsKey("health") ? npcState.Resources["health"].ToString() : "0";
            string mental = npcState.Resources.ContainsKey("mental") ? npcState.Resources["mental"].ToString() : "0";

            return new NPCStatus(location, inventory, pose, holding, health, mental);
        }
    }

    private IEnumerator ExecutePlan(List<GOAPAction> plan)
    {
        isExecutingPlan = true;

        foreach (var action in plan)
        {
            Debug.Log($"GOAPExample: Starting action '{action.Name}'.");

            if (IsMoveAction(action.Name))
            {
                string targetPlace = ExtractTargetPlaceFromMoveAction(action.Name);
                if (!string.IsNullOrEmpty(targetPlace))
                {
                    // NPC의 위치 업데이트
                    UpdateNPCLocation(targetPlace);

                    // NPC가 목적지에 도착할 때까지 대기
                    while (characterControl.IsMoving)
                    {
                        //Debug.Log($"GOAPExample: NPC is moving.");
                        yield return null;
                    }

                    // 도착 확인
                    Debug.Log($"GOAPExample: Arrived at '{targetPlace}'.");

                    // 위치 업데이트 (필요 시)
                    if (npcState.LowerBody.ContainsKey("location"))
                    {
                        npcState.LowerBody["location"] = targetPlace;
                        Debug.Log($"GOAPExample: NPCState 'location' updated to '{targetPlace}'.");
                    }
                    else
                    {
                        Debug.LogWarning($"GOAPExample: NPCState 'location' 키가 존재하지 않습니다.");
                    }
                }
                else
                {
                    Debug.Log($"GOAPExample: Failed to extract target place from action '{action.Name}'.");
                }
            }
            else if (IsGestureAction(action.Name))
            {
                string gestureName = ExtractGestureName(action.Name);
                if (!string.IsNullOrEmpty(gestureName))
                {
                    // Call CharacterControl's PerformGesture method
                    if (characterControl != null)
                    {
                        characterControl.PerformGesture(gestureName);
                        Debug.Log($"GOAPExample: Requested CharacterControl to perform gesture '{gestureName}'.");

                        // 제스처 효과 적용
                        string gestureFlag = $"did_{gestureName.ToLower()}";
                        if (action.Effects.ContainsKey(gestureFlag))
                        {
                            npcState.StateData[gestureFlag] = true;
                            Debug.Log($"GOAPExample: NPCState '{gestureFlag}' set to true.");
                        }

                        // 액션의 시간 비용만큼 대기
                        if (action.Cost.ContainsKey("time"))
                        {
                            yield return new WaitForSeconds(action.Cost["time"]);
                        }
                        else
                        {
                            yield return null;
                        }

                        // 제스처 후 상태 업데이트 (필요 시)
                        // 예: characterControl.UpdateToIdle(); // CharacterControl에서 처리
                    }
                    else
                    {
                        Debug.LogError("GOAPExample: CharacterControl reference is missing.");
                    }
                }
                else
                {
                    Debug.Log($"GOAPExample: Failed to extract gesture name from action '{action.Name}'.");
                }
            }
            else if (IsPickAction(action.Name) || IsDropAction(action.Name))
            {
                string itemName = ExtractItemNameFromAction(action.Name);
                if (!string.IsNullOrEmpty(itemName))
                {
                    InteractionControl interaction = FindObjectOfType<InteractionControl>();
                    if (interaction != null)
                    {
                        if (IsPickAction(action.Name))
                        {
                            interaction.PickUpItem(itemName, npcState, worldState);
                            Debug.Log($"GOAPExample: Picking up item '{itemName}'.");
                        }
                        else if (IsDropAction(action.Name))
                        {
                            interaction.DropItem(itemName, npcState, worldState);
                            Debug.Log($"GOAPExample: Dropping item '{itemName}'.");
                        }

                        // 액션의 시간 비용만큼 대기
                        if (action.Cost.ContainsKey("time"))
                        {
                            yield return new WaitForSeconds(action.Cost["time"]);
                        }
                        else
                        {
                            yield return null;
                        }
                    }
                    else
                    {
                        Debug.LogError("GOAPExample: InteractionControl script not found in the scene.");
                        yield break;
                    }
                }
                else
                {
                    Debug.Log($"GOAPExample: Failed to extract item name from action '{action.Name}'.");
                }
            }
            else if (IsUseAction(action.Name))
            {
                string itemName = ExtractItemNameFromUseAction(action.Name);
                if (!string.IsNullOrEmpty(itemName))
                {
                    InteractionControl interaction = FindObjectOfType<InteractionControl>();
                    if (interaction != null)
                    {
                        interaction.UseItem(itemName, npcState, worldState);
                        Debug.Log($"GOAPExample: Using item '{itemName}'.");

                        // 액션의 시간 비용만큼 대기
                        if (action.Cost.ContainsKey("time"))
                        {
                            yield return new WaitForSeconds(action.Cost["time"]);
                        }
                        else
                        {
                            yield return null;
                        }
                    }
                    else
                    {
                        Debug.LogError("GOAPExample: InteractionControl script not found in the scene.");
                        yield break;
                    }
                }
                else
                {
                    Debug.Log($"GOAPExample: Failed to extract item name from action '{action.Name}'.");
                }
            }
            else
            {
                // 'place_state' 키를 기반으로 장소 상호작용 처리
                bool isPlaceInteraction = false;
                foreach (var effect in action.Effects)
                {
                    if (effect.Key.StartsWith("place_state:", StringComparison.OrdinalIgnoreCase))
                    {
                        isPlaceInteraction = true;
                        break;
                    }
                }

                if (isPlaceInteraction)
                {
                    foreach (var effect in action.Effects)
                    {
                        if (effect.Key.StartsWith("place_state:", StringComparison.OrdinalIgnoreCase))
                        {
                            var parts = effect.Key.Split(':');
                            if (parts.Length == 3)
                            {
                                string placeName = parts[1].ToLower();
                                string stateKey = parts[2].ToLower();
                                object value = effect.Value;

                                UpdatePlaceState(placeName, stateKey, value);
                                Debug.Log($"GOAPExample: Updated place state '{placeName}.{stateKey}' to '{value}'.");

                                // 액션의 시간 비용만큼 대기
                                if (action.Cost.ContainsKey("time"))
                                {
                                    yield return new WaitForSeconds(action.Cost["time"]);
                                }
                                else
                                {
                                    yield return null;
                                }
                            }
                            else
                            {
                                Debug.LogWarning($"GOAPExample: Invalid place_state format in effect key '{effect.Key}'.");
                            }
                        }
                    }
                }
                else
                {
                    // 기타 액션 처리 (예: Think)
                    Debug.Log($"GOAPExample: Executing action '{action.Name}'.");

                    if (action.Cost.ContainsKey("time"))
                    {
                        yield return new WaitForSeconds(action.Cost["time"]);
                    }
                    else
                    {
                        yield return null;
                    }

                    // 액션 효과 적용
                    foreach (var effect in action.Effects)
                    {
                        npcState.StateData[effect.Key] = effect.Value;
                        Debug.Log($"GOAPExample: NPCState '{effect.Key}' set to '{effect.Value}'.");
                    }
                }
            }
        }

        isExecutingPlan = false;
        Debug.Log("GOAPExample: Plan execution completed.");
    }

    /// <summary>
    /// Checks if the action is a move action based on its name
    /// </summary>
    private bool IsMoveAction(string actionName)
    {
        return actionName.Contains("_to_", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Checks if the action is a gesture action based on its name
    /// </summary>
    private bool IsGestureAction(string actionName)
    {
        return gestureNames.Any(g => g.Equals(actionName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Checks if the action is a use action based on its name
    /// </summary>
    private bool IsUseAction(string actionName)
    {
        return actionName.StartsWith("use_", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Checks if the action is a pick action based on its name
    /// </summary>
    private bool IsPickAction(string actionName)
    {
        return actionName.StartsWith("pick_", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Checks if the action is a drop action based on its name
    /// </summary>
    private bool IsDropAction(string actionName)
    {
        return actionName.StartsWith("drop_", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Extracts the target place from a move action name
    /// Example: "sofa_to_meja" -> "meja"
    /// </summary>
    private string ExtractTargetPlaceFromMoveAction(string actionName)
    {
        string[] parts = actionName.Split(new string[] { "_to_" }, StringSplitOptions.None);
        if (parts.Length == 2)
        {
            return parts[1].ToLower(); // Return the part after "_to_"
        }
        return null;
    }

    /// <summary>
    /// Extracts the gesture name from a gesture action name
    /// Example: "thinking" -> "thinking"
    /// </summary>
    private string ExtractGestureName(string actionName)
    {
        // Since gesture action names are the same as gesture names
        return actionName.ToLower();
    }

    /// <summary>
    /// Extracts the item name from a pick or drop action name
    /// Example: "pick_map" -> "map", "drop_snack" -> "snack"
    /// </summary>
    private string ExtractItemNameFromAction(string actionName)
    {
        string[] parts = actionName.Split('_');
        if (parts.Length >= 2)
        {
            return string.Join("_", parts, 1, parts.Length - 1).ToLower();
        }
        return null;
    }

    /// <summary>
    /// Extracts the item name from a use action name
    /// Example: "use_snack" -> "snack"
    /// </summary>
    private string ExtractItemNameFromUseAction(string actionName)
    {
        string[] parts = actionName.Split('_');
        if (parts.Length >= 2)
        {
            return string.Join("_", parts, 1, parts.Length - 1).ToLower();
        }
        return null;
    }

    /// <summary>
    /// Updates the NPC's destination based on the target object name
    /// </summary>
    private void UpdateNPCLocation(string targetObjectName)
    {
        Debug.Log($"GOAPExample: Attempting to find target object '{targetObjectName}'.");
        Debug.Log($"GOAPExample: Total target objects: {targetObjects.Length}");

        GameObject targetObject = Array.Find(targetObjects, obj => obj.name.Equals(targetObjectName, StringComparison.OrdinalIgnoreCase));

        if (targetObject != null)
        {
            Debug.Log($"GOAPExample: Found target object '{targetObject.name}'. Setting destination.");

            // CharacterControl을 통해 목표 지점 설정
            if (characterControl != null)
            {
                characterControl.SetDestination(targetObject.transform.position);
                Debug.Log($"GOAPExample: CharacterControl's destination set to '{targetObject.transform.position}'.");
            }
            else
            {
                Debug.LogWarning("GOAPExample: CharacterControl 참조가 설정되지 않았습니다.");
            }
        }
        else
        {
            Debug.LogWarning($"GOAPExample: Could not find target object '{targetObjectName}'. Available target objects:");
            foreach (var obj in targetObjects)
            {
                Debug.Log($"- {obj.name}");
            }
        }
    }

    /// <summary>
    /// Updates the Place's state and calls the corresponding PlaceInteraction
    /// </summary>
    /// <param name="placeName">Name of the Place</param>
    /// <param name="key">State key</param>
    /// <param name="value">New value</param>
    public void UpdatePlaceState(string placeName, string key, object value)
    {
        if (Places.ContainsKey(placeName))
        {
            Place place = Places[placeName];
            place.State[key] = value;
            Debug.Log($"GOAPExample: Place '{placeName}'의 상태 '{key}'을 '{value}'로 업데이트했습니다.");

            // PlaceInteraction 스크립트 찾기 및 호출
            PlaceInteraction interaction = place.GameObject.GetComponent<PlaceInteraction>();
            if (interaction != null)
            {
                interaction.OnStateChanged(key, value);
                Debug.Log($"GOAPExample: PlaceInteraction '{interaction.GetType().Name}'을 통해 상태 변경을 처리했습니다.");
            }
            else
            {
                Debug.LogWarning($"GOAPExample: Place '{placeName}'에 PlaceInteraction 스크립트가 부착되어 있지 않습니다.");
            }
        }
        else
        {
            Debug.LogError($"GOAPExample: Place '{placeName}'을 찾을 수 없습니다.");
        }
    }

    // Ensure CharacterControl is assigned
    void OnValidate()
    {

    }
}