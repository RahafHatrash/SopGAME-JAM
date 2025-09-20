using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class CreditEntry
{
    public string name;
    public string role;
    public bool isHeader = false; // For section headers like "DEVELOPMENT TEAM"
    public bool isEmptyLine = false; // For adding empty lines/spacing
}

public class ScrollingCredits : MonoBehaviour
{
    [Header("Credits Content")]
    public CreditEntry[] credits;
    
    [Header("UI References")]
    public Transform creditsContainer;
    public GameObject creditTextPrefab;
    public GameObject headerTextPrefab;
    
    [Header("Scrolling Settings")]
    public float scrollSpeed = 50f;
    public float spacingBetweenEntries = 30f;
    public float startDelay = 2f;
    public float endDelay = 3f;
    
    [Header("Text Settings")]
    public Color nameColor = Color.white;
    public Color roleColor = Color.yellow;
    public Color headerColor = Color.cyan;
    public int nameFontSize = 24;
    public int roleFontSize = 18;
    public int headerFontSize = 32;
    
    [Header("Font Settings")]
    public TMP_FontAsset nameFont;
    public TMP_FontAsset roleFont;
    public TMP_FontAsset headerFont;
    
    [Header("Text Style Settings")]
    public FontStyles nameFontStyle = FontStyles.Bold;
    public FontStyles roleFontStyle = FontStyles.Normal;
    public FontStyles headerFontStyle = FontStyles.Bold;
    
    [Header("Scene Management")]
    public string nextSceneName = "MainMenu";
    public bool autoLoadNextScene = true;
    
    private List<GameObject> creditObjects = new List<GameObject>();
    private bool isScrolling = false;
    private float totalHeight = 0f;
    
    void Start()
    {
        CreateCredits();
        StartCoroutine(StartScrolling());
    }
    
    void CreateCredits()
    {
        if (creditsContainer == null)
        {
            Debug.LogError("Credits container not assigned!");
            return;
        }
        
        // Clear existing credits
        foreach (Transform child in creditsContainer)
        {
            Destroy(child.gameObject);
        }
        creditObjects.Clear();
        
        float currentY = 0f;
        
        foreach (CreditEntry credit in credits)
        {
            if (credit.isEmptyLine)
            {
                // Just add spacing for empty line
                currentY -= spacingBetweenEntries;
            }
            else if (credit.isHeader)
            {
                // Create header
                GameObject headerObj = CreateHeaderText(credit.name);
                headerObj.transform.SetParent(creditsContainer, false);
                headerObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, currentY);
                creditObjects.Add(headerObj);
                currentY -= spacingBetweenEntries * 2; // Extra space for headers
            }
            else
            {
                // Create name
                GameObject nameObj = CreateNameText(credit.name);
                nameObj.transform.SetParent(creditsContainer, false);
                nameObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, currentY);
                creditObjects.Add(nameObj);
                currentY -= spacingBetweenEntries;
                
                // Create role
                GameObject roleObj = CreateRoleText(credit.role);
                roleObj.transform.SetParent(creditsContainer, false);
                roleObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, currentY);
                creditObjects.Add(roleObj);
                currentY -= spacingBetweenEntries;
            }
        }
        
        totalHeight = Mathf.Abs(currentY) + Screen.height;
    }
    
    GameObject CreateHeaderText(string text)
    {
        GameObject textObj = new GameObject("HeaderText");
        TextMeshProUGUI textComponent = textObj.AddComponent<TextMeshProUGUI>();
        
        textComponent.text = text;
        textComponent.fontSize = headerFontSize;
        textComponent.color = headerColor;
        textComponent.alignment = TextAlignmentOptions.Center;
        textComponent.fontStyle = headerFontStyle;
        
        // Use custom font if assigned, otherwise use default
        if (headerFont != null)
        {
            textComponent.font = headerFont;
        }
        else
        {
            textComponent.font = Resources.GetBuiltinResource<TMP_FontAsset>("LegacyRuntime SDF");
        }
        
        RectTransform rectTransform = textObj.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(800, 50);
        
        return textObj;
    }
    
    GameObject CreateNameText(string text)
    {
        GameObject textObj = new GameObject("NameText");
        TextMeshProUGUI textComponent = textObj.AddComponent<TextMeshProUGUI>();
        
        textComponent.text = text;
        textComponent.fontSize = nameFontSize;
        textComponent.color = nameColor;
        textComponent.alignment = TextAlignmentOptions.Center;
        textComponent.fontStyle = nameFontStyle;
        
        // Use custom font if assigned, otherwise use default
        if (nameFont != null)
        {
            textComponent.font = nameFont;
        }
        else
        {
            textComponent.font = Resources.GetBuiltinResource<TMP_FontAsset>("LegacyRuntime SDF");
        }
        
        RectTransform rectTransform = textObj.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(600, 30);
        
        return textObj;
    }
    
    GameObject CreateRoleText(string text)
    {
        GameObject textObj = new GameObject("RoleText");
        TextMeshProUGUI textComponent = textObj.AddComponent<TextMeshProUGUI>();
        
        textComponent.text = text;
        textComponent.fontSize = roleFontSize;
        textComponent.color = roleColor;
        textComponent.alignment = TextAlignmentOptions.Center;
        textComponent.fontStyle = roleFontStyle;
        
        // Use custom font if assigned, otherwise use default
        if (roleFont != null)
        {
            textComponent.font = roleFont;
        }
        else
        {
            textComponent.font = Resources.GetBuiltinResource<TMP_FontAsset>("LegacyRuntime SDF");
        }
        
        RectTransform rectTransform = textObj.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(600, 25);
        
        return textObj;
    }
    
    IEnumerator StartScrolling()
    {
        // Wait for start delay
        yield return new WaitForSeconds(startDelay);
        
        isScrolling = true;
        float startTime = Time.time;
        
        while (isScrolling)
        {
            float elapsedTime = Time.time - startTime;
            float newY = elapsedTime * scrollSpeed;
            
            creditsContainer.localPosition = new Vector3(0, newY, 0);
            
            // Check if we've scrolled past all credits
            if (newY > totalHeight)
            {
                isScrolling = false;
                break;
            }
            
            yield return null;
        }
        
        // Wait for end delay
        yield return new WaitForSeconds(endDelay);
        
        // Load next scene
        if (autoLoadNextScene && !string.IsNullOrEmpty(nextSceneName))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneName);
        }
    }
    
    void Update()
    {
        // Allow user to skip credits
        if (Input.anyKeyDown && isScrolling)
        {
            StopAllCoroutines();
            if (autoLoadNextScene && !string.IsNullOrEmpty(nextSceneName))
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneName);
            }
        }
    }
    
    // Method to manually start credits (useful for testing)
    [ContextMenu("Test Credits")]
    public void TestCredits()
    {
        CreateCredits();
        StartCoroutine(StartScrolling());
    }
    
    // Method to refresh credits (useful when credits are updated)
    [ContextMenu("Refresh Credits")]
    public void RefreshCredits()
    {
        CreateCredits();
    }
}
