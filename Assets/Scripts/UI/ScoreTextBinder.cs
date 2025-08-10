using UnityEngine;
using UnityEngine.UI;

public class ScoreTextBinder : MonoBehaviour
{
    [SerializeField] private Text scoreText;
    [SerializeField] private bool showHighScoreAlso = false;

    private void Awake()
    {
        if (scoreText == null) scoreText = GetComponent<Text>();
    }

    private void OnEnable()
    {
        GameEvents.OnScoreChanged += HandleScoreChanged;
        // Initial fill
        HandleScoreChanged(ScoreManager.GetScore());
    }

    private void OnDisable()
    {
        GameEvents.OnScoreChanged -= HandleScoreChanged;
    }

    private void HandleScoreChanged(int score)
    {
        if (scoreText == null) return;
        if (showHighScoreAlso)
        {
            scoreText.text = $"Score: {score}  High: {ScoreManager.GetHighScore()}";
        }
        else
        {
            scoreText.text = $"Score: {score}";
        }
    }
}
