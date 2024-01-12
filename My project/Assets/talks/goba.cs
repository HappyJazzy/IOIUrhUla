using UnityEngine;

public class goba : MonoBehaviour
{
    public KeyCode interactKey = KeyCode.P;

    void Update()
    {
        if (Input.GetKeyDown(interactKey))
        {
            TryInteractWithMushroom();
        }
    }

    void TryInteractWithMushroom()
    {
        // Check if the player is colliding with any objects
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 1f);

        foreach (Collider2D collider in colliders)
        {
            // Check if the collider belongs to a game object with the "Mushroom" tag
            if (collider.CompareTag("Mushroom"))
            {
                // Mushroom found, perform interaction (e.g., make it disappear)
                Destroy(collider.gameObject);
                break; // Exit the loop after interacting with the first mushroom found
            }
        }
    }
}
