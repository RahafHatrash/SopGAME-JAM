using UnityEngine;
using UnityEngine.UI;

public class HealthUISetup : MonoBehaviour
{
    [ContextMenu("Create Health UI")]
    public void CreateHealthUI()
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
        
        // Create Health UI Panel
        GameObject healthPanel = new GameObject("HealthPanel");
        healthPanel.transform.SetParent(canvas.transform, false);
        
        // Add RectTransform and set position
        RectTransform panelRect = healthPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0, 1);
        panelRect.anchorMax = new Vector2(0, 1);
        panelRect.anchoredPosition = new Vector2(100, -50);
        panelRect.sizeDelta = new Vector2(200, 100);
        
        // Add background image
        Image panelImage = healthPanel.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.5f); // Semi-transparent black
        
        // Create Health Text
        GameObject healthTextObj = new GameObject("HealthText");
        healthTextObj.transform.SetParent(healthPanel.transform, false);
        
        Text healthText = healthTextObj.AddComponent<Text>();
        healthText.text = "Health: 10/10";
        healthText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        healthText.fontSize = 18;
        healthText.color = Color.white;
        healthText.alignment = TextAnchor.MiddleCenter;
        
        RectTransform textRect = healthTextObj.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0, 0.5f);
        textRect.anchorMax = new Vector2(1, 1);
        textRect.offsetMin = new Vector2(10, 0);
        textRect.offsetMax = new Vector2(-10, -10);
        
        // Create Health Bar
        GameObject healthBarObj = new GameObject("HealthBar");
        healthBarObj.transform.SetParent(healthPanel.transform, false);
        
        Slider healthBar = healthBarObj.AddComponent<Slider>();
        healthBar.minValue = 0;
        healthBar.maxValue = 10;
        healthBar.value = 10;
        
        RectTransform barRect = healthBarObj.GetComponent<RectTransform>();
        barRect.anchorMin = new Vector2(0, 0);
        barRect.anchorMax = new Vector2(1, 0.5f);
        barRect.offsetMin = new Vector2(10, 10);
        barRect.offsetMax = new Vector2(-10, 0);
        
        // Create Health Bar Background
        GameObject backgroundObj = new GameObject("Background");
        backgroundObj.transform.SetParent(healthBarObj.transform, false);
        
        Image backgroundImage = backgroundObj.AddComponent<Image>();
        backgroundImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);
        
        RectTransform bgRect = backgroundObj.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        
        // Create Health Bar Fill
        GameObject fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(healthBarObj.transform, false);
        
        Image fillImage = fillObj.AddComponent<Image>();
        fillImage.color = Color.green;
        
        RectTransform fillRect = fillObj.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
        
        // Set up slider components
        healthBar.targetGraphic = fillImage;
        healthBar.fillRect = fillRect;
        
        // Add HealthUI script to the panel
        HealthUI healthUI = healthPanel.AddComponent<HealthUI>();
        healthUI.healthText = healthText;
        healthUI.healthBar = healthBar;
        healthUI.healthBarFill = fillImage;
        
        Debug.Log("Health UI created successfully! Check the HealthPanel in your Canvas.");
    }
}
