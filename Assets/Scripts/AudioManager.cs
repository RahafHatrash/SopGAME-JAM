using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;      // For background music
    [SerializeField] private AudioSource sfxSource;        // For sound effects
    [SerializeField] private AudioSource uiSource;         // For UI sounds
    [SerializeField] private AudioSource playerSource;     // For player-specific sounds

    [Header("Background Music - Scene Based")]
    [SerializeField] private AudioClip[] backgroundMusic;
    [SerializeField] private string[] sceneNames;          // Scene names corresponding to background music
    [SerializeField] private AudioClip defaultBackgroundMusic; // Default music for scenes without specific music
    [SerializeField] private AudioClip mainMenuMusic;      // Special music for main menu
    [SerializeField] private AudioClip tutorialMusic;      // Special music for tutorial scenes
    [SerializeField] private float musicFadeTime = 2f;

    [Header("Player Movement Audio")]
    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private AudioClip footstepSound;
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private AudioClip shootSound;
    [SerializeField] private AudioClip buttonClickSound;
    [SerializeField] private AudioClip pickupSound;       // Sound for picking up bonus items
    [SerializeField] private float playerAudioVolume = 0.7f;

    [Header("Footstep Settings")]
    [SerializeField] private float footstepInterval = 0.5f;
    private float lastFootstepTime;
    private bool isMoving = false;

    [Header("Individual Volume Controls")]
    [SerializeField] private float masterVolume = 1f;
    [SerializeField] private float musicVolume = 0.5f;
    [SerializeField] private float sfxVolume = 0.7f;
    [SerializeField] private float uiVolume = 0.8f;
    [SerializeField] private float playerVolume = 0.7f;
    [SerializeField] private float footstepVolume = 0.3f;
    [SerializeField] private float jumpVolume = 0.7f;
    [SerializeField] private float hitVolume = 0.7f;
    [SerializeField] private float shootVolume = 0.7f;
    [SerializeField] private float deathVolume = 0.8f;
    [SerializeField] private float pickupVolume = 0.6f;
    [SerializeField] private float buttonClickVolume = 0.8f;

    [Header("Audio Settings")]
    [SerializeField] private bool playMusicOnStart = true;
    [SerializeField] private bool loopBackgroundMusic = true;
    [SerializeField] private bool enableFootsteps = true;

    // Singleton pattern
    public static AudioManager Instance { get; private set; }

    // Scene-to-music mapping
    private Dictionary<string, AudioClip> sceneMusicMap;

    // Current playing music
    private AudioClip currentMusic;
    private Coroutine musicFadeCoroutine;

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudioManager();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Subscribe to scene loading events
        SceneManager.sceneLoaded += OnSceneLoaded;
        
        // Play music for current scene or default music if no music is playing
        if (playMusicOnStart)
        {
            if (!IsMusicPlaying())
            {
                PlaySceneMusic();
            }
        }
    }

    void OnDestroy()
    {
        // Unsubscribe from events
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void InitializeAudioManager()
    {
        // Create audio sources if they don't exist
        if (musicSource == null)
        {
            GameObject musicObj = new GameObject("MusicSource");
            musicObj.transform.SetParent(transform);
            musicSource = musicObj.AddComponent<AudioSource>();
            musicSource.playOnAwake = false;
            musicSource.loop = true;
            musicSource.volume = musicVolume;
        }

        if (sfxSource == null)
        {
            GameObject sfxObj = new GameObject("SFXSource");
            sfxObj.transform.SetParent(transform);
            sfxSource = sfxObj.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            sfxSource.loop = false;
            sfxSource.volume = 1f;
        }

        if (uiSource == null)
        {
            GameObject uiObj = new GameObject("UISource");
            uiObj.transform.SetParent(transform);
            uiSource = uiObj.AddComponent<AudioSource>();
            uiSource.playOnAwake = false;
            uiSource.loop = false;
            uiSource.volume = 0.8f;
        }

        if (playerSource == null)
        {
            GameObject playerObj = new GameObject("PlayerSource");
            playerObj.transform.SetParent(transform);
            playerSource = playerObj.AddComponent<AudioSource>();
            playerSource.playOnAwake = false;
            playerSource.loop = false;
            playerSource.volume = playerAudioVolume;
        }

        // Initialize scene-to-music mapping
        sceneMusicMap = new Dictionary<string, AudioClip>();
        if (backgroundMusic != null && sceneNames != null)
        {
            for (int i = 0; i < Mathf.Min(backgroundMusic.Length, sceneNames.Length); i++)
            {
                if (backgroundMusic[i] != null && !string.IsNullOrEmpty(sceneNames[i]))
                {
                    sceneMusicMap[sceneNames[i]] = backgroundMusic[i];
                }
            }
        }

        // Set music source properties
        musicSource.loop = loopBackgroundMusic;
        musicSource.volume = musicVolume * masterVolume;
        
        // Set initial volumes for all audio sources
        UpdateAllVolumes();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"Scene loaded: {scene.name}");
        // Only play scene music if no music is currently playing or if this scene has specific music
        if (!IsMusicPlaying() || HasSceneSpecificMusic(scene.name))
        {
            PlaySceneMusic();
        }
    }

    #region Background Music Methods

    public void PlaySceneMusic()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        PlaySceneMusic(currentSceneName);
    }

    public void PlaySceneMusic(string sceneName)
    {
        // Check for special scenes first (main menu and tutorial)
        if (IsMainMenuScene(sceneName))
        {
            if (mainMenuMusic != null && mainMenuMusic != currentMusic)
            {
                PlayBackgroundMusic(mainMenuMusic);
                Debug.Log($"[AudioManager] Playing main menu music for scene: {sceneName}");
            }
            return;
        }
        
        if (IsTutorialScene(sceneName))
        {
            if (tutorialMusic != null && tutorialMusic != currentMusic)
            {
                PlayBackgroundMusic(tutorialMusic);
                Debug.Log($"[AudioManager] Playing tutorial music for scene: {sceneName}");
            }
            return;
        }
        
        // Check for scene-specific music
        if (sceneMusicMap.ContainsKey(sceneName))
        {
            AudioClip musicToPlay = sceneMusicMap[sceneName];
            if (musicToPlay != null && musicToPlay != currentMusic)
            {
                PlayBackgroundMusic(musicToPlay);
                Debug.Log($"[AudioManager] Playing scene-specific music for: {sceneName}");
            }
        }
        else
        {
            // If no specific music for this scene, use default background music
            PlayDefaultBackgroundMusic();
        }
    }

    public void PlayBackgroundMusic(AudioClip music)
    {
        if (music == null) return;

        currentMusic = music;
        
        // Stop current music with fade out
        if (musicFadeCoroutine != null)
        {
            StopCoroutine(musicFadeCoroutine);
        }
        
        musicFadeCoroutine = StartCoroutine(FadeMusic(musicSource.clip, music));
    }

    public void StopBackgroundMusic()
    {
        if (musicFadeCoroutine != null)
        {
            StopCoroutine(musicFadeCoroutine);
        }
        
        musicFadeCoroutine = StartCoroutine(FadeOutMusic());
    }

    private IEnumerator FadeMusic(AudioClip fromClip, AudioClip toClip)
    {
        // Fade out current music
        if (fromClip != null)
        {
            yield return StartCoroutine(FadeOutMusic());
        }

        // Change clip and fade in new music
        musicSource.clip = toClip;
        musicSource.Play();
        yield return StartCoroutine(FadeInMusic());
    }

    private IEnumerator FadeOutMusic()
    {
        float startVolume = musicSource.volume;
        float elapsedTime = 0f;

        while (elapsedTime < musicFadeTime)
        {
            elapsedTime += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsedTime / musicFadeTime);
            yield return null;
        }

        musicSource.volume = 0f;
        musicSource.Stop();
    }

    private IEnumerator FadeInMusic()
    {
        float targetVolume = musicVolume * masterVolume;
        float elapsedTime = 0f;

        while (elapsedTime < musicFadeTime)
        {
            elapsedTime += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(0f, targetVolume, elapsedTime / musicFadeTime);
            yield return null;
        }

        musicSource.volume = targetVolume;
    }

    public void PlayDefaultBackgroundMusic()
    {
        if (defaultBackgroundMusic != null)
        {
            if (currentMusic != defaultBackgroundMusic)
            {
                PlayBackgroundMusic(defaultBackgroundMusic);
            }
            else if (!IsMusicPlaying())
            {
                // If it's the same music but not playing, just start it without fade
                musicSource.clip = defaultBackgroundMusic;
                musicSource.Play();
                currentMusic = defaultBackgroundMusic;
            }
        }
        else
        {
            Debug.LogWarning("No default background music assigned!");
        }
    }

    private bool HasSceneSpecificMusic(string sceneName)
    {
        return sceneMusicMap.ContainsKey(sceneName);
    }

    private bool IsMainMenuScene(string sceneName)
    {
        string sceneLower = sceneName.ToLower();
        return sceneLower.Contains("mainmenu") || sceneLower.Contains("main_menu") || 
               sceneLower == "mainmenu" || sceneLower == "main_menu";
    }

    private bool IsTutorialScene(string sceneName)
    {
        string sceneLower = sceneName.ToLower();
        return sceneLower.Contains("tutorial") || sceneLower.Contains("tut") ||
               sceneLower.StartsWith("tutorial") || sceneLower.Contains("tutorial 1");
    }

    #endregion

    #region Player Movement Audio Methods

    public void PlayJumpSound()
    {
        if (jumpSound != null)
        {
            float volume = jumpVolume * playerVolume * masterVolume;
            playerSource.PlayOneShot(jumpSound, volume);
            Debug.Log("Jump sound played");
        }
    }

    public void PlayHitSound()
    {
        if (hitSound != null)
        {
            float volume = hitVolume * playerVolume * masterVolume;
            playerSource.PlayOneShot(hitSound, volume);
            Debug.Log("Hit sound played");
        }
    }

    public void PlayDeathSound()
    {
        if (deathSound != null)
        {
            float volume = deathVolume * playerVolume * masterVolume;
            playerSource.PlayOneShot(deathSound, volume);
            Debug.Log("Death sound played");
        }
    }

    public void PlayShootSound()
    {
        if (shootSound != null)
        {
            float volume = shootVolume * playerVolume * masterVolume;
            playerSource.PlayOneShot(shootSound, volume);
            Debug.Log("Shoot sound played");
        }
    }

    public void PlayButtonClickSound()
    {
        if (buttonClickSound != null)
        {
            float volume = buttonClickVolume * uiVolume * masterVolume;
            uiSource.PlayOneShot(buttonClickSound, volume);
            Debug.Log("Button click sound played");
        }
    }

    public void PlayPickupSound()
    {
        if (pickupSound != null)
        {
            float volume = pickupVolume * sfxVolume * masterVolume;
            sfxSource.PlayOneShot(pickupSound, volume);
            Debug.Log("Pickup sound played");
        }
    }

    public void SetPlayerMoving(bool moving)
    {
        isMoving = moving;
    }

    public void StopMovementSounds()
    {
        isMoving = false;
        if (playerSource != null && playerSource.isPlaying)
        {
            playerSource.Stop();
        }
        Debug.Log("Movement sounds stopped");
    }

    public void PlayFootstepSound()
    {
        if (!enableFootsteps || footstepSound == null || !isMoving) return;

        // Check if enough time has passed since last footstep
        if (Time.time - lastFootstepTime >= footstepInterval)
        {
            float volume = footstepVolume * playerVolume * masterVolume;
            playerSource.PlayOneShot(footstepSound, volume);
            lastFootstepTime = Time.time;
        }
    }

    #endregion

    #region Volume Control Methods

    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        UpdateAllVolumes();
        AudioListener.volume = masterVolume;
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        UpdateAllVolumes();
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        UpdateAllVolumes();
    }

    public void SetUIVolume(float volume)
    {
        uiVolume = Mathf.Clamp01(volume);
        UpdateAllVolumes();
    }

    public void SetPlayerVolume(float volume)
    {
        playerVolume = Mathf.Clamp01(volume);
        UpdateAllVolumes();
    }

    public void SetJumpVolume(float volume)
    {
        jumpVolume = Mathf.Clamp01(volume);
    }

    public void SetHitVolume(float volume)
    {
        hitVolume = Mathf.Clamp01(volume);
    }

    public void SetShootVolume(float volume)
    {
        shootVolume = Mathf.Clamp01(volume);
    }

    public void SetDeathVolume(float volume)
    {
        deathVolume = Mathf.Clamp01(volume);
    }

    public void SetPickupVolume(float volume)
    {
        pickupVolume = Mathf.Clamp01(volume);
    }

    public void SetButtonClickVolume(float volume)
    {
        buttonClickVolume = Mathf.Clamp01(volume);
    }

    public void SetFootstepVolume(float volume)
    {
        footstepVolume = Mathf.Clamp01(volume);
    }

    private void UpdateAllVolumes()
    {
        if (musicSource != null)
            musicSource.volume = musicVolume * masterVolume;
        
        if (sfxSource != null)
            sfxSource.volume = sfxVolume * masterVolume;
        
        if (uiSource != null)
            uiSource.volume = uiVolume * masterVolume;
        
        if (playerSource != null)
            playerSource.volume = playerVolume * masterVolume;
    }

    #endregion

    #region Utility Methods

    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (clip != null && sfxSource != null)
        {
            float finalVolume = volume * sfxVolume * masterVolume;
            sfxSource.PlayOneShot(clip, finalVolume);
        }
    }

    public void PlayUISFX(AudioClip clip, float volume = 1f)
    {
        if (clip != null && uiSource != null)
        {
            float finalVolume = volume * uiVolume * masterVolume;
            uiSource.PlayOneShot(clip, finalVolume);
        }
    }

    public void PlayPlayerSFX(AudioClip clip, float volume = 1f)
    {
        if (clip != null && playerSource != null)
        {
            float finalVolume = volume * playerVolume * masterVolume;
            playerSource.PlayOneShot(clip, finalVolume);
        }
    }

    public bool IsMusicPlaying()
    {
        return musicSource != null && musicSource.isPlaying;
    }

    public void PauseMusic()
    {
        if (musicSource != null && musicSource.isPlaying)
        {
            musicSource.Pause();
        }
    }

    public void ResumeMusic()
    {
        if (musicSource != null && !musicSource.isPlaying)
        {
            musicSource.UnPause();
        }
    }

    /// <summary>
    /// Start playing the default background music immediately (no fade)
    /// Use this to start continuous background music that will loop across scenes
    /// </summary>
    public void StartContinuousBackgroundMusic()
    {
        if (defaultBackgroundMusic != null)
        {
            musicSource.clip = defaultBackgroundMusic;
            musicSource.volume = musicVolume * masterVolume;
            musicSource.Play();
            currentMusic = defaultBackgroundMusic;
            Debug.Log("Started continuous background music: " + defaultBackgroundMusic.name);
        }
        else
        {
            Debug.LogWarning("No default background music assigned! Cannot start continuous music.");
        }
    }

    /// <summary>
    /// Force play default background music even if music is already playing
    /// This will restart the default music
    /// </summary>
    public void ForcePlayDefaultMusic()
    {
        if (defaultBackgroundMusic != null)
        {
            musicSource.clip = defaultBackgroundMusic;
            musicSource.volume = musicVolume * masterVolume;
            musicSource.Play();
            currentMusic = defaultBackgroundMusic;
            Debug.Log("Force started default background music: " + defaultBackgroundMusic.name);
        }
    }

    /// <summary>
    /// Play main menu music
    /// </summary>
    public void PlayMainMenuMusic()
    {
        if (mainMenuMusic != null)
        {
            PlayBackgroundMusic(mainMenuMusic);
            Debug.Log("Playing main menu music: " + mainMenuMusic.name);
        }
        else
        {
            Debug.LogWarning("No main menu music assigned!");
        }
    }

    /// <summary>
    /// Play tutorial music
    /// </summary>
    public void PlayTutorialMusic()
    {
        if (tutorialMusic != null)
        {
            PlayBackgroundMusic(tutorialMusic);
            Debug.Log("Playing tutorial music: " + tutorialMusic.name);
        }
        else
        {
            Debug.LogWarning("No tutorial music assigned!");
        }
    }

    #endregion

    #region Inspector Helper Methods

    [ContextMenu("Test Jump Sound")]
    public void TestJumpSound()
    {
        PlayJumpSound();
    }

    [ContextMenu("Test Hit Sound")]
    public void TestHitSound()
    {
        PlayHitSound();
    }

    [ContextMenu("Test Footstep Sound")]
    public void TestFootstepSound()
    {
        PlayFootstepSound();
    }

    [ContextMenu("Test Button Click Sound")]
    public void TestButtonClickSound()
    {
        PlayButtonClickSound();
    }

    [ContextMenu("Test Pickup Sound")]
    public void TestPickupSound()
    {
        PlayPickupSound();
    }

    [ContextMenu("Test Stop Movement Sounds")]
    public void TestStopMovementSounds()
    {
        StopMovementSounds();
    }

    [ContextMenu("Start Continuous Background Music")]
    public void TestStartContinuousMusic()
    {
        StartContinuousBackgroundMusic();
    }

    [ContextMenu("Play Default Background Music")]
    public void TestPlayDefaultMusic()
    {
        PlayDefaultBackgroundMusic();
    }

    [ContextMenu("Force Play Default Music")]
    public void TestForcePlayDefaultMusic()
    {
        ForcePlayDefaultMusic();
    }

    [ContextMenu("Play Main Menu Music")]
    public void TestPlayMainMenuMusic()
    {
        PlayMainMenuMusic();
    }

    [ContextMenu("Play Tutorial Music")]
    public void TestPlayTutorialMusic()
    {
        PlayTutorialMusic();
    }

    [ContextMenu("Reset All Volumes to Default")]
    public void ResetAllVolumesToDefault()
    {
        masterVolume = 1f;
        musicVolume = 0.5f;
        sfxVolume = 0.7f;
        uiVolume = 0.8f;
        playerVolume = 0.7f;
        footstepVolume = 0.3f;
        jumpVolume = 0.7f;
        hitVolume = 0.7f;
        shootVolume = 0.7f;
        deathVolume = 0.8f;
        pickupVolume = 0.6f;
        buttonClickVolume = 0.8f;
        UpdateAllVolumes();
        Debug.Log("All volumes reset to default values");
    }

    #endregion

    void Update()
    {
        // Handle footstep sounds based on player movement
        if (enableFootsteps && isMoving)
        {
            PlayFootstepSound();
        }
    }
}
