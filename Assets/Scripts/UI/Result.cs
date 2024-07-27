using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Result : MonoBehaviour {
    public Text highscoreText;
    public Text MonstersKilled;
    public Text score;
	private RequestPacket scoreStorage = new RequestPacket("http://localhost:3000/");
	private bool hasPosted = false;
	// Use this for initialization
	async void Start () {
		highscoreText.text = ScoreManager.highScore.ToString();
		MonstersKilled.text = ScoreManager.MonsterKills.ToString();
		score.text = ScoreManager.score.ToString();

		if (!hasPosted && ScoreManager.score > 0 && SceneManager.GetActiveScene().name == "Menu 1")
		{
			Debug.Log(ScoreManager.highScore.ToString());
			if (ScoreManager.score >= ScoreManager.highScore)
			{
				Debug.Log(ScoreManager.highScore);
				await scoreStorage.postRequest(scoreStorage.getUrl() + "addScore", ScoreManager.highScore.ToString());
				hasPosted = true;
			}
		}
	}
	
	// Update is called once per frame
	void Update () {

 
	}
}
