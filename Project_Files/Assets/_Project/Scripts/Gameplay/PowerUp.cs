using UnityEngine;

public abstract class PowerUp : MonoBehaviour
{
    // Method to be implemented by subclasses
    public abstract void ActivatePowerUp(PlayerController player);

    private void OnTriggerEnter(Collider other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            ActivatePowerUp(player);
            Destroy(gameObject); // Remove the power-up from the scene after activation
        }
    }
}
