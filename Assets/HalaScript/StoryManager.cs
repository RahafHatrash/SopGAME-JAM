using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class StoryManager : MonoBehaviour
{
    [Header("UI Reference")]
    public TextMeshProUGUI storyText;
    public UnityEngine.UI.Image tutorialImage;

    [Header("Story Lines")]
    [TextArea(2, 5)]
    public string[] storyLines;

    [Header("Tutorial Photos")]
    public Sprite[] tutorialPhotos;
    public float photoDisplayTime = 3f; // How long each photo shows

    [Header("Typewriter Settings")]
    public float typingSpeed = 0.05f; // delay between each letter

    private int currentLine = 0;
    private int currentPhoto = 0;
    private bool isTyping = false;
    private bool isShowingPhotos = false;
    private Coroutine typingCoroutine;

    void Start()
    {
        // Hide tutorial image initially
        if (tutorialImage != null)
        {
            tutorialImage.gameObject.SetActive(false);
        }
        
        if (storyLines.Length > 0)
        {
            StartTyping(storyLines[currentLine]);
        }
    }

    void Update()
    {
        if (Input.anyKeyDown)
        {
            if (isShowingPhotos)
            {
                // Skip to next photo or end photos
                ShowNextPhoto();
            }
            else if (isTyping)
            {
                // If still typing, finish instantly
                FinishLineInstantly();
            }
            else
            {
                ShowNextLine();
            }
        }
    }

    void ShowNextLine()
    {
        currentLine++;

        if (currentLine < storyLines.Length)
        {
            StartTyping(storyLines[currentLine]);
        }
        else
        {
            StartTutorialPhotos();
        }
    }

    void StartTyping(string line)
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        typingCoroutine = StartCoroutine(TypeLine(line));
    }

    IEnumerator TypeLine(string line)
    {
        isTyping = true;
        storyText.text = "";

        foreach (char c in line.ToCharArray())
        {
            storyText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }

    void FinishLineInstantly()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        // instantly show full line
        storyText.text = storyLines[currentLine];
        isTyping = false;
    }

    void StartTutorialPhotos()
    {
        if (tutorialPhotos.Length > 0)
        {
            // Hide story text and show tutorial image
            if (storyText != null)
            {
                storyText.gameObject.SetActive(false);
            }
            
            if (tutorialImage != null)
            {
                tutorialImage.gameObject.SetActive(true);
            }
            
            isShowingPhotos = true;
            currentPhoto = 0;
            ShowCurrentPhoto();
        }
        else
        {
            // No photos, go directly to next scene
            EndStory();
        }
    }
    
    void ShowCurrentPhoto()
    {
        if (tutorialImage != null && currentPhoto < tutorialPhotos.Length)
        {
            tutorialImage.sprite = tutorialPhotos[currentPhoto];
            Debug.Log($"Showing tutorial photo {currentPhoto + 1}/{tutorialPhotos.Length}");
        }
    }
    
    void ShowNextPhoto()
    {
        currentPhoto++;
        
        if (currentPhoto < tutorialPhotos.Length)
        {
            ShowCurrentPhoto();
        }
        else
        {
            EndStory();
        }
    }
    
    void EndStory()
    {
        Debug.Log("Story and tutorials finished! Load next scene here.");
        // Example:
        SceneManager.LoadScene("Hala'sSceneSKY");
    }
}
