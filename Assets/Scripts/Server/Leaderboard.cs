using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
public class RequestPacket
{
    private string url;
    private string data;
    private string contentType;
    public RequestPacket(string url, string data, string contentType)
    {
        this.url = url;
        this.data = data;
        this.contentType = contentType;
    }
    public RequestPacket(string url, string data)
    {
        this.url = url;
        this.data = data;
        this.contentType = "application/json";
    }
    public RequestPacket(string url)
    {
        this.url = url;
        this.contentType = "applications/json";
    }
    //incase I need to modify how data is retrieved in the codebase or for security measures
    public string getUrl() { return url; }
    public string getData() { return data; }
    public string getContentType() { return contentType; }

}
//up for modi
public class Highscore
{
    public string score;
    public string date; //[sep date class with timezone included];
}
public class Leaderboard : MonoBehaviour
{
    RequestPacket apiTest = new RequestPacket("https://12d0099a-1529-4de2-9468-c224649003b1-00-187mkm0yvrntp.janeway.replit.dev:3000/amountOfUsers");
    void Start()
    {
        StartCoroutine(GetRequest(apiTest.getUrl()));

    }
    IEnumerator GetRequest(string url)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            string[] pages = url.Split('/');
            int page = pages.Length - 1;

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:

                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError(pages[page] + ": Error: " + webRequest.error);
                    break;

                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError(pages[page] + ": HTTP Error: " + webRequest.error);
                    break;

                case UnityWebRequest.Result.Success:
                    Debug.Log(pages[page] + ":\nReceived: " + webRequest.downloadHandler.text);
                    break;
            }
        }
    }
    IEnumerator postRequest(RequestPacket scorePacket)
    {
        using (UnityWebRequest www = UnityWebRequest.Post(scorePacket.getUrl(), scorePacket.getData(), scorePacket.getContentType()))
        {
            yield return www.SendWebRequest();
            //modify condition to include waiting time if appilcable.
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(www.error);
            }
            else
            {
                Debug.Log("Form upload complete!");
            }
        }
    }
    // Update is called once per frame
    void Update()
    {

    }
}
