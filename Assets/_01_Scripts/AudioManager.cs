using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    private static AudioManager _instance;
    public static AudioManager Instance => _instance;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Music Tracks")]
    [SerializeField] private AudioClip titleTheme;        // Main Menu
    [SerializeField] private AudioClip fileSelectionTheme; // Character Selection
    [SerializeField] private AudioClip minishWoods;       // GYM Scenes
    [SerializeField] private AudioClip mtCrenelSummit;    // Gameplay

    [Header("Volume Settings")]
    [Range(0f, 1f)]
    [SerializeField] private float musicVolume = 0.5f;
    [Range(0f, 1f)]
    [SerializeField] private float sfxVolume = 0.7f;

    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 0.5f;

    private string _currentSceneName;
    private string _currentMusicName;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
        }

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
        }

        musicSource.volume = musicVolume;
        sfxSource.volume = sfxVolume;

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Force music to change based on new scene
        PlayMusicForScene(scene.name);
    }

    private void PlayMusicForScene(string sceneName)
    {
        AudioClip clipToPlay = GetMusicForScene(sceneName);
        string musicName = clipToPlay != null ? clipToPlay.name : "None";

        // Don't restart if same music is already playing
        if (_currentMusicName == musicName && musicSource.isPlaying)
        {
            Debug.Log($"[AudioManager] Same music already playing: {musicName}");
            return;
        }

        if (clipToPlay != null)
        {
            Debug.Log($"[AudioManager] Switching music for scene: {sceneName} → {musicName}");
            PlayMusic(clipToPlay, fadeDuration);
            _currentMusicName = musicName;
        }
    }

    private AudioClip GetMusicForScene(string sceneName)
    {
        // Main Menu
        if (sceneName.Contains("MainMenu") || sceneName == "SC_MainMenu")
        {
            return titleTheme;
        }
        
        // Character Selection
        if (sceneName.Contains("CharacterSelect") || sceneName == "SC_CharacterSelect")
        {
            return fileSelectionTheme;
        }
        
        // GYM Scenes
        if (sceneName.Contains("GYM") || sceneName.Contains("Gym"))
        {
            return minishWoods;
        }
        
        // Gameplay
        if (sceneName == "SC_Gameplay")
        {
            return mtCrenelSummit;
        }
        
        // Default fallback - keep current or play title
        return titleTheme;
    }

    public void PlayMusic(AudioClip clip, float fadeDuration = 0f)
    {
        if (clip == null) return;
        
        if (fadeDuration > 0f)
        {
            StartCoroutine(FadeToNewMusic(clip, fadeDuration));
        }
        else
        {
            musicSource.clip = clip;
            musicSource.Play();
        }
    }

    private IEnumerator FadeToNewMusic(AudioClip newClip, float duration)
    {
        // Fade out current music
        float startVolume = musicSource.volume;
        float timer = 0f;
        
        while (timer < duration)
        {
            timer += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(startVolume, 0f, timer / duration);
            yield return null;
        }
        
        // Switch clip
        musicSource.Stop();
        musicSource.clip = newClip;
        musicSource.Play();
        
        // Fade in new music
        timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(0f, musicVolume, timer / duration);
            yield return null;
        }
        
        musicSource.volume = musicVolume;
    }

    public void StopMusic(float fadeDuration = 0.5f)
    {
        if (fadeDuration > 0f)
        {
            StartCoroutine(FadeOutMusic(fadeDuration));
        }
        else
        {
            musicSource.Stop();
        }
    }

    private IEnumerator FadeOutMusic(float duration)
    {
        float startVolume = musicSource.volume;
        float timer = 0f;
        
        while (timer < duration)
        {
            timer += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(startVolume, 0f, timer / duration);
            yield return null;
        }
        
        musicSource.Stop();
        musicSource.volume = musicVolume;
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        musicSource.volume = musicVolume;
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        sfxSource.volume = sfxVolume;
    }
}