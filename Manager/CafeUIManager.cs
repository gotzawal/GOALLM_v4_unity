using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshPro 사용을 위해 추가

public class CafeUIManager : MonoBehaviour
{
    public static CafeUIManager instance;

    [SerializeField] private GameObject _chatUI;
    [SerializeField] private GameObject _mainPanel;
    [SerializeField] private GameObject _sideMenuPanel;
    [SerializeField] private GameObject _questPanel;
    [SerializeField] private GameObject _inventoryPanel;

    [SerializeField] private Button _sideMenuOpenButton;
    [SerializeField] private Button _sideMenuCloseButton;
    [SerializeField] private Button _questButton;
    [SerializeField] private Button _inventoryButton;
    [SerializeField] private Button _ExitButton;

    [SerializeField] private Slider TokenSlider;
    [SerializeField] private Text txt_TokenText;

    [SerializeField] private Slider HeartSlider;
    [SerializeField] private Slider FriendShipSlider;

    [SerializeField] private List<ConstData.ChatType> ChatData;

    [SerializeField] private GameObject InventoryGroup = null;
    [SerializeField] private GameObject InventoryElementPrefeb = null;

    [SerializeField] private List<Image> ChatBoxGroup;
    [SerializeField] private List<Image> ChatBgGroup;
    [SerializeField] private List<Text> ChatTextGroup;

    [SerializeField] private GameObject ChatView;
    [SerializeField] private Image ChatObject;

    [SerializeField] private Transform questListContainer; // 퀘스트 아이템을 담을 부모 오브젝트
    [SerializeField] private GameObject questItemPrefab; // QuestItemUI 프리팹

    private float opacityValue = 20f;
    public bool isFocused = false;

    private void Awake()
    {
        if (instance != null) return;
        instance = this;
    }

    private void Start()
    {
        AddAllButtonListeners();

        // InputField 관련 리스너는 GameManagerServerSet으로 이전되었으므로 제거
    }

    private void OnDestroy()
    {
        // 버튼 리스너 제거
        RemoveAllButtonListeners();
    }

    public void OpenChatUI()
    {
        if (!_chatUI.activeInHierarchy) _chatUI.SetActive(true);
        // InputField의 인터랙티브 상태는 GameManagerServerSet에서 관리
    }

    public void CloseChatUI()
    {
        if (_chatUI.activeInHierarchy) _chatUI.SetActive(false);
        // InputField의 인터랙티브 상태는 GameManagerServerSet에서 관리
    }

    public void ToggleMainPanel()
    {
        _mainPanel.SetActive(!_mainPanel.activeInHierarchy);
    }

    public void ToggleSideMenuPanel(bool pressed)
    {
        _sideMenuPanel.SetActive(pressed);
    }

    public void ToggleQuestPanel()
    {
        _questPanel.SetActive(!_questPanel.activeInHierarchy);
    }

    public void ToggleInventoryPanel()
    {
        _inventoryPanel.SetActive(!_inventoryPanel.activeInHierarchy);
    }

    public void ExitGame()
    {
        Fade.Out(0.5f, () =>
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit(); // Quit the application
#endif
        });
    }

    public void AddAllButtonListeners()
    {
        _sideMenuOpenButton.onClick.AddListener(() => ToggleSideMenuPanel(true));
        _sideMenuCloseButton.onClick.AddListener(() => ToggleSideMenuPanel(false));
        _questButton.onClick.AddListener(() => ToggleQuestPanel());
        _inventoryButton.onClick.AddListener(() => ToggleInventoryPanel());
        _ExitButton.onClick.AddListener(() => ExitGame());
    }

    public void RemoveAllButtonListeners()
    {
        _sideMenuOpenButton.onClick.RemoveAllListeners();
        _sideMenuCloseButton.onClick.RemoveAllListeners();
        _questButton.onClick.RemoveAllListeners();
        _inventoryButton.onClick.RemoveAllListeners();
        _ExitButton.onClick.RemoveAllListeners();
        ToggleSideMenuPanel(false);
    }

    /// <summary>
    /// 플레이어의 메시지를 챗 UI에 표시합니다.
    /// GameManagerServerSet에서 호출됩니다.
    /// </summary>
    /// <param name="message">플레이어의 메시지</param>
    public void DisplayPlayerMessage(string message)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(message))
                return; // 빈 메시지는 표시하지 않음

            ChatData.Add(ConstData.ChatType.PLAYER);
            Image temp = Instantiate_ChatBox();
            temp.GetComponent<chatElement>().SetData(message, ConstData.ChatType.PLAYER);
            temp.GetComponent<GridLayoutGroup>().childAlignment = TextAnchor.UpperRight;
        }
        catch (Exception ex)
        {
            Debug.LogError("Error in DisplayPlayerMessage: " + ex.Message);
        }
    }

    /// <summary>
    /// 서버로부터 받은 NPC의 메시지를 챗 UI에 표시합니다.
    /// </summary>
    /// <param name="data">NPC의 메시지</param>
    public void ReceiveMessage(string data)
    {
        try
        {
            ChatData.Add(ConstData.ChatType.MAID);
            Image temp = Instantiate_ChatBox();
            temp.GetComponent<chatElement>().SetData(data, ConstData.ChatType.MAID);
            temp.GetComponent<GridLayoutGroup>().childAlignment = TextAnchor.UpperLeft;
        }
        catch (Exception ex)
        {
            Debug.LogError("Error in ReceiveMessage: " + ex.Message);
        }
    }

    /// <summary>
    /// 토큰 수를 설정하고 UI를 업데이트합니다.
    /// </summary>
    /// <param name="value">토큰 수</param>
    public void SetTokenCount(int value)
    {
        TokenSlider.value = value;
        txt_TokenText.text = "PHASE COUNT " + value.ToString() + "/  50";
    }

    /// <summary>
    /// 하트 슬라이더를 설정합니다.
    /// </summary>
    /// <param name="value">하트 값</param>
    public void SetHeartSlider(float value)
    {
        HeartSlider.value = value / 2;
    }

    /// <summary>
    /// 우정 슬라이더를 설정합니다.
    /// </summary>
    /// <param name="value">우정 값</param>
    public void SetFriendShipSlider(float value)
    {
        FriendShipSlider.value = value / 2;
    }

    /// <summary>
    /// 서버로부터 받은 퀘스트 배열을 UI에 표시합니다.
    /// </summary>
    /// <param name="quests">퀘스트 배열</param>
    public void UpdateQuestDisplay(GameManager.Quest[] quests)
    {
        if (questListContainer == null)
        {
            Debug.LogError("questListContainer가 할당되지 않았습니다.");
            return;
        }

        if (questItemPrefab == null)
        {
            Debug.LogError("questItemPrefab이 할당되지 않았습니다.");
            return;
        }

        // 기존 퀘스트 아이템 삭제
        foreach (Transform child in questListContainer)
        {
            Destroy(child.gameObject);
        }

        // 새로운 퀘스트 아이템 생성
        foreach (var quest in quests)
        {
            GameObject questItemObj = Instantiate(questItemPrefab, questListContainer);
            QuestItemUI questItemUI = questItemObj.GetComponent<QuestItemUI>();
            if (questItemUI != null)
            {
                questItemUI.SetQuestData(quest);
            }
            else
            {
                Debug.LogError("QuestItemUI 컴포넌트를 찾을 수 없습니다.");
            }
        }
    }

    /// <summary>
    /// 챗 박스를 인스턴스화하고 불투명도를 조절합니다.
    /// </summary>
    /// <returns>인스턴스화된 챗 박스의 Image 컴포넌트</returns>
    private Image Instantiate_ChatBox()
    {
        Image temp = Instantiate(ChatObject, ChatView.transform);
        ChatBoxGroup.Add(temp);
        ChatBgGroup.Add(temp.transform.GetChild(0).GetComponent<Image>());
        ChatTextGroup.Add(temp.transform.GetChild(0).GetChild(0).GetComponent<Text>());

        for (int i = 0; i < ChatBoxGroup.Count; i++)
        {
            float opacity = 100f;
            opacity = opacity - (((ChatBoxGroup.Count - 2) - i) * opacityValue);
            opacity /= 100;

            Color c = ChatBgGroup[i].color;
            c.a = opacity;
            ChatBgGroup[i].color = c;

            c = ChatTextGroup[i].color;
            c.a = opacity;
            ChatTextGroup[i].color = c;
        }

        return temp;
    }
}
