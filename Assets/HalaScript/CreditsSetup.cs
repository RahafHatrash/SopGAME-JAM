using UnityEngine;
using UnityEngine.UI;

public class CreditsSetup : MonoBehaviour
{
    [ContextMenu("Create Credits UI")]
    public void CreateCreditsUI()
    {
        // Find or create Canvas
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }
        
        // Create Credits Panel
        GameObject creditsPanel = new GameObject("CreditsPanel");
        creditsPanel.transform.SetParent(canvas.transform, false);
        
        // Add RectTransform and set to full screen
        RectTransform panelRect = creditsPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        
        // Add background image (black)
        Image panelImage = creditsPanel.AddComponent<Image>();
        panelImage.color = Color.black;
        
        // Create Credits Container (this will hold all the credit text)
        GameObject creditsContainer = new GameObject("CreditsContainer");
        creditsContainer.transform.SetParent(creditsPanel.transform, false);
        
        RectTransform containerRect = creditsContainer.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.5f, 0);
        containerRect.anchorMax = new Vector2(0.5f, 1);
        containerRect.anchoredPosition = new Vector2(0, -Screen.height / 2);
        containerRect.sizeDelta = new Vector2(800, Screen.height * 2);
        
        // Add ContentSizeFitter to automatically size the container
        ContentSizeFitter sizeFitter = creditsContainer.AddComponent<ContentSizeFitter>();
        sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        
        // Add VerticalLayoutGroup for automatic spacing
        VerticalLayoutGroup layoutGroup = creditsContainer.AddComponent<VerticalLayoutGroup>();
        layoutGroup.spacing = 30f;
        layoutGroup.childAlignment = TextAnchor.MiddleCenter;
        layoutGroup.childControlHeight = false;
        layoutGroup.childControlWidth = false;
        layoutGroup.childForceExpandHeight = false;
        layoutGroup.childForceExpandWidth = false;
        
        // Add ScrollingCredits script to the panel
        ScrollingCredits scrollingCredits = creditsPanel.AddComponent<ScrollingCredits>();
        scrollingCredits.creditsContainer = creditsContainer.transform;
        
        // Set up default credits
        scrollingCredits.credits = new CreditEntry[]
        {
            new CreditEntry { name = "FORBIDDEN ENTROPY", role = "", isHeader = true },
            new CreditEntry { name = "--------------------------------", role = "", isEmptyLine = true }, // Empty line
            new CreditEntry { name = "", role = "", isEmptyLine = true }, // Empty line
            new CreditEntry { name = "Hala Filimban", role = "Lead Developer" },
            new CreditEntry { name = "Noor Bin Abbas", role = "Game Designer" },
            new CreditEntry { name = "Rahaf Hatrash", role = "Artist" },
            new CreditEntry { name = "", role = "", isEmptyLine = true }, // Empty line
            new CreditEntry { name = "THANKS", role = "", isHeader = true },
        };
        
        Debug.Log("Credits UI created successfully! Check the CreditsPanel in your Canvas.");
        Debug.Log("You can now customize the credits array in the ScrollingCredits component.");
    }
}
