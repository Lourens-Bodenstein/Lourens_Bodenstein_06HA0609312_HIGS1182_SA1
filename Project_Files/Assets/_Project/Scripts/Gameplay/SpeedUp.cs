using System.Collections;
using UnityEngine;

public class SpeedUp : PowerUp
{
    [Header("PowerUp Adjustments")]
    [SerializeField] private float speedBoostMultiplier = 2.0f;
    [SerializeField] private float duration = 5.0f;

    public override void ActivatePowerUp(PlayerController player)
    {
        player.StartCoroutine(ApplySpeedBoost(player));

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound("pickup");
        }
    }

    private IEnumerator ApplySpeedBoost(PlayerController player)
    {
        Debug.Log("PowerUp: Speed boost activated.");
        player.Speed = player.BaseSpeed * speedBoostMultiplier;

        yield return new WaitForSeconds(duration);

        player.Speed = player.BaseSpeed;
        Debug.Log("PowerUp: Speed boost expired. Speed reverted.");
    }
}
