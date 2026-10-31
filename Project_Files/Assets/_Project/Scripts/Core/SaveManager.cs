using UnityEngine;

public class SaveManager : MonoBehaviour
{
    private static SaveManager _instance;

    public static SaveManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<SaveManager>();

                if (_instance == null)
                {
                    GameObject rawManagerObject = new GameObject("Runtime_SaveManager");
                    _instance = rawManagerObject.AddComponent<SaveManager>();
                    Debug.Log("SaveManager: No instance found. Dynamically spawned tracking engine at runtime.");
                }
            }
            return _instance;
        }
    }

    // Storage Keys
    private const string HIGH_SCORE_KEY = "AsteroidScavenger_HighScore";
    private const string MUSIC_VOLUME_KEY = "SavedMusicVolume";
    private const string SFX_VOLUME_KEY = "SavedSFXVolume";

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    public void SaveHighScore(int scoreToSave)
    {
        int currentHighScore = LoadHighScore();

        if (scoreToSave > currentHighScore)
        {
            PlayerPrefs.SetInt(HIGH_SCORE_KEY, scoreToSave);
            PlayerPrefs.Save();
            Debug.Log($"SaveManager: New High Score Saved! Value: {scoreToSave}");
        }
    }

    public int LoadHighScore()
    {
        return PlayerPrefs.GetInt(HIGH_SCORE_KEY, 0);
    }

    public void SaveMusicVolume(float volume)
    {
        PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, Mathf.Clamp01(volume));
        PlayerPrefs.Save();
    }

    public float LoadMusicVolume(float defaultFallback = 0.5f)
    {
        return PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, defaultFallback);
    }

    public void SaveSFXVolume(float volume)
    {
        PlayerPrefs.SetFloat(SFX_VOLUME_KEY, Mathf.Clamp01(volume));
        PlayerPrefs.Save();
    }

    public float LoadSFXVolume(float defaultFallback = 0.7f)
    {
        return PlayerPrefs.GetFloat(SFX_VOLUME_KEY, defaultFallback);
    }
}
