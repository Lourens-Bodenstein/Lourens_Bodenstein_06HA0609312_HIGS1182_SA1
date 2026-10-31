using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState { StartMenu, Playing, GameOver, Victory }
    public GameState CurrentState { get; private set; }

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI highScoreText;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject victoryPanel;

    private int currentScore = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        ChangeState(GameState.Playing);
        LoadAndDisplayHighScore();
    }

    public void ChangeState(GameState newState)
    {
        CurrentState = newState;
        switch (CurrentState)
        {
            case GameState.Playing:
                Time.timeScale = 1f;
                currentScore = 0;
                UpdateScoreUI();
                if (gameOverPanel) gameOverPanel.SetActive(false);
                if (victoryPanel) victoryPanel.SetActive(false);

                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;

                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayBackgroundMusic();
                }
                break;

            case GameState.GameOver:
                Time.timeScale = 0f;
                SaveCurrentSessionScore();
                if (gameOverPanel) gameOverPanel.SetActive(true);

                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;

                // Swap out game music for the specific loss theme
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayGameOverMusic();
                }
                break;

            case GameState.Victory:
                Time.timeScale = 0f;
                SaveCurrentSessionScore();
                if (victoryPanel) victoryPanel.SetActive(true);

                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;

                // Swap out game music for the specific victory theme
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayVictoryMusic();
                }
                break;
        }
    }

    public void AddScore(int amount)
    {
        if (CurrentState != GameState.Playing) return;

        currentScore += amount;
        UpdateScoreUI();

        if (SaveManager.Instance != null)
        {
            int savedHighScore = SaveManager.Instance.LoadHighScore();
            if (currentScore > savedHighScore)
            {
                SaveManager.Instance.SaveHighScore(currentScore);
                if (highScoreText != null)
                {
                    highScoreText.text = $"High Score: {currentScore}";
                }
            }
        }
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Scrap Collected: {currentScore}";
        }
    }

    public void LoadAndDisplayHighScore()
    {
        if (highScoreText != null && SaveManager.Instance != null)
        {
            int loadedRecord = SaveManager.Instance.LoadHighScore();
            highScoreText.text = $"High Score: {loadedRecord}";
        }
    }

    private void SaveCurrentSessionScore()
    {
        if (SaveManager.Instance != null)
        {
            int savedHighScore = SaveManager.Instance.LoadHighScore();
            if (currentScore > savedHighScore)
            {
                SaveManager.Instance.SaveHighScore(currentScore);
            }
        }
    }

    private void OnApplicationQuit()
    {
        SaveCurrentSessionScore();
    }

    public void TriggerGameOver()
    {
        ChangeState(GameState.GameOver);
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenuScene");
    }
}
