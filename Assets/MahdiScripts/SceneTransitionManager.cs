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

    [Header("Flash Excluded Scenes")]
    [Tooltip("List of scene names where flash effect should NOT appear")]
    public List<string> flashExcludedScenes = new List<string>();

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
        if (FindObjectsByType<SceneTransitionManager>(FindObjectsSortMode.None).Length > 1)
        {
            Debug.Log("SceneTransitionManager: Multiple managers detected, destroying duplicate.");
            Destroy(gameObject);
            return;
        }

        if (mainCamera == null)
            mainCamera = Camera.main;

        if (mainCamera != null)
            originalCameraPosition = mainCamera.transform.position;

        SetupFlashImage();

        if (staticFlashImage != null && staticFlashImage.color.a > 0f)
        {
            StartCoroutine(FadeOutFlash());
        }
        else
        {
            ClearFlashEffect();
        }

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

        StartTransitionTimer();
    }

    void StartTransitionTimer()
    {
        if (transitionCoroutine != null)
            StopCoroutine(transitionCoroutine);

        transitionCoroutine = StartCoroutine(TransitionTimer());
    }

    IEnumerator TransitionTimer()
    {
        while (true)
        {
            float transitionTime = Random.Range(minTransitionTime, maxTransitionTime);
            timeUntilNextTransition = transitionTime;

            float waitTime = Mathf.Max(0f, transitionTime - shakeStartTime);
            if (waitTime > 0f)
                yield return new WaitForSeconds(waitTime);

            if (shakeStartTime > 0f && mainCamera != null)
            {
                StartCoroutine(ScreenShakeEffect(shakeStartTime));
                yield return new WaitForSeconds(shakeStartTime);
            }

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

        currentSceneName = SceneManager.GetActiveScene().name;

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

        StartCoroutine(FlashAndTransition(nextScene));
    }

    void SetupFlashImage()
    {
        if (staticFlashImage != null)
        {
            flashImage = staticFlashImage;
        }
        else if (flashImage == null)
        {
            GameObject canvasGO = new GameObject("TransitionFlashCanvas");
            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000;

            CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            canvasGO.AddComponent<GraphicRaycaster>();

            GameObject flashGO = new GameObject("FlashImage");
            flashGO.transform.SetParent(canvasGO.transform, false);

            flashImage = flashGO.AddComponent<Image>();
            flashImage.color = new Color(flashColor.r, flashColor.g, flashColor.b, 0f);

            RectTransform rectTransform = flashImage.rectTransform;
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.anchoredPosition = Vector2.zero;

            DontDestroyOnLoad(canvasGO);
            staticFlashImage = flashImage;

            // Hide flash in excluded scenes automatically
            SceneManager.sceneLoaded += (scene, mode) =>
            {
                if (flashExcludedScenes.Contains(scene.name))
                    canvasGO.SetActive(false);
                else
                {
                    canvasGO.SetActive(true);
                    ClearFlashEffect();
                }
            };
        }
    }

    IEnumerator ScreenShakeEffect(float duration)
    {
        isShaking = true;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            if (mainCamera != null)
            {
                Vector3 shakeOffset = new Vector3(
                    Random.Range(-1f, 1f) * shakeIntensity,
                    Random.Range(-1f, 1f) * shakeIntensity,
                    0f
                );
                mainCamera.transform.position = originalCameraPosition + shakeOffset;
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if (mainCamera != null)
            mainCamera.transform.position = originalCameraPosition;

        isShaking = false;
    }

    IEnumerator FlashAndTransition(string sceneName)
    {
        if (flashExcludedScenes.Contains(sceneName))
        {
            ClearFlashEffect();
            SceneManager.LoadScene(sceneName);
            yield break;
        }

        if (flashImage != null)
        {
            isFlashing = true;
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

            flashImage.color = targetColor;
        }

        SceneManager.LoadScene(sceneName);
    }

    void ClearFlashEffect()
    {
        if (staticFlashImage != null)
        {
            staticFlashImage.color = new Color(flashColor.r, flashColor.g, flashColor.b, 0f);
            isFlashing = false;
        }
    }

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

            staticFlashImage.color = targetColor;
            isFlashing = false;
        }
    }

    [ContextMenu("Force Transition")]
    public void ForceTransition() => TransitionToRandomScene();

    public void AddScene(string sceneName)
    {
        if (!sceneNames.Contains(sceneName))
            sceneNames.Add(sceneName);
    }

    public void RemoveScene(string sceneName)
    {
        if (sceneNames.Contains(sceneName))
            sceneNames.Remove(sceneName);
    }

    void OnValidate()
    {
        if (minTransitionTime > maxTransitionTime)
            maxTransitionTime = minTransitionTime;

        if (shakeStartTime > minTransitionTime)
            shakeStartTime = minTransitionTime * 0.5f;
    }

    void OnDestroy()
    {
        if (isShaking && mainCamera != null)
            mainCamera.transform.position = originalCameraPosition;
    }
}
