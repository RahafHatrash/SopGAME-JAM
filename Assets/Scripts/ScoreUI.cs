using UnityEngine;
using UnityEngine.UI;

public class ScoreUI : MonoBehaviour
{
    [Header("UI References")]
    public Text scoreText;
    public Text highScoreText;
    
    void Start()
    {
        // Find ScoreManager and connect UI
        ScoreManager scoreManager = FindFirstObjectByType<ScoreManager>();
        if (scoreManager != null)
        {
            scoreManager.scoreText = scoreText;
            scoreManager.highScoreText = highScoreText;
            scoreManager.UpdateUI();
        }
        else
        {
            Debug.LogWarning("[ScoreUI] ScoreManager not found!");
        }
    }
}
