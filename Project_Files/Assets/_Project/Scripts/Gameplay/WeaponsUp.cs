using UnityEngine;

public class WeaponsUp : PowerUp
{
    public override void ActivatePowerUp(PlayerController player)
    {
        WeaponSystem weaponSystem = player.GetComponent<WeaponSystem>();

        if (weaponSystem != null)
        {
            Debug.Log("PowerUp: Permanent weapon upgrade tier tier advancement triggered.");
            weaponSystem.TriggerWeaponUpgrade();
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound("pickup");
        }
    }
}
