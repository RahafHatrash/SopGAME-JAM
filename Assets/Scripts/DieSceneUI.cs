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
        // Reset current score for new game (but keep high score)
        PlayerPrefs.SetInt("CurrentScore", 0);
        PlayerPrefs.Save();
        SceneManager.LoadScene("Hala'sSceneSKY");
    }
    
    public void QuitGame()
    {
        Debug.Log("[DieSceneUI] Quitting game...");
        // Quit the game
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
