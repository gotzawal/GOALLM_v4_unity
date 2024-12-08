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
    [SerializeField] private InputField InputField;

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

        // Add listener to the InputField to detect when editing ends
        InputField.onEndEdit.AddListener(OnInputFieldEndEdit);
    }

    private void OnDestroy()
    {
        // Remove listener to prevent memory leaks
        InputField.onEndEdit.RemoveListener(OnInputFieldEndEdit);
    }

    public void OpenChatUI()
    {
        if (!_chatUI.activeInHierarchy) _chatUI.SetActive(true);
        InputField.interactable = true;
    }

    public void CloseChatUI()
    {
        if (_chatUI.activeInHierarchy) _chatUI.SetActive(false);
        InputField.interactable = false;
    }

    public void ToggleMainPanel()
    {
        if (_mainPanel.activeInHierarchy) _mainPanel.SetActive(false);
        else _mainPanel.SetActive(true);
    }

    public void ToggleSideMenuPanel(bool pressed)
    {
        _sideMenuPanel.SetActive(pressed);
    }

    public void ToggleQuestPanel()
    {
        if (_questPanel.activeInHierarchy) _questPanel.SetActive(false);
        else _questPanel.SetActive(true);
    }

    public void ToggleInventoryPanel()
    {
        if (_inventoryPanel.activeInHierarchy) _inventoryPanel.SetActive(false);
        else _inventoryPanel.SetActive(true);
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

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (_chatUI.activeInHierarchy)
            {
                if (!InputField.isFocused)
                {
                    isFocused = true;
                    InputField.ActivateInputField();
                }
                else
                {
                    isFocused = false;
                    // Optionally unfocus the input field
                    // InputField.DeactivateInputField();
                }
            }
        }
    }

    private void OnInputFieldEndEdit(string text)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(text))
            {
                SendMessage_F(text);
                isFocused = false;

                // Reactivate the input field for continuous typing
                InputField.ActivateInputField();
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Error in OnInputFieldEndEdit: " + ex.Message);
        }
    }

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

    public enum ChatType
    {
        PLAYER,
        MAID
    }

    public void SendMessage_F(string message)
    {
        try
        {
            Debug.Log("SendMessage_F called with message: " + message);

            if (string.IsNullOrWhiteSpace(message))
                return; // Do not send empty messages

            ChatData.Add(ConstData.ChatType.PLAYER);
            Image temp = Instantiate_ChatBox();
            temp.GetComponent<chatElement>().SetData(message, (ConstData.ChatType.PLAYER));
            temp.GetComponent<GridLayoutGroup>().childAlignment = TextAnchor.UpperRight;

            // Pass the message to the GameManager to send to the server
            if (GameManagerServerSet.instance != null)
            {
                GameManagerServerSet.instance.OnInputFieldSubmit(message);
            }
            else
            {
                Debug.LogError("GameManagerServerSet.instance is null.");
            }

            InputField.text = ""; // Clear input field
            InputField.ActivateInputField(); // Keep the InputField focused
        }
        catch (Exception ex)
        {
            Debug.LogError("Error in SendMessage_F: " + ex.Message);
        }
    }

    public void ReceiveMessage(string data) // 'data' is the NPC's text
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

    public void SetTokenCount(int value)
    {
        TokenSlider.value = value;
        txt_TokenText.text = "PHASE COUNT " + value.ToString() + "/  50";
    }

    public void SetHeartSlider(float value)
    {
        HeartSlider.value = value/2;
    }

    public void SetFriendShipSlider(float value)
    {
        FriendShipSlider.value = value/2;
    }

    /// <summary>
    /// 서버로부터 받은 퀘스트 배열을 UI에 표시합니다.
    /// </summary>
    /// <param name="quests">퀘스트 배열</param>
    public void UpdateQuestDisplay(GameManagerServerSet.Quest[] quests)
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


}