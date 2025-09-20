using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScoreManager : MonoBehaviour
{
    [Header("Score Settings")]
    public int currentScore = 0;
    public int highScore = 0;
    
    [Header("XP Settings")]
    public int xpPerKill = 5;
    public int bonusXPAmount = 10;
    
    [Header("UI References")]
    public Text scoreText;
    public Text highScoreText;
    public Text xpText;
    
    [Header("High Score Settings")]
    public string highScoreKey = "HighScore";
    
    // Singleton pattern
    public static ScoreManager Instance { get; private set; }
    
    void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadHighScore();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        // Reset current score at start of each game
        currentScore = 0;
        UpdateUI();
        
        Debug.Log("[ScoreManager] Score system initialized!");
    }
    
    public void AddXP(int amount)
    {
        currentScore += amount;
        UpdateUI();
        
        // Check for new high score
        if (currentScore > highScore)
        {
            highScore = currentScore;
            SaveHighScore();
            Debug.Log($"[ScoreManager] New high score: {highScore}!");
        }
        
        Debug.Log($"[ScoreManager] Added {amount} XP! Total: {currentScore}");
    }
    
    public void AddKillXP()
    {
        AddXP(xpPerKill);
    }
    
    public void AddBonusXP()
    {
        AddXP(bonusXPAmount);
    }
    
    public void UpdateUI()
    {
        if (scoreText != null)
            scoreText.text = "Score: " + currentScore.ToString();
            
        if (highScoreText != null)
            highScoreText.text = "High Score: " + highScore.ToString();
            
        if (xpText != null)
            xpText.text = "XP: " + currentScore.ToString();
    }
    
    void SaveHighScore()
    {
        PlayerPrefs.SetInt(highScoreKey, highScore);
        PlayerPrefs.Save();
        Debug.Log($"[ScoreManager] High score saved: {highScore}");
    }
    
    void LoadHighScore()
    {
        highScore = PlayerPrefs.GetInt(highScoreKey, 0);
        Debug.Log($"[ScoreManager] High score loaded: {highScore}");
    }
    
    public void ResetScore()
    {
        currentScore = 0;
        UpdateUI();
        Debug.Log("[ScoreManager] Score reset!");
    }
    
    public void ShowFinalScore()
    {
        Debug.Log($"[ScoreManager] Final Score: {currentScore} | High Score: {highScore}");
    }
}
