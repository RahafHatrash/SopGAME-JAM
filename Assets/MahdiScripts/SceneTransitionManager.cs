using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneTransitionManager : MonoBehaviour
{
    [Header("Scene Transition Settings")]
    [Tooltip("List of scenes to randomly transition between")]
    public List<string> sceneNames = new List<string>();
    
    [Tooltip("Minimum time in seconds before transitioning to next scene")]
    [Range(1f, 60f)]
    public float minTransitionTime = 10f;
    
    [Tooltip("Maximum time in seconds before transitioning to next scene")]
    [Range(1f, 60f)]
    public float maxTransitionTime = 30f;
    
    [Header("Transition Effects")]
    [Tooltip("How many seconds before transition to start screen shake")]
    [Range(0.5f, 10f)]
    public float shakeStartTime = 3f;
    
    [Tooltip("Intensity of screen shake effect")]
    [Range(0.1f, 2f)]
    public float shakeIntensity = 0.5f;
    
    [Tooltip("Duration of flash effect")]
    [Range(0.1f, 2f)]
    public float flashDuration = 0.3f;
    
    [Tooltip("Color of the flash effect")]
    public Color flashColor = Color.white;
    
    [Tooltip("Maximum transparency/opacity of the flash effect (0 = invisible, 1 = fully opaque)")]
    [Range(0f, 1f)]
    public float flashMaxAlpha = 1f;
    
    [Header("References")]
    [Tooltip("Main camera for screen shake")]
    public Camera mainCamera;
    
    [Tooltip("UI Image for flash effect (optional - will create if not assigned)")]
    public Image flashImage;
    
    [Header("Debug Info")]
    [SerializeField] private float timeUntilNextTransition;
    [SerializeField] private string currentSceneName;
    [SerializeField] private bool isShaking = false;
    [SerializeField] private bool isFlashing = false;
    
    private Coroutine transitionCoroutine;
    private Vector3 originalCameraPosition;
    
    // Static reference to flash image for cross-scene persistence
    private static Image staticFlashImage;
    
    void Start()
    {
        // Only run if this is the first SceneTransitionManager found
        if (FindObjectsByType<SceneTransitionManager>(FindObjectsSortMode.None).Length > 1)
        {
            Debug.Log("SceneTransitionManager: Multiple managers detected, destroying duplicate.");
            Destroy(gameObject);
            return;
        }
        
        // Initialize camera reference
        if (mainCamera == null)
            mainCamera = Camera.main;
        
        if (mainCamera != null)
            originalCameraPosition = mainCamera.transform.position;
        
        // Setup flash image if not assigned
        SetupFlashImage();
        
        // Check if we need to fade out flash from previous scene
        if (staticFlashImage != null && staticFlashImage.color.a > 0f)
        {
            StartCoroutine(FadeOutFlash());
        }
        else
        {
            // Clear any existing flash from previous scene
            ClearFlashEffect();
        }
        
        // Validate settings
        if (sceneNames.Count == 0)
        {
            Debug.LogWarning("SceneTransitionManager: No scenes added to the list! Please add scene names in the inspector.");
            return;
        }
        
        if (minTransitionTime > maxTransitionTime)
        {
            Debug.LogWarning("SceneTransitionManager: Min transition time is greater than max transition time! Swapping values.");
            float temp = minTransitionTime;
            minTransitionTime = maxTransitionTime;
            maxTransitionTime = temp;
        }
        
        // Start the transition coroutine
        StartTransitionTimer();
    }
    
    void StartTransitionTimer()
    {
        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
        }
        
        transitionCoroutine = StartCoroutine(TransitionTimer());
    }
    
    IEnumerator TransitionTimer()
    {
        while (true)
        {
            // Calculate random transition time
            float transitionTime = Random.Range(minTransitionTime, maxTransitionTime);
            timeUntilNextTransition = transitionTime;
            
            Debug.Log($"SceneTransitionManager: Next transition in {transitionTime:F1} seconds");
            
            // Wait for most of the transition time, minus the shake start time
            float waitTime = Mathf.Max(0f, transitionTime - shakeStartTime);
            if (waitTime > 0f)
            {
                yield return new WaitForSeconds(waitTime);
            }
            
            // Start screen shake if we have time left
            if (shakeStartTime > 0f && mainCamera != null)
            {
                StartCoroutine(ScreenShakeEffect(shakeStartTime));
                yield return new WaitForSeconds(shakeStartTime);
            }
            
            // Transition to a random scene
            TransitionToRandomScene();
        }
    }
    
    void TransitionToRandomScene()
    {
        if (sceneNames.Count == 0)
        {
            Debug.LogError("SceneTransitionManager: No scenes available for transition!");
            return;
        }
        
        // Get current scene name for reference
        currentSceneName = SceneManager.GetActiveScene().name;
        
        // Select a random scene (excluding current scene if there are multiple scenes)
        string nextScene;
        if (sceneNames.Count > 1)
        {
            List<string> availableScenes = new List<string>(sceneNames);
            availableScenes.Remove(currentSceneName);
            nextScene = availableScenes[Random.Range(0, availableScenes.Count)];
        }
        else
        {
            nextScene = sceneNames[0];
        }
        
        Debug.Log($"SceneTransitionManager: Transitioning from '{currentSceneName}' to '{nextScene}'");
        
        // Start flash effect before transitioning
        StartCoroutine(FlashAndTransition(nextScene));
    }
    
    // Setup flash image for screen flash effect
    void SetupFlashImage()
    {
        // Use static reference if available, otherwise create new one
        if (staticFlashImage != null)
        {
            flashImage = staticFlashImage;
        }
        else if (flashImage == null)
        {
            // Create a canvas and flash image if not assigned
            GameObject canvasGO = new GameObject("TransitionFlashCanvas");
            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000; // High sorting order to appear on top
            
            CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            canvasGO.AddComponent<GraphicRaycaster>();
            
            GameObject flashGO = new GameObject("FlashImage");
            flashGO.transform.SetParent(canvasGO.transform, false);
            
            flashImage = flashGO.AddComponent<Image>();
            flashImage.color = new Color(flashColor.r, flashColor.g, flashColor.b, 0f); // Start transparent
            
            RectTransform rectTransform = flashImage.rectTransform;
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.anchoredPosition = Vector2.zero;
            
            DontDestroyOnLoad(canvasGO);
            staticFlashImage = flashImage; // Set static reference
        }
    }
    
    // Screen shake effect coroutine
    IEnumerator ScreenShakeEffect(float duration)
    {
        isShaking = true;
        float elapsedTime = 0f;
        
        while (elapsedTime < duration)
        {
            if (mainCamera != null)
            {
                // Generate random shake offset
                Vector3 shakeOffset = new Vector3(
                    Random.Range(-1f, 1f) * shakeIntensity,
                    Random.Range(-1f, 1f) * shakeIntensity,
                    0f
                );
                
                // Apply shake to camera
                mainCamera.transform.position = originalCameraPosition + shakeOffset;
            }
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // Reset camera position
        if (mainCamera != null)
            mainCamera.transform.position = originalCameraPosition;
        
        isShaking = false;
    }
    
    // Flash and transition coroutine
    IEnumerator FlashAndTransition(string sceneName)
    {
        if (flashImage != null)
        {
            isFlashing = true;
            
            // Flash in (fade to flash color)
            float flashInTime = flashDuration * 0.5f;
            float elapsedTime = 0f;
            Color startColor = flashImage.color;
            Color targetColor = new Color(flashColor.r, flashColor.g, flashColor.b, flashMaxAlpha);
            
            while (elapsedTime < flashInTime)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / flashInTime;
                flashImage.color = Color.Lerp(startColor, targetColor, progress);
                yield return null;
            }
            
            // Ensure flash is fully opaque
            flashImage.color = targetColor;
        }
        
        // Load the scene (flash will be handled in the new scene)
        SceneManager.LoadScene(sceneName);
    }
    
    // Clear flash effect (make it transparent)
    void ClearFlashEffect()
    {
        if (staticFlashImage != null)
        {
            staticFlashImage.color = new Color(flashColor.r, flashColor.g, flashColor.b, 0f);
            isFlashing = false;
        }
    }
    
    // Fade out flash effect after scene transition
    IEnumerator FadeOutFlash()
    {
        if (staticFlashImage != null)
        {
            float flashOutTime = flashDuration * 0.5f;
            float elapsedTime = 0f;
            Color startColor = staticFlashImage.color;
            Color targetColor = new Color(flashColor.r, flashColor.g, flashColor.b, 0f);
            
            while (elapsedTime < flashOutTime)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / flashOutTime;
                staticFlashImage.color = Color.Lerp(startColor, targetColor, progress);
                yield return null;
            }
            
            // Ensure flash is fully transparent
            staticFlashImage.color = targetColor;
            isFlashing = false;
        }
    }
    
    // Public method to manually trigger transition (useful for testing)
    [ContextMenu("Force Transition")]
    public void ForceTransition()
    {
        TransitionToRandomScene();
    }
    
    // Public method to add scenes at runtime
    public void AddScene(string sceneName)
    {
        if (!sceneNames.Contains(sceneName))
        {
            sceneNames.Add(sceneName);
            Debug.Log($"SceneTransitionManager: Added scene '{sceneName}' to transition list");
        }
    }
    
    // Public method to remove scenes at runtime
    public void RemoveScene(string sceneName)
    {
        if (sceneNames.Contains(sceneName))
        {
            sceneNames.Remove(sceneName);
            Debug.Log($"SceneTransitionManager: Removed scene '{sceneName}' from transition list");
        }
    }
    
    void OnValidate()
    {
        // Ensure min is not greater than max
        if (minTransitionTime > maxTransitionTime)
        {
            maxTransitionTime = minTransitionTime;
        }
        
        // Ensure shake start time is not greater than minimum transition time
        if (shakeStartTime > minTransitionTime)
        {
            shakeStartTime = minTransitionTime * 0.5f;
        }
    }
    
    void OnDestroy()
    {
        // Clean up camera position if still shaking
        if (isShaking && mainCamera != null)
        {
            mainCamera.transform.position = originalCameraPosition;
        }
    }
}
