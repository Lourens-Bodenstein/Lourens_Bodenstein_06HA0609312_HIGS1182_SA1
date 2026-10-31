using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform playerTarget;
    [SerializeField] private Vector3 offset = new Vector3(0f, 15f, -10f);

    private void LateUpdate()
    {
        // Dynamic search hook: if the target isn't assigned yet, look for it using the Player tag
        if (playerTarget == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerTarget = playerObj.transform;
                Debug.Log("CameraFollow: Player target located and cached successfully.");
            }
            else
            {
                // Return early if player hasn't spawned yet to prevent errors
                return;
            }
        }

        // Execute precise tracking calculation frame-rate independently
        ExecuteCameraTracking();
    }

    // Required single-responsibility custom method to manage positioning
    public void ExecuteCameraTracking()
    {
        transform.position = playerTarget.position + offset;
        transform.LookAt(playerTarget.position);
    }
}
