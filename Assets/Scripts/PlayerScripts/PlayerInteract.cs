using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    private bool canOpenShop = false;

    void Update()
    {
        if (canOpenShop && Input.GetKeyDown(KeyCode.E))
        {
            ShopManager.Instance.ToggleShop();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<ShopNpc>() != null)
        {
            canOpenShop = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<ShopNpc>() != null)
        {
            canOpenShop = false;
        }
    }
}