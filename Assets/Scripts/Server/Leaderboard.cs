using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine.UI;

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
    //SSL connection error, branch out to this later because https is too encrypted
    public async Task<string> getRequest(string url)
    {
        using (HttpClient client = new HttpClient())
        {
            try
            {
                Debug.Log(url);
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                string[] pages = url.Split('/');

                Debug.Log(pages[pages.Length - 1] + ":\nReceived: " + responseBody);
                return responseBody;
            }
            catch (HttpRequestException e)
            {
                string[] pages = url.Split('/');

                Debug.LogError(pages[pages.Length - 1] + ": Error: " + e.Message);
                return null;
            }
        }
    }
    public async Task<string> postRequest(string url, string data)
    {
        try
        {
            using (HttpClient client = new HttpClient())
            {
                Debug.Log(url);
                //Application of polymorphism
                HttpContent content = new StringContent(data);
                HttpResponseMessage response = await client.PostAsync(url, content);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                string[] pages = url.Split('/');

                Debug.Log(pages[pages.Length - 1] + ":\nReceived: " + responseBody);
                return responseBody; 

        
            }

        } catch (HttpRequestException e)
        {
            string[] pages = url.Split('/');
            Debug.LogError(pages[pages.Length - 1] + ": Error: " + e.Message);
            return null;
        }
    }

}
//up for modi
public class Highscore
{
    public string score;
    public string date; //[sep date class with timezone included];
}
public class Leaderboard : MonoBehaviour
{
    private RequestPacket apiTest = new RequestPacket("https://12d0099a-1529-4de2-9468-c224649003b1-00-187mkm0yvrntp.janeway.replit.dev:3000/amountOfUsers");
    private RequestPacket scoreStorage = new RequestPacket("http://localhost:3000/");
    public string[] scoresArray;
    //leaderboard ui section
    public GameObject scrollPanel;
    public float scrollPanelHeight;
    public GameObject placeholder;
    private async void Start() 
    {
 
        string scoreData = await scoreStorage.getRequest(scoreStorage.getUrl() + "getAllScores");
        scoresArray = formatScores(scoreData);
        RectTransform ScrollPanelRT = scrollPanel.GetComponent<RectTransform>();
        scrollPanelHeight = ScrollPanelRT.sizeDelta.y;
        createScoreBoard(ScrollPanelRT);
        await scoreStorage.postRequest(scoreStorage.getUrl() + "addScore", 10000.ToString());

        //StartCoroutine(getRequest(apiTest.getUrl()));
        //StartCoroutine(getRequest(serverGetTest.getUrl()));
    }
    private void createScoreBoard(RectTransform ScrollPanelRT)
    {
        //crate method that sorts scores from greatest to least.

        for(int i = 0; i < scoresArray.Length; i++)
        {
            GameObject scoreText = Instantiate(placeholder, scrollPanel.transform);
            scoreText.transform.GetChild(0).GetComponent<Text>().text = "Score " + (i + 1).ToString() + ":";
            scoreText.transform.GetChild(1).GetComponent<Text>().text = string.Format("{0:n0}", int.Parse(scoresArray[i]));
        }
    }
    //branch out to later
    string[] formatScores(string scoreData)
    {
        return scoreData.Split(',');
    }
    private void Update()
    {
        
    }






    //backup code, using asynchrnous instead of coruotinges.
    IEnumerator getRequest(string url)
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
                    yield return webRequest.downloadHandler.text;
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
 
}
