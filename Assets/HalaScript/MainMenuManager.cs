using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public void PlayGame()
    {
        // Reset ALL scores and health for new game
        PlayerPrefs.SetInt("CurrentScore", 0);
        PlayerPrefs.SetInt("HighScore", 0);
        PlayerPrefs.SetInt("PlayerHealth", 10); // Full health
        PlayerPrefs.Save();
        Debug.Log("[MainMenuManager] New game started - ALL scores and health reset");
        
        // Replace "GameScene" with the actual name of your gameplay scene
        SceneManager.LoadScene("Tutorial");
    }

    public void ShowCredits()
    {
        // Replace "CreditsScene" with the name of your credits scene
        SceneManager.LoadScene("CreditScene");
    }

//     public void QuitGame()
//     {
//         Application.Quit();
//         Debug.Log("Game Quit!"); // Only visible in the Editor
//     }

public void QuitGame()
    {
        #if UNITY_EDITOR
        EditorApplication.isPlaying = false; // Stop play mode in Editor
        #else
        Application.Quit(); // Quit standalone build
        #endif

        Debug.Log("Game Quit!");
    }
 }
