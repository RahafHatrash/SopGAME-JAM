using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathSceneManager : MonoBehaviour
{
    [Header("Death Scene Settings")]
    public string mainMenuSceneName = "MainMenu";
    
    void Start()
    {
        Debug.Log("[DeathSceneManager] Death scene loaded - Player has died!");
    }
    
    void Update()
    {
        // Allow player to press any key to return to main menu immediately
        if (Input.anyKeyDown)
        {
            ReturnToMainMenu();
        }
    }
    
    public void ReturnToMainMenu()
    {
        Debug.Log("[DeathSceneManager] Returning to main menu...");
        SceneManager.LoadScene(mainMenuSceneName);
    }
    
    public void RestartGame()
    {
        Debug.Log("[DeathSceneManager] Restarting game...");
        // You can add logic to restart from a specific scene here
        SceneManager.LoadScene("Hala'sSceneSKY"); // Start from first scene
    }
}
