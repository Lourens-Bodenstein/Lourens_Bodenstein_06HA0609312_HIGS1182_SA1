using UnityEngine;

[ExecuteAlways]
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Source References")]
    private AudioSource musicSource;
    private AudioSource sfxSource;

    [Header("Audio Clip Assets")]
    [SerializeField] private AudioClip backgroundMusic;
    [SerializeField] private AudioClip victoryMusic;
    [SerializeField] private AudioClip gameOverMusic;
    [SerializeField] private AudioClip laserShootSound;
    [SerializeField] private AudioClip scrapPickupSound;
    [SerializeField] private AudioClip playerHitSound;
    [SerializeField] private AudioClip explosionSound;

    [Header("Global Volume Settings")]
    [Range(0f, 1f)][SerializeField] private float musicVolume = 0.5f;
    [Range(0f, 1f)][SerializeField] private float sfxVolume = 0.7f;

    private void Awake()
    {
        if (Application.isPlaying)
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            musicSource = gameObject.AddComponent<AudioSource>();
            sfxSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
        }

        SyncWithDiskData();
    }

    private void Start()
    {
        if (Application.isPlaying)
        {
            PlayBackgroundMusic();
        }
    }

    private void OnValidate()
    {
        if (Application.isPlaying) return;
        SyncWithDiskData();
    }

    private void SyncWithDiskData()
    {
        if (PlayerPrefs.HasKey("SavedMusicVolume"))
        {
            musicVolume = PlayerPrefs.GetFloat("SavedMusicVolume", musicVolume);
        }

        if (PlayerPrefs.HasKey("SavedSFXVolume"))
        {
            sfxVolume = PlayerPrefs.GetFloat("SavedSFXVolume", sfxVolume);
        }

        if (musicSource != null) musicSource.volume = musicVolume;
    }

    public void PlayBackgroundMusic()
    {
        if (backgroundMusic == null || musicSource == null) return;

        musicSource.clip = backgroundMusic;
        musicSource.volume = musicVolume;
        musicSource.Play();
    }

    // Switches the music channel over to the victory loop
    public void PlayVictoryMusic()
    {
        if (victoryMusic == null || musicSource == null) return;

        musicSource.clip = victoryMusic;
        musicSource.volume = musicVolume;
        musicSource.Play();
    }

    // Switches the music channel over to the game over loop
    public void PlayGameOverMusic()
    {
        if (gameOverMusic == null || musicSource == null) return;

        musicSource.clip = gameOverMusic;
        musicSource.volume = musicVolume;
        musicSource.Play();
    }

    public void StopBackgroundMusic()
    {
        if (musicSource != null && musicSource.isPlaying)
        {
            musicSource.Stop();
        }
    }

    public void PlaySound(string clipName)
    {
        if (sfxSource == null) return;

        AudioClip targetClip = null;

        switch (clipName.ToLower())
        {
            case "laser": case "shoot": targetClip = laserShootSound; break;
            case "scrap": case "pickup": targetClip = scrapPickupSound; break;
            case "hit": case "gameover": targetClip = playerHitSound; break;
            case "explosion": case "detonate": targetClip = explosionSound; break;
            default:
                Debug.LogWarning($"AudioManager: Sound name '{clipName}' not found.");
                break;
        }

        if (targetClip != null)
        {
            sfxSource.PlayOneShot(targetClip, sfxVolume);
        }
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (musicSource != null) musicSource.volume = musicVolume;
        if (SaveManager.Instance != null) SaveManager.Instance.SaveMusicVolume(musicVolume);
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        if (SaveManager.Instance != null) SaveManager.Instance.SaveSFXVolume(sfxVolume);
    }

    public float GetMusicVolume() => musicVolume;
    public float GetSFXVolume() => sfxVolume;
}
