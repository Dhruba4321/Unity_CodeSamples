using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Text.RegularExpressions;
using TMPro;


public class UserRegistrationController : MonoBehaviour
{
    public TMP_InputField user_Name, user_Email, user_phone, vechile_No;
    public static string name, email, numberPlateName;
    public static long phone;
    public TextMeshProUGUI name_Error_Message, email_Error_Message, phone_Error_Message, vechileNo_Error_Message;

    public bool isEmail; //To validate email id

    public SceneTransitionManager sceneTransitionManager;
    
    //public JsonController[] jsonControllers = new JsonController[10];
     

    #region Sending Input Fields Data to Server

    /* Send Data to Google Form
    [SerializeField]
    private string BASE_URL = "https://docs.google.com/forms/u/0/d/e/1FAIpQLSd2t5bJMLtnLNX-0eaUZz6P6TMMC3L0az5JopacShRl3V5zIQ/formResponse";

    IEnumerator Post(string Name, string Email, string phone, string numberPlateName)
    {
        WWWForm form = new WWWForm();
        form.AddField("entry.1667268058", Name);
        form.AddField("entry.1246579663", Email);
        form.AddField("entry.1692871743", phone);
        form.AddField("entry.1328673119", numberPlateName);

        byte[] rawData = form.data;
        WWW www = new WWW(BASE_URL, rawData);
        yield return www;
    }

    */

    public void SendData()
    {
        name = user_Name.text;
        email = user_Email.text;
        phone = long.Parse(user_phone.text);
        numberPlateName = vechile_No.text;

        isEmail = Regex.IsMatch(email, @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z", RegexOptions.IgnoreCase);

        #region ERROR Logic for the empty input Fields

        if (name.Length <= 2)
        {
            name_Error_Message.text = "*Please Enter Valid Name";
        }
        else if (name.Length > 2)
        {
            name_Error_Message.text = "";
        }

        if (phone< 1000000000)
        {
            phone_Error_Message.text = "*Please Enter Valid phone Number";
        }
        else
        {
            phone_Error_Message.text = "";
        }

        if (numberPlateName.Length >= 4)
        {
            vechileNo_Error_Message.text = "";           
        }
        else if(numberPlateName.Length < 4)
        {           
            vechileNo_Error_Message.text = "Please Enter a minimun 4 AlphaNumaric Character";
        }


        #endregion



        //create json file for registration data 
        JsonController data = new JsonController();
        data.name = name;
        data.email = email;
        data.phone = phone;
        data.numberPlateName = numberPlateName;

        string json = JsonUtility.ToJson(data, true);
        Utils.CreateAndSaveFile(Application.persistentDataPath, "Registration", ".json", json);


        if (name.Length > 2 && numberPlateName.Length >= 4 && phone>1000000000)
        {
            //StartCoroutine(Post(name, email, phone, numberPlateName)); //Send data to google sheet
            sceneTransitionManager.GoToScene(2); //Scene Transition Start
            StartCoroutine(postRequest("http://13.126.56.105:443/api/user/register-user", json));// send registration data           
        }     
    }

    //Send Data to the cloud server
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
    #endregion


    #region sending all the datas to the Json file
    /*public void SaveToJson() 
    {
        JsonController data = new JsonController();
        data.name = name;
        data.email = email;
        data.phone_no = phone;

        jsonControllers[0] = new JsonController();
        jsonControllers[0].name = name;
        jsonControllers[0].email = email;
        jsonControllers[0].phone = phone;
        jsonControllers[0].numberPlateName = numberPlateName;

        string jData = JsonHelper.ToJson(jsonControllers, true);
        Utils.CreateAndSaveFile(Application.persistentDataPath, "Registration", ".json", jData);
        //File.WriteAllText(Application.persistentDataPath + "/UserRegistrationData.json", json);
        
    }

    

    /*load all datas from Json
    public void LoadFromJson()
    {
        string json = File.ReadAllText(Application.dataPath + "/UserRegistrationData.json");
        JsonController data = JsonUtility.FromJson<JsonController>(json);

        user_Name.text= data.name;
        user_Email.text = data.email;
        user_phone.text = data.phone_no;
    }
    
    */

    #endregion

}
