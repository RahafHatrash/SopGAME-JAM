using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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
    
    [Header("Debug Info")]
    [SerializeField] private float timeUntilNextTransition;
    [SerializeField] private string currentSceneName;
    
    private Coroutine transitionCoroutine;
    
    void Start()
    {
        // Only run if this is the first SceneTransitionManager found
        if (FindObjectsByType<SceneTransitionManager>(FindObjectsSortMode.None).Length > 1)
        {
            Debug.Log("SceneTransitionManager: Multiple managers detected, destroying duplicate.");
            Destroy(gameObject);
            return;
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
            
            // Wait for the calculated time
            yield return new WaitForSeconds(transitionTime);
            
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
        
        // Load the next scene
        SceneManager.LoadScene(nextScene);
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
    }
}
