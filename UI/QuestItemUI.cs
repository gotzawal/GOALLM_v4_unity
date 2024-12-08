using UnityEngine;
using TMPro; // TextMeshPro 사용을 위해 추가

public class QuestItemUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI questNameText; // TextMeshProUGUI로 변경
    [SerializeField] private TextMeshProUGUI questCompletedText; // TextMeshProUGUI로 변경

    /// <summary>
    /// 퀘스트 데이터를 UI에 설정합니다.
    /// </summary>
    /// <param name="quest">설정할 퀘스트 데이터</param>
    public void SetQuestData(GameManagerServerSet.Quest quest)
    {
        if (quest == null)
        {
            Debug.LogError("SetQuestData: quest 데이터가 null입니다.");
            return;
        }

        questNameText.text = quest.questName;

        // 퀘스트 완료 상태에 따라 텍스트와 색상을 설정합니다.
        if (quest.isCompleted.Equals("Cleared", System.StringComparison.OrdinalIgnoreCase))
        {
            questCompletedText.text = " True";
            questCompletedText.color = Color.green;
        }
        else
        {
            questCompletedText.text = "False";
            questCompletedText.color = Color.red;
        }
    }
}