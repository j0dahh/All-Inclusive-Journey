using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Background Music")]
    public AudioClip mainMenuMusic;
    public AudioClip gameSelectionMusic;
    public AudioClip[] gameMusicVariations; // For different mini-games

    [Header("Settings")]
    public float musicVolume = 0.3f;
    public float fadeDuration = 1.5f;

    private AudioSource musicSource;
    private string currentScene;

    void Awake()
    {
        // Singleton pattern - only one AudioManager exists
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist between scenes
            InitializeAudio();
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void InitializeAudio()
    {
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.volume = musicVolume;
        musicSource.loop = true;
        musicSource.playOnAwake = false;

        // Listen for scene changes
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        string sceneName = scene.name;

        // Don't change music if we're already on the right track
        if (sceneName == currentScene) return;

        currentScene = sceneName;
        HandleSceneMusic(sceneName);
    }

    void HandleSceneMusic(string sceneName)
    {
        AudioClip newClip = null;

        // Determine which music to play based on scene
        if (sceneName == "MainMenu")
        {
            newClip = mainMenuMusic;
        }
        else if (sceneName == "GameSelection")
        {
            newClip = gameSelectionMusic;
        }
        else if (sceneName == "ColorMatcher")
        {
            newClip = gameMusicVariations[0]; // First game variation
        }
        else if (sceneName == "ShapeSorter")
        {
            newClip = gameMusicVariations[1]; // Second game variation
        }
        // Add more scenes as needed

        // Change music with smooth fade
        if (newClip != null)
        {
            StartCoroutine(SwitchMusic(newClip));
        }
    }

    public IEnumerator SwitchMusic(AudioClip newClip)
    {
        // Fade out current music
        if (musicSource.isPlaying)
        {
            float startVolume = musicSource.volume;

            for (float t = 0; t < fadeDuration; t += Time.deltaTime)
            {
                musicSource.volume = Mathf.Lerp(startVolume, 0, t / fadeDuration);
                yield return null;
            }

            musicSource.Stop();
            musicSource.volume = startVolume; // Reset volume
        }

        // Play new music with fade in
        musicSource.clip = newClip;
        musicSource.Play();

        float targetVolume = musicVolume;
        musicSource.volume = 0;

        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            musicSource.volume = Mathf.Lerp(0, targetVolume, t / fadeDuration);
            yield return null;
        }

        musicSource.volume = targetVolume;
    }

    // Public method to change volume if needed
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        musicSource.volume = musicVolume;
    }

    // Clean up
    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}