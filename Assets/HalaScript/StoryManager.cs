using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class StoryManager : MonoBehaviour
{
    [Header("UI Reference")]
    public TextMeshProUGUI storyText;

    [Header("Story Lines")]
    [TextArea(2, 5)]
    public string[] storyLines;

    [Header("Typewriter Settings")]
    public float typingSpeed = 0.05f; // delay between each letter

    private int currentLine = 0;
    private bool isTyping = false;
    private Coroutine typingCoroutine;

    void Start()
    {
        if (storyLines.Length > 0)
        {
            StartTyping(storyLines[currentLine]);
        }
    }

    void Update()
    {
        if (Input.anyKeyDown)
        {
            if (isTyping)
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
            EndStory();
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

    void EndStory()
    {
        Debug.Log("Story finished! Load next scene here.");
        // Example:
        SceneManager.LoadScene("Hala'sSceneSKY");
    }
}
