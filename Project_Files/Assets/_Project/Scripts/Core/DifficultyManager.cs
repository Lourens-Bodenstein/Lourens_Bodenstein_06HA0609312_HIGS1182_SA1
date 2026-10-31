using UnityEngine;
using System.Collections;
using TMPro;

public class DifficultyManager : MonoBehaviour
{
    public static DifficultyManager Instance { get; private set; }

    [Header("Wave Configuration")]
    [SerializeField] private TextMeshProUGUI waveTextDisplay;
    [SerializeField] private Camera gameplayCamera;

    // Configured wave durations (e.g., each wave lasts 20 seconds)
    [SerializeField] private float waveDuration = 20f;

    [Tooltip("How many seconds before the wave ends should the warning start flashing?")]
    [SerializeField] private float warningTriggerTime = 5f;

    private int currentWave = 1;
    private float waveTimer = 0f;

    // Internal track state mechanics to prevent starting overlapping coroutines
    private bool isWarningSequenceActive = false;
    private Coroutine flashingRoutineInstance;

    // Upgradable difficulty modifiers for later systems
    public float EnemySpeedMultiplier { get; private set; } = 1.0f;
    public float AsteroidSpawnModifier { get; private set; } = 1.0f;

    // Deep space backdrop space target shift styles per wave array
    private Color[] waveColors = new Color[]
    {
        new Color(0f, 0f, 0f),          // Wave 1: Midnight Black
        new Color(0.1f, 0f, 0.15f),     // Wave 2: Cosmic Nebula Violet
        new Color(0f, 0.1f, 0.2f),      // Wave 3: Deep Space Abyss Teal
        new Color(0.2f, 0.05f, 0.05f)   // Wave 4: Supernova Warning Red
    };

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (gameplayCamera == null) gameplayCamera = Camera.main;
        ResetNormalWaveUI();
    }

    private void Update()
    {
        // Don't advance waves if the game state is already over or won
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameManager.GameState.Playing) return;

        HandleWaveTimeEngine();
    }

    private void HandleWaveTimeEngine()
    {
        waveTimer += Time.deltaTime;

        // Smoothly transition camera background color to current wave color index target properties 
        if (gameplayCamera != null)
        {
            int colorIndex = Mathf.Clamp(currentWave - 1, 0, waveColors.Length - 1);
            gameplayCamera.backgroundColor = Color.Lerp(gameplayCamera.backgroundColor, waveColors[colorIndex], Time.deltaTime * 0.5f);
        }

        // Fulfills Requirement: Evaluate if the active wave timeline has entered the final countdown margin zone
        float timeRemaining = waveDuration - waveTimer;
        if (currentWave < 4 && timeRemaining <= warningTriggerTime && !isWarningSequenceActive)
        {
            StartWarningSequence();
        }

        if (waveTimer >= waveDuration && currentWave < 4)
        {
            AdvanceToNextWave();
        }
    }

    // Custom method matching assignment guidelines
    public void AdvanceToNextWave()
    {
        StopWarningSequence();

        waveTimer = 0f;
        currentWave++;

        // Linearly step up speed multipliers as difficulty increments
        EnemySpeedMultiplier += 0.4f;
        AsteroidSpawnModifier += 0.5f;

        ResetNormalWaveUI();
        Debug.Log($"DifficultyManager: Advanced to Wave {currentWave}. Enemy Speed Multiplier: {EnemySpeedMultiplier}");
    }

    private void StartWarningSequence()
    {
        isWarningSequenceActive = true;
        // Launch dynamic text animation loop safely using C# threading engine pipelines
        flashingRoutineInstance = StartCoroutine(FlashWarningTextRoutine());
    }

    private void StopWarningSequence()
    {
        isWarningSequenceActive = false;
        if (flashingRoutineInstance != null)
        {
            StopCoroutine(flashingRoutineInstance);
            flashingRoutineInstance = null;
        }
    }

    /// <summary>
    /// Routine that loops back and forth changing subtext sizes and transparency alpha codes cleanly.
    /// </summary>
    private IEnumerator FlashWarningTextRoutine()
    {
        while (isWarningSequenceActive)
        {
            if (waveTextDisplay != null)
            {
                // Flashing state ON (Using TextMeshPro rich text color codes for bright yellow alert)
                waveTextDisplay.text = $"WAVE {currentWave}\n<size=60%><color=#FFDD00>⚠️ DENSITY INCREASING! ⚠️</color></size>";
            }
            yield return new WaitForSeconds(0.4f);

            if (waveTextDisplay != null)
            {
                // Flashing state OFF (Wipe subtext string out to create an absolute flashing visual blink rhythm)
                waveTextDisplay.text = $"WAVE {currentWave}\n<size=60%> </size>";
            }
            yield return new WaitForSeconds(0.3f);
        }
    }

    private void ResetNormalWaveUI()
    {
        if (waveTextDisplay == null) return;

        if (currentWave == 4)
        {
            waveTextDisplay.text = "WAVE 4\n<size=60%><color=#FF3333>⚠️ MAXIMUM DENSITY REACHED ⚠️</color></size>";
        }
        else
        {
            // Standard state text: Displays a calm status layout during the safe baseline timeline of the level
            waveTextDisplay.text = $"WAVE {currentWave}\n<size=60%>Sector Status: Stable</size>";
        }
    }
}
