using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public void PlayGame()
    {
        // Replace "GameScene" with the actual name of your gameplay scene
        SceneManager.LoadScene("Tutorial");
    }

    public void ShowCredits()
    {
        // Replace "CreditsScene" with the name of your credits scene
        SceneManager.LoadScene("CreditScene");
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Game Quit!"); // Only visible in the Editor
    }
}
