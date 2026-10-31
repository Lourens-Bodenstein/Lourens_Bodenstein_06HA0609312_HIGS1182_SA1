using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    [Header("UI Canvas")]
    [Tooltip("Drag your Pause Menu Canvas or Panel GameObject here.")]
    [SerializeField] private GameObject pauseMenuUI;

    private bool isPaused = false;

    // Cache references so we can turn them back on later
    private PlayerController cachedController;
    private MonoBehaviour cachedWeaponSystem;

    private void Update()
    {
        // CRITICAL FIX: Only allow pausing if the game is actively in the Playing state
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameManager.GameState.Playing)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void ResumeGame()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;

        // Re-enable player scripts if they were found
        if (cachedController != null) cachedController.enabled = true;
        if (cachedWeaponSystem != null) cachedWeaponSystem.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void PauseGame()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;

        // Find the active Player Spaceship in the scene
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null)
        {
            player = GameObject.Find("PlayerSpaceship(Clone)");
        }

        if (player != null)
        {
            cachedController = player.GetComponent<PlayerController>();
            if (cachedController != null) cachedController.enabled = false;

            var weaponComponent = player.GetComponent("WeaponSystem") as MonoBehaviour;
            if (weaponComponent != null)
            {
                cachedWeaponSystem = weaponComponent;
                cachedWeaponSystem.enabled = false;
            }
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (UnityEngine.EventSystems.EventSystem.current != null)
        {
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
        }
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
