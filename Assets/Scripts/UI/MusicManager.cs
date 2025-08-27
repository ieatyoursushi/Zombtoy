using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

/// <summary>
/// Centralized audio management system
/// Handles music, SFX, and volume controls
/// Scalable for multiplayer audio needs
/// </summary>
public class MusicManager : Singleton<MusicManager>
{
    // Singleton overrides
    protected override bool AllowAutoCreate => false; // Place explicitly in first menu or bootstrap scene
    protected override bool Persistent => true;       // Persist so music keeps playing
    protected override bool LogCreation => false;
    [Header("Audio Configuration")]
    [SerializeField] private AudioMixer masterMixer;
    [SerializeField] private AudioSource[] backgroundMusicSources;
    [SerializeField] private AudioSource menuMusic; // optional menu music (can be empty)
    private AudioSource activeBackgroundMusic;
    [SerializeField] private AudioSource sfxSource;
    
    [Header("UI References")]
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Toggle muteToggle;
    
    [Header("Volume Settings")]
    [Range(0f, 1f)] public float masterVolume = 1f;
    [Range(0f, 1f)] public float musicVolume = 0.354f;
    [Range(0f, 1f)] public float sfxVolume = 1f;
    
    // Properties
    public bool IsMuted { get; private set; }
    public float MasterVolume => masterVolume;
    public float MusicVolume => musicVolume;
    public float SFXVolume => sfxVolume;
    
    // Events
    public System.Action<float> OnVolumeChanged;
    public System.Action<bool> OnMuteToggled;
    
    // Static properties for backwards compatibility
    public static bool MusicOn 
    { 
        get => Instance != null && !Instance.IsMuted;
        set => Instance?.SetMuted(!value);
    }
    public static float volumeValue => Instance?.musicVolume ?? 0.354f;
    
    protected override void Awake()
    {
        Debug.Log($"[MusicManager] Awake called on {gameObject.name} in scene {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}");
        base.Awake();
        LoadAudioSettings();
    }
    
    private void Start()
    {
        InitializeAudio();
        InitializeUI();
        SubscribeToEvents();
    }
    
    void OnEnable()
    {
        // Re-initialize dependencies when entering a new scene
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        Debug.Log($"[MusicManager] Scene loaded: {scene.name}, refreshing audio sources");
    // Stop any background music immediately to avoid playOnAwake races
    StopAllBackgroundMusic();
    // Reset references so they get re-found in the new scene
    backgroundMusicSources = null;
        volumeSlider = null;
        muteToggle = null;
        InitializeAudio();
        InitializeUI();
        // If this is not a gameplay scene, ensure music is stopped
        if (!IsGameplayScene(scene.name))
        {
            StopAllBackgroundMusic();
            StopMenuMusic();
        }
    }
    
    protected override void OnDestroy()
    {
        base.OnDestroy();
        UnsubscribeFromEvents();
        SaveAudioSettings();
    }
    
    private void InitializeAudio()
    {
        // Find background music sources if not assigned
    if (backgroundMusicSources == null || backgroundMusicSources.Length == 0)
        {
            var holder = GameObject.Find("BackgroundMusicSources") ?? GameObject.Find("BackgroundMusic");
            if (holder != null)
            {
                backgroundMusicSources = holder.GetComponentsInChildren<AudioSource>(true);
            }
            else
            {
                // Fallback: find AudioSources with names starting with BackgroundMusic
                var all = GameObject.FindObjectsOfType<AudioSource>();
                var list = new System.Collections.Generic.List<AudioSource>();
                foreach (var a in all)
                {
                    if (a.gameObject.name.ToLower().StartsWith("backgroundmusic"))
                        list.Add(a);
                }
                if (list.Count > 0)
                    backgroundMusicSources = list.ToArray();
            }
        }

        // Immediately deactivate background music GameObjects to avoid PlayOnAwake races
        if (backgroundMusicSources != null)
        {
            foreach (var src in backgroundMusicSources)
            {
                if (src == null || src.gameObject == null) continue;
                try
                {
                    // prefer manager control over GameObject active state
                    src.playOnAwake = false;
                    src.Stop();
                }
                catch { }
            }
        }
    // Dump what we discovered so we can debug why nothing is playing
    DumpDiscoveredAudioSources();
        
        // Create SFX source if needed
        if (sfxSource == null)
        {
            GameObject sfxGO = new GameObject("SFX Source");
            sfxGO.transform.SetParent(transform);
            sfxSource = sfxGO.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
        }
        
        // Apply initial settings
        UpdateAudioSettings();
        
        // Do not auto-play here for all scenes. Music should start when a game run begins
        // or when we detect a gameplay scene. If we're in a gameplay scene and sources exist,
        // pick one randomly for this game instance.
        var sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (IsGameplayScene(sceneName))
        {
            PickAndPlayBackgroundMusic();
        }
        else
        {
            if (menuMusic != null && !IsMuted)
                menuMusic.Play();
        }
    }
    
    private void InitializeUI()
    {
        // Find volume slider if not assigned
        if (volumeSlider == null)
        {
            var sliderGO = GameObject.Find("VolumeSlider") ?? GameObject.Find("MusicSlider") ?? GameObject.Find("Volume");
            if (sliderGO != null)
                volumeSlider = sliderGO.GetComponent<Slider>();
        }
        
        // Find mute toggle if not assigned
        if (muteToggle == null)
        {
            var toggleGO = GameObject.Find("MuteToggle") ?? GameObject.Find("MusicToggle") ?? GameObject.Find("Mute");
            if (toggleGO != null)
                muteToggle = toggleGO.GetComponent<Toggle>();
        }
        
        if (volumeSlider != null)
        {
            volumeSlider.value = musicVolume;
            volumeSlider.onValueChanged.RemoveAllListeners(); // Remove old listeners
            volumeSlider.onValueChanged.AddListener(SetMusicVolume);
        }
        
        if (muteToggle != null)
        {
            muteToggle.isOn = !IsMuted;
            muteToggle.onValueChanged.RemoveAllListeners(); // Remove old listeners
            muteToggle.onValueChanged.AddListener(SetMuted);
        }
    }
    
    private void SubscribeToEvents()
    {
        GameEvents.OnGamePaused += HandleGamePaused;
        GameEvents.OnGameResumed += HandleGameResumed;
    GameEvents.OnGameStarted += HandleGameStarted;
    GameEvents.OnPlayerDeath += HandleGameEnded;
    GameEvents.OnGameOver += HandleGameEnded;
    }
    
    private void UnsubscribeFromEvents()
    {
        GameEvents.OnGamePaused -= HandleGamePaused;
        GameEvents.OnGameResumed -= HandleGameResumed;
    GameEvents.OnGameStarted -= HandleGameStarted;
    GameEvents.OnPlayerDeath -= HandleGameEnded;
    GameEvents.OnGameOver -= HandleGameEnded;
    }
    
    private void HandleGamePaused()
    {
        if (activeBackgroundMusic != null)
            activeBackgroundMusic.Pause();
        if (menuMusic != null && menuMusic.isPlaying)
            menuMusic.Pause();
    }
    
    private void HandleGameResumed()
    {
        if (activeBackgroundMusic != null && !IsMuted)
            activeBackgroundMusic.UnPause();
        if (menuMusic != null && !IsMuted && menuMusic.isPlaying)
            menuMusic.UnPause();
    }

    private void HandleGameStarted()
    {
        if (backgroundMusicSources == null || backgroundMusicSources.Length == 0)
            InitializeAudio();

        PickAndPlayBackgroundMusic();
    }

    private void HandleGameEnded()
    {
        // Stop music when the player dies or the game ends
        StopAllBackgroundMusic();
        StopMenuMusic();
    }

    private bool IsGameplayScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName)) return false;
        sceneName = sceneName.ToLowerInvariant();
        return sceneName.Contains("level") || sceneName.Contains("game") || sceneName.Contains("play");
    }
    
    private void Update()
    {
        // Update volume from slider (backwards compatibility)
        if (volumeSlider != null && !Mathf.Approximately(volumeSlider.value, musicVolume))
        {
            SetMusicVolume(volumeSlider.value);
        }
        
        UpdateAudioSettings();
    }
    
    private void UpdateAudioSettings()
    {
        if (activeBackgroundMusic != null)
        {
            activeBackgroundMusic.volume = IsMuted ? 0f : musicVolume * masterVolume;
        }
        // Ensure menu music respects mute
        if (menuMusic != null)
        {
            menuMusic.volume = IsMuted ? 0f : musicVolume * masterVolume;
        }
        
        if (sfxSource != null)
        {
            sfxSource.volume = IsMuted ? 0f : sfxVolume * masterVolume;
        }
        
        // Apply to audio mixer if available
        if (masterMixer != null)
        {
            masterMixer.SetFloat("MasterVolume", IsMuted ? -80f : Mathf.Log10(masterVolume) * 20);
            masterMixer.SetFloat("MusicVolume", Mathf.Log10(musicVolume) * 20);
            masterMixer.SetFloat("SFXVolume", Mathf.Log10(sfxVolume) * 20);
        }
    }
    
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        UpdateAudioSettings();
        OnVolumeChanged?.Invoke(masterVolume);
    }
    
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        UpdateAudioSettings();
        OnVolumeChanged?.Invoke(musicVolume);
    }
    
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        UpdateAudioSettings();
    }
    
    public void SetMuted(bool muted)
    {
        IsMuted = muted;
        UpdateAudioSettings();
        OnMuteToggled?.Invoke(IsMuted);
        
        if (muteToggle != null)
            muteToggle.isOn = !IsMuted;
    }
    
    public void ToggleMute()
    {
        SetMuted(!IsMuted);
    }
    
    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (sfxSource != null && clip != null && !IsMuted)
        {
            sfxSource.PlayOneShot(clip, volume * sfxVolume * masterVolume);
        }
    }
    
    public void PlaySFXAtPoint(AudioClip clip, Vector3 position, float volume = 1f)
    {
        if (clip != null && !IsMuted)
        {
            AudioSource.PlayClipAtPoint(clip, position, volume * sfxVolume * masterVolume);
        }
    }
    
    private void LoadAudioSettings()
    {
        masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.354f);
        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
        IsMuted = PlayerPrefs.GetInt("AudioMuted", 0) == 1;
    }
    
    private void SaveAudioSettings()
    {
        PlayerPrefs.SetFloat("MasterVolume", masterVolume);
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
        PlayerPrefs.SetInt("AudioMuted", IsMuted ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void PickAndPlayBackgroundMusic()
    {
        StopMenuMusic();

        if (backgroundMusicSources == null || backgroundMusicSources.Length == 0)
            return;
        // If we already have an active background music playing for this run, keep it
        if (activeBackgroundMusic != null && activeBackgroundMusic.isPlaying)
        {
            Debug.Log($"[MusicManager] Background music already playing: {activeBackgroundMusic.gameObject.name}");
            return;
        }

        // Stop all first
        foreach (var src in backgroundMusicSources)
        {
            if (src == null) continue;
            src.Stop();
        }

        // Choose a random source for this game instance
        var valid = new System.Collections.Generic.List<AudioSource>();
        foreach (var s in backgroundMusicSources) if (s != null) valid.Add(s);
        if (valid.Count == 0) return;

        int idx = Random.Range(0, valid.Count);
        activeBackgroundMusic = valid[idx];

        Debug.Log($"[MusicManager] Picked background music index={idx} name={(activeBackgroundMusic != null ? activeBackgroundMusic.gameObject.name : "<null>")}");

        if (activeBackgroundMusic != null)
        {
            if (!IsMuted)
                activeBackgroundMusic.Play();
        }
    }

    private void StopAllBackgroundMusic()
    {
        if (backgroundMusicSources != null)
        {
            foreach (var src in backgroundMusicSources)
            {
                if (src == null) continue;
                src.Stop();
            }
        }
        activeBackgroundMusic = null;
    }

    private void StopMenuMusic()
    {
        if (menuMusic != null)
        {
            menuMusic.Stop();
        }
    }

    private void DumpDiscoveredAudioSources()
    {
        if (backgroundMusicSources == null)
        {
            Debug.Log("[MusicManager] No backgroundMusicSources discovered");
            return;
        }

        Debug.Log($"[MusicManager] Discovered {backgroundMusicSources.Length} backgroundMusicSources:");
        for (int i = 0; i < backgroundMusicSources.Length; i++)
        {
            var s = backgroundMusicSources[i];
            if (s == null)
            {
                Debug.Log($"  [{i}] <null>");
                continue;
            }
            Debug.Log($"  [{i}] name={s.gameObject.name} clip={(s.clip!=null? s.clip.name : "<null>")} playOnAwake={s.playOnAwake} isPlaying={s.isPlaying} volume={s.volume}");
        }
    }
    
    // Static methods for backwards compatibility
    public static void PlaySound(AudioClip clip) => Instance?.PlaySFX(clip);
    public static void PlaySoundAtPosition(AudioClip clip, Vector3 pos) => Instance?.PlaySFXAtPoint(clip, pos);
    public static void SetGlobalMute(bool muted) => Instance?.SetMuted(muted);
}
