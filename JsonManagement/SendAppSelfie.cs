using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using System;
using UnityEngine.UI;

public class SendAppSelfie : MonoBehaviour
{
    // The API endpoint to send the image to
    public string apiUrl = "http://13.126.56.105:443/api/whatsapp/whatsapp-upload";

    // The image to send
    public RawImage preview;
    string imageString;

    public void SendSelfie()
    {
        Texture2D img = (Texture2D)preview.texture;
        // Attach the image as a form field
        byte[] imageData = img.EncodeToPNG();

        imageString = Convert.ToBase64String(imageData);

        StartCoroutine(UploadImage(imageData));
    }
    //___________________________________________________________________________________________________________________________________________________________//

    #region Create json file for phone number
    /*create json file for phone number
    Ph_no_JsonController data1 = new Ph_no_JsonController();
    data1.phone = UserRegistrationController.phone;

    string phone_json = JsonUtility.ToJson(data1, true);

    StartCoroutine(postRequest("http://13.126.56.105:443/api/whatsapp/whatsapp-upload", phone_json));// send whtasapp number

}

//POST Data to the cloud server
IEnumerator postRequest(string url, string json)
{
    var uwr = new UnityWebRequest(url, "POST");
    byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(json);
    uwr.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
    uwr.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
    uwr.SetRequestHeader("Content-Type", "application/json");

    //Send the request then wait here until it returns
    yield return uwr.SendWebRequest();

    if (uwr.isNetworkError)
    {
        Debug.Log("Error While Sending: " + uwr.error);
    }
    else
    {
        Debug.Log("Received: " + uwr.downloadHandler.text);
    }
}

    */
    #endregion

    IEnumerator UploadImage(byte[] imageData)
    {
        WWWForm form = new WWWForm();
        form.AddField("phone", UserRegistrationController.phone.ToString());
        form.AddBinaryData("image", imageData);
        
        UnityWebRequest www = UnityWebRequest.Post("http://13.126.56.105:443/api/whatsapp/whatsapp-upload", form);
        www.chunkedTransfer = false;

        yield return www.SendWebRequest();
        Debug.Log(www.downloadHandler.text);
    }
}
