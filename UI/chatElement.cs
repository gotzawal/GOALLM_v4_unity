using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class chatElement : MonoBehaviour
{
    [SerializeField] private Text txt_Chat = null;

    [SerializeField] private string dialogData = null;

    [SerializeField] private float typingSpeed = 0.05f;

    [SerializeField] private Sprite img_playerChat = null;
    [SerializeField] private Sprite img_maidChat = null;

    [SerializeField] private Image img_Bg = null;

    Coroutine chatCoroutine = null;
    public void SetData(string value , ConstData.ChatType type) // 0 = maid , 1 = player
    {
        if(type == ConstData.ChatType.MAID)
        {
            img_Bg.sprite = img_maidChat;
        }
        else if( type == ConstData.ChatType.PLAYER)
        {
            img_Bg.sprite = img_playerChat;
        }

        dialogData = value;
        if (!gameObject.activeInHierarchy)
        {

            txt_Chat.text = dialogData;
            return;
        }
        chatCoroutine = StartCoroutine(OnTypingText());
    }

    public void OnDisable()
    {
        if(chatCoroutine != null)
        {   
            StopCoroutine(chatCoroutine);
        }
        txt_Chat.text = dialogData;
    }
    IEnumerator OnTypingText()
    {
        int index = 0;
        txt_Chat.text = "";
        while (index < dialogData.Length)
        {
            txt_Chat.text += dialogData[index];
            index++;

            yield return new WaitForSeconds(typingSpeed);
        }

    }
    
}
