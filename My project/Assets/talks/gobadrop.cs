using UnityEngine;

public class gobadrop : MonoBehaviour
{
    public KeyCode interactKey = KeyCode.F;
    public GameObject flowerObject;

    void Start()
    {
        HideFlower();
    }

    void Update()
    {
        if (Input.GetKeyDown(interactKey) && IsPlayerCollidingWithFlower())
        {
            ShowFlower();
        }
    }

    private bool IsPlayerCollidingWithFlower()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 1f);

        foreach (Collider2D collider in colliders)
        {
            if (collider.CompareTag("Respawn"))
            {
                return true;
            }
        }

        return false;
    }

    void ShowFlower()
    {
        flowerObject.SetActive(true);
    }

    void HideFlower()
    {
        flowerObject.SetActive(false);
    }
}
