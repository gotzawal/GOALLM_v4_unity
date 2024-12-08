using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System;


[System.Serializable]
public class UserInputData
{
    public string userResponse;
    public string anything; // things that will be added
}


[System.Serializable]
public class ResponseData
{
    public string response_text;
    public string response_audio;
    public int reaction;
}

public class ChatManager : MonoBehaviour
{
    public string url;

    // use this function this way:

    // StartCoroutine(chatManager.SendMessageToServer(userInput, (responseText, audioClip, reaction) =>
    // {
    //     ReceiveMessage(responseText, audioClip, reaction);
    // }));

    // where ReceiveMessage function utilize response text, audioclip, and reaction 

    public IEnumerator SendMessageToServer(UserInputData userInput, Action<string, AudioClip, int> onResponse)
    {
        UnityWebRequest www = new UnityWebRequest(url, "POST");
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(JsonUtility.ToJson(userInput));
        www.uploadHandler = new UploadHandlerRaw(jsonToSend);
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "application/json");

        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            try
            {
                ResponseData data = JsonUtility.FromJson<ResponseData>(www.downloadHandler.text);
                string responseText = data.response_text;
                byte[] audioBytes = System.Convert.FromBase64String(data.response_audio);
                AudioClip audioClip = WavUtility.ToAudioClip(audioBytes, "ResponseClip");
                onResponse?.Invoke(responseText, audioClip, data.reaction);
            }
            catch (System.Exception)
            {
                Debug.LogError("Server communication failed, " + www.error);
                onResponse?.Invoke("Sorry... can you say that again?", null, 0);
            }
        }
        else
        {
            Debug.LogError("Server communication failed, " + www.error);
            onResponse?.Invoke("Sorry... can you say that again?", null, 0);
        }
    }
}
