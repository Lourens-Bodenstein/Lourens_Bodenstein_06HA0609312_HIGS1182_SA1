using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [Header("Scene Configuration")]
    [SerializeField] private string gameplaySceneName = "GameplayScene";

    [Header("UI Reference")]
    [SerializeField] private TextMeshProUGUI highScoreDisplay;

    [Header("Options Sub-Menu UI")]
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private Image optionsBackgroundImage;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    [Header("Volume Percentage Labels")]
    [SerializeField] private TextMeshProUGUI musicPercentageText; // Drag Music % Text here
    [SerializeField] private TextMeshProUGUI sfxPercentageText;   // Drag SFX % Text here

    [Header("Credits Sub-Menu UI")]
    [SerializeField] private GameObject creditsPanel;

    private bool isInitializing = false;

    private void Start()
    {
        DisplayHighScore();
        InitializeSliders();

        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(false);
    }

    public void DisplayHighScore()
    {
        if (highScoreDisplay != null && SaveManager.Instance != null)
        {
            int savedHighScore = SaveManager.Instance.LoadHighScore();
            highScoreDisplay.text = $"Personal High Score: {savedHighScore}";
        }
    }

    private void InitializeSliders()
    {
        if (SaveManager.Instance != null)
        {
            isInitializing = true;

            if (musicSlider != null)
            {
                float savedMusicValue = SaveManager.Instance.LoadMusicVolume(0.5f);
                musicSlider.value = savedMusicValue;
                UpdatePercentageText(musicPercentageText, savedMusicValue);
            }

            if (sfxSlider != null)
            {
                float savedSFXValue = SaveManager.Instance.LoadSFXVolume(0.7f);
                sfxSlider.value = savedSFXValue;
                UpdatePercentageText(sfxPercentageText, savedSFXValue);
            }

            isInitializing = false;
        }
    }

    // Helper routine to format the floating number into a clean percentage string
    private void UpdatePercentageText(TextMeshProUGUI targetText, float value)
    {
        if (targetText != null)
        {
            int percentageValue = Mathf.RoundToInt(value * 100f);
            targetText.text = $"{percentageValue}%";
        }
    }

    // --- OPTIONS MENU METHODS ---

    public void OpenOptions()
    {
        if (optionsPanel != null) optionsPanel.SetActive(true);
        if (creditsPanel != null) creditsPanel.SetActive(false);
        InitializeSliders();
    }

    public void CloseOptions()
    {
        if (optionsPanel != null) optionsPanel.SetActive(false);
    }

    public void ChangeOptionsBackgroundColor(Color newColor)
    {
        if (optionsBackgroundImage != null)
        {
            optionsBackgroundImage.color = newColor;
        }
    }

    public void OnMusicSliderChanged(float value)
    {
        if (isInitializing) return;

        if (SaveManager.Instance != null) SaveManager.Instance.SaveMusicVolume(value);
        if (AudioManager.Instance != null) AudioManager.Instance.SetMusicVolume(value);

        // Update percentage readout visually in real time
        UpdatePercentageText(musicPercentageText, value);
    }

    public void OnSFXSliderChanged(float value)
    {
        if (isInitializing) return;

        if (SaveManager.Instance != null) SaveManager.Instance.SaveSFXVolume(value);
        if (AudioManager.Instance != null) AudioManager.Instance.SetSFXVolume(value);

        // Update percentage readout visually in real time
        UpdatePercentageText(sfxPercentageText, value);
    }

    // --- CREDITS MENU METHODS ---

    public void OpenCredits()
    {
        if (creditsPanel != null) creditsPanel.SetActive(true);
        if (optionsPanel != null) optionsPanel.SetActive(false);
    }

    public void CloseCredits()
    {
        if (creditsPanel != null) creditsPanel.SetActive(false);
    }

    // --- GAME LOOP METHODS ---

    public void PlayGame()
    {
        if (string.IsNullOrEmpty(gameplaySceneName)) gameplaySceneName = "GameplayScene";
        SceneManager.LoadScene(gameplaySceneName);
    }

    public void QuitGame()
    {
        // Highlighted Cyan feedback message for quick testing visibility
        Debug.Log("<color=cyan>[MAIN MENU]</color> Quit Game button pressed. Shutting down application player...");

        Application.Quit();
    }
}
