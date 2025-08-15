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
		Debug.Log("[Result] Starting result screen initialization...");
		
		// Check if ScoreManager instance exists
		var smInstance = ScoreManager.Instance;
		Debug.Log($"[Result] ScoreManager.Instance exists: {smInstance != null}");
		
		if (smInstance != null)
		{
			Debug.Log($"[Result] ScoreManager instance name: {smInstance.gameObject.name}");
			Debug.Log($"[Result] ScoreManager scene: {smInstance.gameObject.scene.name}");
			Debug.Log($"[Result] ScoreManager is DontDestroyOnLoad: {smInstance.gameObject.scene.name == "DontDestroyOnLoad"}");
		}
		
		// Wait a frame to ensure singletons are properly initialized
		await System.Threading.Tasks.Task.Yield();
		
		Debug.Log("[Result] Initializing result screen");
		
		// Use finalized run snapshot if available
		if (ScoreManager.HasFinalizedRun())
		{
			Debug.Log("[Result] Using finalized run stats");
			var stats = ScoreManager.GetLastRunStats();
			Debug.Log($"[Result] Stats - Score: {stats.score}, High: {stats.highScore}, Kills: {stats.monsterKills}");
			
			highscoreText.text = stats.highScore.ToString();
			MonstersKilled.text = stats.monsterKills.ToString();
			score.text = stats.score.ToString();
		}
		else
		{
			Debug.Log("[Result] Using live values (fallback)");
			var liveScore = ScoreManager.GetScore();
			var liveHigh = ScoreManager.GetHighScore();
			var liveKills = ScoreManager.GetMonsterKills();
			
			Debug.Log($"[Result] Live values - Score: {liveScore}, High: {liveHigh}, Kills: {liveKills}");
			
			highscoreText.text = liveHigh.ToString();
			MonstersKilled.text = liveKills.ToString();
			score.text = liveScore.ToString();
		}

		// Post only if we have a valid run and are in expected scene
		if (!hasPosted && ScoreManager.HasFinalizedRun() && SceneManager.GetActiveScene().name == "Menu 1")
		{
			var stats = ScoreManager.GetLastRunStats();
			if (stats.score >= stats.highScore && stats.score > 0)
			{
				Debug.Log($"[Result] Posting high score: {stats.highScore}");
				await scoreStorage.postRequest(scoreStorage.getUrl() + "addScore", stats.highScore.ToString());
				hasPosted = true;
			}
		}
		
		Debug.Log("[Result] Result screen initialization complete");
	}
	
	// Update is called once per frame
	void Update () {

 
	}
}
