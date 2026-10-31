using System.Collections;
using UnityEngine;

public class ShieldUp : PowerUp
{
    [Header("Shield Power-Up Tuning")]
    [SerializeField] private float shieldBoostAmount = 50.0f;
    [SerializeField] private float duration = 8.0f;

    public override void ActivatePowerUp(PlayerController player)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound("pickup");
        }

        // 1. Apply changes INSTANTLY on frame 0 to prevent instant death
        Debug.Log("PowerUp: Shield boost activated synchronously.");
        player.Shield += shieldBoostAmount;
        player.SetInvulnerable(true);

        // 2. Start the coroutine ONLY to handle the expiration timer
        player.StartCoroutine(ApplyShieldBoost(player));
    }

    private IEnumerator ApplyShieldBoost(PlayerController player)
    {
        // Wait for the power-up duration to run out
        yield return new WaitForSeconds(duration);

        // Revert stats after duration has passed
        player.Shield -= shieldBoostAmount;
        player.SetInvulnerable(false);
        Debug.Log("PowerUp: Shield boost expired. Shield reverted and vulnerability restored.");
    }
}
