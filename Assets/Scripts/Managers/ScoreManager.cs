using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;
public class ScoreManager : MonoBehaviour
{
    public static int score;
    public static int highScore;
    public static int MonsterKills;
    public static bool isHighScore;
    public static bool highsScoreChanged;
    private RequestPacket scoreStorage = new RequestPacket("http://localhost:3000/");
    Text text;
    public GameObject CheatMode;

    void Awake ()
    {
        text = GetComponent <Text> ();
        score = 0;
        MonsterKills = 0;
    }
    private void Start()
    {
        score = 0;
        CheatMode = GameObject.Find("CheatMode");
    }
   
    void Update ()
    {
        text.text = "Score: " + score;
        if(score >= highScore && CheatMode != null)
        {
            highScore = score;
            isHighScore = true;
 
        } else
        {
            isHighScore = false;
        }
    }
}


