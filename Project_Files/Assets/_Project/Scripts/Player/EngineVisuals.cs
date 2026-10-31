using UnityEngine;

public class EngineVisuals : MonoBehaviour
{
    [Header("Particle System Dependencies")]
    [SerializeField] private ParticleSystem leftEngineParticles;
    [SerializeField] private ParticleSystem rightEngineParticles;

    private void Start()
    {
        // Guard clauses to verify safety initialization states
        if (leftEngineParticles == null) Debug.LogWarning("EngineVisuals: Left engine particle reference unassigned!");
        if (rightEngineParticles == null) Debug.LogWarning("EngineVisuals: Right engine particle reference unassigned!");

        // Default both engines to off at launch
        SetEngineState(leftEngineParticles, false);
        SetEngineState(rightEngineParticles, false);
    }

    /// <summary>
    /// Custom OOP state controller that updates the particle systems based on movement data.
    /// </summary>
    public void UpdateEngineEffects(float forwardInput, float turnInput)
    {
        // Turning right requires Left Engine thrust. Turning left requires Right Engine thrust.
        bool activateLeftEngine = (forwardInput > 0.01f) || (turnInput > 0.01f);
        bool activateRightEngine = (forwardInput > 0.01f) || (turnInput < -0.01f);

        SetEngineState(leftEngineParticles, activateLeftEngine);
        SetEngineState(rightEngineParticles, activateRightEngine);
    }

    // Encapsulated worker method to safely play or halt particles
    private void SetEngineState(ParticleSystem engine, bool shouldExhaust)
    {
        if (engine == null) return;

        var emission = engine.emission;

        if (shouldExhaust && !emission.enabled)
        {
            emission.enabled = true;
            if (!engine.isPlaying) engine.Play();
        }
        else if (!shouldExhaust && emission.enabled)
        {
            emission.enabled = false;
        }
    }
}
