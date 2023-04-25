///<comments>
/// To store multiple dynamic data entries in a JSON file using a serializable class and append new data without deleting the previous data,
/// you need to first retrieve the existing data from Firebase and then append the new data before uploading it back.
///</comments>

using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using System;

namespace Tropyverse
{
    public class SendPlayerData : MonoBehaviour
    {
        [Tooltip("Replace with your Firebase Realtime Database URL")]
        [SerializeField] private string firebaseUrl = "https://tropyverse-e25f5-default-rtdb.firebaseio.com/";

        private void Start()
        {
            // Create a new DataObject instance to append
            PlayerDataJson newData = new PlayerDataJson { name = PhotonNetwork.NickName, timeStamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") };
            StartCoroutine(AppendData(newData));
        }

        // Coroutine to get current data from Firebase
        private IEnumerator GetData(System.Action<string> onResult)
        {
            string path = "PlayerData/data.json";
            using (UnityWebRequest request = UnityWebRequest.Get(firebaseUrl + path))
            {
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError(request.error);
                    onResult(null);
                }
                else
                {
                    onResult(request.downloadHandler.text);
                }
            }
        }

        // Coroutine to post updated data to Firebase
        private IEnumerator PostData(string json)
        {
            string path = "PlayerData/data.json";
            using (UnityWebRequest request = UnityWebRequest.Put(firebaseUrl + path, json))
            {
                request.method = UnityWebRequest.kHttpVerbPUT;
                request.SetRequestHeader("Content-Type", "application/json");
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError(request.error);
                }
                else
                {
                    Debug.Log("Data uploaded successfully.");
                }
            }
        }

        // Coroutine to append new data to the existing data and post the updated data to Firebase
        private IEnumerator AppendData(PlayerDataJson newData)
        {
            // Get the current data from Firebase
            yield return GetData(json =>
            {
                if (json != null)
                {
                    // Deserialize the JSON string to a SerializableList<PlayerData>
                    SerializableList<PlayerDataJson> playerDatas = JsonUtility.FromJson<SerializableList<PlayerDataJson>>(json);

                    // Append the new data
                    playerDatas.Items.Add(newData);

                    // Serialize the updated list back to a JSON string
                    string updatedJson = JsonUtility.ToJson(playerDatas);

                    // Post the updated data back to Firebase
                    StartCoroutine(PostData(updatedJson));
                }
            });
        }
    }
}