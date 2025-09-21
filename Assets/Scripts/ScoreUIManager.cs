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
        
        // Always load current score from PlayerPrefs (it will be 0 only if explicitly reset)
        currentScore = PlayerPrefs.GetInt("CurrentScore", 0);
        Debug.Log($"[ScoreUIManager] Current score loaded: {currentScore}");
        
        UpdateUI();
        
        Debug.Log($"[ScoreUIManager] Score UI initialized! Current: {currentScore}, High: {highScore}");
    }
    
    public void AddScore(int points)
    {
        currentScore += points;
        
        // Save current score
        PlayerPrefs.SetInt("CurrentScore", currentScore);
        
        // Check if current score is higher than high score
        if (currentScore > highScore)
        {
            highScore = currentScore;
            PlayerPrefs.SetInt("HighScore", highScore);
            Debug.Log($"[ScoreUIManager] New high score: {highScore}!");
        }
        
        PlayerPrefs.Save();
        UpdateUI();
        Debug.Log($"[ScoreUIManager] Added {points} points! Total: {currentScore}, High: {highScore}");
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
    
    // Method to clear all scores (for Play button from MainMenu)
    public void ClearAllScores()
    {
        currentScore = 0;
        highScore = 0;
        PlayerPrefs.SetInt("CurrentScore", 0);
        PlayerPrefs.SetInt("HighScore", 0);
        PlayerPrefs.Save();
        UpdateUI();
        Debug.Log("[ScoreUIManager] All scores cleared completely!");
    }
    
    // Method to reset only current score (for Restart)
    public void ResetCurrentScore()
    {
        currentScore = 0;
        PlayerPrefs.SetInt("CurrentScore", 0);
        PlayerPrefs.Save();
        UpdateUI();
        Debug.Log("[ScoreUIManager] Current score reset to 0, high score kept!");
    }
}
