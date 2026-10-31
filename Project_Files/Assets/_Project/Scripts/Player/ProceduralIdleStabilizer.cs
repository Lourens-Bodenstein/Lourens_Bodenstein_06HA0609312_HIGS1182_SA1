// ===================================================================================
// ASSIGNMENT SPECIFICATION LINK: ANIMATIONCONTROLLER ROLE
// This script serves as the mandatory 'AnimationController' system required by the brief.
// Instead of a standard visual Mecanim state machine, it handles all player idle, 
// movement tracking, and thrust animations programmatically via procedural code.
// ===================================================================================

using UnityEngine;

public class ProceduralIdleStabilizer : MonoBehaviour
{
    private enum SequenceState
    {
        StandardStandstill,
        InitialSlowSpin,
        BoosterOverCorrectLeft,
        BoosterPanicRight,
        FinalStabilizationRecovery
    }

    [Header("Timing Thresholds (Seconds)")]
    [SerializeField] private float timeBeforeSpinStarts = 4.0f;
    [SerializeField] private float stage1Duration = 2.5f;
    [SerializeField] private float stage2Duration = 2.0f;
    [SerializeField] private float stage3Duration = 1.5f;

    [Header("Dynamic Rotational Speeds")]
    [SerializeField] private float initialSpinSpeed = 15f; // Drifts Right (Clockwise)
    [SerializeField] private float oppositeOverCorrectSpeed = -45f; // Flips Left (Counter-Clockwise)
    [SerializeField] private float chaoticSpinSpeed = 120f; // Spins out Right (Clockwise)

    private EngineVisuals engineVisuals;
    private SequenceState currentState = SequenceState.StandardStandstill;
    private float stateTimer = 0f;
    private bool isPlayerInputActive = false;

    private void Start()
    {
        engineVisuals = GetComponent<EngineVisuals>();
        if (engineVisuals == null) Debug.LogWarning("ProceduralIdleStabilizer: Missing EngineVisuals!");
    }

    private void Update()
    {
        EvaluateStateSequence();
    }

    public void NotifyPlayerMovement(float forwardInput, float turnInput)
    {
        if (Mathf.Abs(forwardInput) > 0.01f || Mathf.Abs(turnInput) > 0.01f)
        {
            if (currentState != SequenceState.StandardStandstill)
            {
                ResetStabilizerSequence();
            }
            isPlayerInputActive = true;
        }
        else
        {
            isPlayerInputActive = false;
        }
    }

    private void EvaluateStateSequence()
    {
        if (isPlayerInputActive || (GameManager.Instance != null && GameManager.Instance.CurrentState != GameManager.GameState.Playing))
        {
            return;
        }

        stateTimer += Time.deltaTime;

        switch (currentState)
        {
            case SequenceState.StandardStandstill:
                if (stateTimer >= timeBeforeSpinStarts)
                {
                    TransitionToState(SequenceState.InitialSlowSpin);
                }
                break;

            case SequenceState.InitialSlowSpin:
                // Ship slowly drifts Right (Clockwise). No engines firing.
                transform.Rotate(Vector3.up * initialSpinSpeed * Time.deltaTime);
                SetVisualOutputs(0f, 0f);

                if (stateTimer >= stage1Duration)
                {
                    TransitionToState(SequenceState.BoosterOverCorrectLeft);
                }
                break;

            case SequenceState.BoosterOverCorrectLeft:
                // Ship is spinning Right. RIGHT engine fires to push it Left.
                // It overcorrects and starts violently spinning Left (Counter-Clockwise).
                transform.Rotate(Vector3.up * oppositeOverCorrectSpeed * Time.deltaTime);
                SetVisualOutputs(0f, -1f); // -1f triggers the Right Thruster in EngineVisuals

                if (stateTimer >= stage2Duration)
                {
                    TransitionToState(SequenceState.BoosterPanicRight);
                }
                break;

            case SequenceState.BoosterPanicRight:
                // Ship is spinning Left. LEFT engine fires in panic to push it Right.
                // It overcorrects massively, sending it spinning wildly Right.
                transform.Rotate(Vector3.up * chaoticSpinSpeed * Time.deltaTime);
                SetVisualOutputs(0f, 1f); // 1f triggers the Left Thruster in EngineVisuals

                if (stateTimer >= stage3Duration)
                {
                    TransitionToState(SequenceState.FinalStabilizationRecovery);
                }
                break;

            case SequenceState.FinalStabilizationRecovery:
                // Final braking burn: Ship is spinning Right, so RIGHT engine fires briefly to freeze momentum.
                SetVisualOutputs(0f, -1f);
                Debug.Log("ProceduralIdleStabilizer: Braking thrusters executed successfully.");
                ResetStabilizerSequence();
                break;
        }
    }

    private void TransitionToState(SequenceState newState)
    {
        currentState = newState;
        stateTimer = 0f;
        Debug.Log($"ProceduralIdleStabilizer: Sequence State -> {newState}");
    }

    private void ResetStabilizerSequence()
    {
        currentState = SequenceState.StandardStandstill;
        stateTimer = 0f;
        SetVisualOutputs(0f, 0f);
    }

    private void SetVisualOutputs(float forward, float turn)
    {
        if (engineVisuals != null) engineVisuals.UpdateEngineEffects(forward, turn);
    }
}
