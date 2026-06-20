using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource backgroundSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Audio Clips")]
    public AudioClip backgroundMusic;
    public AudioClip celebrationMusic;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Keeps music playing across scenes
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        PlayBackgroundMusic();
    }

    public void PlayBackgroundMusic()
    {
        if (backgroundMusic == null) return;

        backgroundSource.clip = backgroundMusic;
        backgroundSource.loop = true;
        backgroundSource.volume = 0.4f; // Keep it low and unobtrusive
        backgroundSource.Play();
    }

    public void PlayCelebration()
    {
        // Stop the background loop so they don't clash
        backgroundSource.Stop();

        if (celebrationMusic == null) return;

        sfxSource.clip = celebrationMusic;
        sfxSource.loop = false; // Play only once
        sfxSource.volume = 0.7f; // Make it prominent
        sfxSource.Play();
    }
}
