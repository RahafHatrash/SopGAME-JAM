using UnityEngine;
using UnityEngine.UI;

public class ButtonSound : MonoBehaviour
{
    public AudioClip clickSound;
    public string sceneToLoad;

    private Button myButton;

    void Start()
    {
        myButton = GetComponent<Button>();
        if (myButton != null)
        {
            myButton.onClick.AddListener(OnButtonClicked);
        }
    }

    void OnButtonClicked()
    {
        if (ButtonSoundManager.Instance != null && clickSound != null)
        {
            ButtonSoundManager.Instance.PlaySound(clickSound);
        }
        
        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneToLoad);
        }
    }
}
