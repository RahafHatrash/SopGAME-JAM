using UnityEngine;
using UnityEngine.UI;
using TMPro; // للـ TextMeshPro
using UnityEngine.UI; // للـ UI القديم


public class ScoreUIManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI scoreText;
    
    [Header("Score Settings")]
    public int currentScore = 0;
    public int highScore = 0;
    
    void Start()
    {
        // Load high score
        highScore = PlayerPrefs.GetInt("HighScore", 0);
        
        // Load current score from previous scene (if exists)
        currentScore = PlayerPrefs.GetInt("CurrentScore", 0);
        
        UpdateUI();
        
        Debug.Log($"[ScoreUIManager] Score UI initialized! Current: {currentScore}, High: {highScore}");
    }
    
    public void AddScore(int points)
    {
        currentScore += points;
        
        // Save current score
        PlayerPrefs.SetInt("CurrentScore", currentScore);
        PlayerPrefs.Save();
        
        UpdateUI();
        Debug.Log($"[ScoreUIManager] Added {points} points! Total: {currentScore}");
    }

    void UpdateUI()
    {
        if (scoreText != null)
            scoreText.text = "Score: " + currentScore.ToString();
    }
    
    public void ResetScore()
    {
        currentScore = 0;
        PlayerPrefs.SetInt("CurrentScore", 0);
        PlayerPrefs.Save();
        UpdateUI();
    }
    
    // Save score when changing scenes
    void OnDestroy()
    {
        PlayerPrefs.SetInt("CurrentScore", currentScore);
        PlayerPrefs.Save();
    }
    
    public int GetCurrentScore()
    {
        return currentScore;
    }
    
    public int GetHighScore()
    {
        return PlayerPrefs.GetInt("HighScore", 0);
    }
}
