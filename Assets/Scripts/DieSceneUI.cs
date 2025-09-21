using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class DieSceneUI : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI finalScoreText;
    public TextMeshProUGUI highScoreText;
    
    void Start()
    {
        Debug.Log("[DieSceneUI] Starting DieSceneUI...");
        
        // Play death sound when death scene loads
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayDeathSound();
            Debug.Log("[DieSceneUI] Death sound played");
        }
        else
        {
            Debug.LogWarning("[DieSceneUI] AudioManager.Instance is null - cannot play death sound");
        }
        
        // Get current score
        int finalScore = PlayerPrefs.GetInt("CurrentScore", 0);
        Debug.Log($"[DieSceneUI] Current Score from PlayerPrefs: {finalScore}");
        
        // Get high score
        int highScore = PlayerPrefs.GetInt("HighScore", 0);
        Debug.Log($"[DieSceneUI] High Score from PlayerPrefs: {highScore}");
        
        // Check if current score is higher than high score
        if (finalScore > highScore)
        {
            highScore = finalScore;
            PlayerPrefs.SetInt("HighScore", highScore);
            PlayerPrefs.Save();
            Debug.Log($"[DieSceneUI] New high score: {highScore}!");
        }

        
        if (finalScoreText != null)
        {
            finalScoreText.text = "Score: " + finalScore.ToString();
            Debug.Log($"[DieSceneUI] Score text set to: {finalScoreText.text}");
        }
        else
        {
            Debug.LogWarning("[DieSceneUI] finalScoreText is null!");
        }
            
        if (highScoreText != null)
        {
            highScoreText.text = "High Score: " + highScore.ToString();
            Debug.Log($"[DieSceneUI] High Score text set to: {highScoreText.text}");
        }
        else
        {
            Debug.LogWarning("[DieSceneUI] highScoreText is null!");
        }
            
        Debug.Log($"[DieSceneUI] Final Score: {finalScore}, High Score: {highScore}");
        Debug.Log("[DieSceneUI] Use RestartGame() and QuitGame() functions on buttons directly in Inspector");
    }
    
    public void RestartGame()
    {
        Debug.Log("[DieSceneUI] Restarting game...");
        // Reset current score and health for restart
        PlayerPrefs.SetInt("CurrentScore", 0);
        PlayerPrefs.SetInt("PlayerHealth", 10); // Reset health to max
        PlayerPrefs.Save();
        Debug.Log("[DieSceneUI] Current score and health reset for restart");
        SceneManager.LoadScene("Hala'sSceneSKY");
    }
    
    public void QuitGame()
    {
        Debug.Log("[DieSceneUI] Quitting game and clearing ALL scores...");
        // Clear ALL scores (current and high) - EVERYTHING gets reset
        PlayerPrefs.SetInt("CurrentScore", 0);
        PlayerPrefs.SetInt("HighScore", 0);
        PlayerPrefs.SetInt("PlayerHealth", 10); // Reset health to max
        PlayerPrefs.Save();
        Debug.Log("[DieSceneUI] ALL scores and health cleared - EVERYTHING reset!");
        
        // Quit the game
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
