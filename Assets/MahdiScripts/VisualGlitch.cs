using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[System.Serializable]
public class GlitchSettings
{
    [Header("Glitch Timing")]
    [Range(0.1f, 10f)]
    public float glitchFrequency = 2f; // How often glitches occur (per second)
    
    [Range(0.1f, 5f)]
    public float glitchDuration = 0.3f; // How long each glitch lasts
    
    [Range(0f, 1f)]
    public float glitchIntensity = 0.5f; // Overall intensity of glitch effects
    
    [Header("Glitch Effects")]
    [Range(0f, 1f)]
    public float screenShakeIntensity = 0.3f;
    
    [Range(0f, 1f)]
    public float colorDistortionIntensity = 0.4f;
    
    [Range(0f, 1f)]
    public float scanlineIntensity = 0.6f;
    
    [Range(0f, 1f)]
    public float noiseIntensity = 0.3f;
    
    [Range(0f, 1f)]
    public float pixelationIntensity = 0.2f;
    
    [Header("Color Channels")]
    public bool redChannelGlitch = true;
    public bool greenChannelGlitch = true;
    public bool blueChannelGlitch = true;
    
    [Header("Advanced")]
    public bool randomizeIntensity = true;
    public bool enableScreenFreeze = false;
    [Range(0f, 1f)]
    public float freezeChance = 0.1f;
}

public class VisualGlitch : MonoBehaviour
{
    [Header("Glitch Settings")]
    public GlitchSettings settings = new GlitchSettings();
    
    [Header("References")]
    public Camera targetCamera;
    public Volume postProcessVolume;
    
    // Private variables
    private float glitchTimer = 0f;
    private bool isGlitching = false;
    private float glitchStartTime = 0f;
    private Vector3 originalCameraPosition;
    private float originalTimeScale = 1f;
    
    // Post-processing components
    private ChromaticAberration chromaticAberration;
    private FilmGrain filmGrain;
    private Vignette vignette;
    private ColorAdjustments colorAdjustments;
    private LensDistortion lensDistortion;
    
    void Start()
    {
        // Get camera reference
        if (targetCamera == null)
            targetCamera = Camera.main;
        
        // Store original camera position
        originalCameraPosition = targetCamera.transform.position;
        
        // Get post-processing components
        if (postProcessVolume != null)
        {
            postProcessVolume.profile.TryGet(out chromaticAberration);
            postProcessVolume.profile.TryGet(out filmGrain);
            postProcessVolume.profile.TryGet(out vignette);
            postProcessVolume.profile.TryGet(out colorAdjustments);
            postProcessVolume.profile.TryGet(out lensDistortion);
        }
        
        // Start the glitch cycle
        StartGlitchCycle();
    }
    
    void Update()
    {
        if (isGlitching)
        {
            UpdateGlitchEffects();
            
            // Check if glitch should end
            if (Time.time - glitchStartTime >= settings.glitchDuration)
            {
                EndGlitch();
            }
        }
    }
    
    void StartGlitchCycle()
    {
        // Schedule next glitch
        float nextGlitchDelay = 1f / settings.glitchFrequency + Random.Range(-0.2f, 0.2f);
        Invoke(nameof(TriggerGlitch), nextGlitchDelay);
    }
    
    void TriggerGlitch()
    {
        if (!isGlitching)
        {
            StartGlitch();
        }
        
        // Schedule next glitch
        StartGlitchCycle();
    }
    
    void StartGlitch()
    {
        isGlitching = true;
        glitchStartTime = Time.time;
        
        // Apply random intensity if enabled
        float intensity = settings.randomizeIntensity ? 
            Random.Range(settings.glitchIntensity * 0.5f, settings.glitchIntensity * 1.5f) : 
            settings.glitchIntensity;
        
        // Screen freeze effect
        if (settings.enableScreenFreeze && Random.value < settings.freezeChance)
        {
            Time.timeScale = 0f;
            Invoke(nameof(ResumeTime), 0.1f);
        }
        
        // Apply glitch effects
        ApplyScreenShake(intensity);
        ApplyColorDistortion(intensity);
        ApplyScanlines(intensity);
        ApplyNoise(intensity);
        ApplyPixelation(intensity);
    }
    
    void UpdateGlitchEffects()
    {
        float elapsedTime = Time.time - glitchStartTime;
        float progress = elapsedTime / settings.glitchDuration;
        
        // Fade out effects over time
        float fadeOut = 1f - progress;
        
        // Update screen shake
        if (settings.screenShakeIntensity > 0f)
        {
            Vector3 shakeOffset = new Vector3(
                Random.Range(-1f, 1f) * settings.screenShakeIntensity * fadeOut,
                Random.Range(-1f, 1f) * settings.screenShakeIntensity * fadeOut,
                0f
            );
            targetCamera.transform.position = originalCameraPosition + shakeOffset;
        }
    }
    
    void EndGlitch()
    {
        isGlitching = false;
        
        // Reset camera position
        targetCamera.transform.position = originalCameraPosition;
        
        // Reset post-processing effects
        ResetPostProcessingEffects();
    }
    
    void ApplyScreenShake(float intensity)
    {
        if (settings.screenShakeIntensity > 0f)
        {
            // Screen shake is applied in UpdateGlitchEffects
        }
    }
    
    void ApplyColorDistortion(float intensity)
    {
        if (chromaticAberration != null && settings.colorDistortionIntensity > 0f)
        {
            chromaticAberration.intensity.value = settings.colorDistortionIntensity * intensity * 2f;
        }
        
        if (lensDistortion != null)
        {
            lensDistortion.intensity.value = settings.colorDistortionIntensity * intensity * 0.3f;
        }
    }
    
    void ApplyScanlines(float intensity)
    {
        if (settings.scanlineIntensity > 0f)
        {
            // Create scanline effect using film grain
            if (filmGrain != null)
            {
                filmGrain.intensity.value = settings.scanlineIntensity * intensity * 0.5f;
                filmGrain.type.value = FilmGrainLookup.Thin1;
            }
        }
    }
    
    void ApplyNoise(float intensity)
    {
        if (settings.noiseIntensity > 0f)
        {
            if (filmGrain != null)
            {
                filmGrain.intensity.value = Mathf.Max(filmGrain.intensity.value, settings.noiseIntensity * intensity);
            }
        }
    }
    
    void ApplyPixelation(float intensity)
    {
        if (settings.pixelationIntensity > 0f)
        {
            // Pixelation effect can be achieved by adjusting camera pixel ratio
            // or through shader effects (would require custom shader)
        }
    }
    
    void ResetPostProcessingEffects()
    {
        if (chromaticAberration != null)
            chromaticAberration.intensity.value = 0f;
        
        if (filmGrain != null)
            filmGrain.intensity.value = 0f;
        
        if (lensDistortion != null)
            lensDistortion.intensity.value = 0f;
    }
    
    void ResumeTime()
    {
        Time.timeScale = originalTimeScale;
    }
    
    // Public methods for external control
    public void TriggerManualGlitch()
    {
        if (!isGlitching)
        {
            StartGlitch();
        }
    }
    
    public void SetGlitchIntensity(float intensity)
    {
        settings.glitchIntensity = Mathf.Clamp01(intensity);
    }
    
    public void SetGlitchFrequency(float frequency)
    {
        settings.glitchFrequency = Mathf.Clamp(frequency, 0.1f, 10f);
    }
    
    // Debug methods
    [ContextMenu("Test Glitch")]
    public void TestGlitch()
    {
        TriggerManualGlitch();
    }
    
    void OnDestroy()
    {
        // Clean up
        Time.timeScale = originalTimeScale;
        if (targetCamera != null)
            targetCamera.transform.position = originalCameraPosition;
    }
    
    // Visual debug in scene view
    void OnDrawGizmosSelected()
    {
        if (targetCamera != null)
        {
            Gizmos.color = isGlitching ? Color.red : Color.green;
            Gizmos.DrawWireSphere(targetCamera.transform.position, 1f);
        }
    }
}
