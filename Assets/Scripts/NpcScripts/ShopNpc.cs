using UnityEngine;

public class ShopNpc : MonoBehaviour
{
    public float interactDistance = 2f;
    private Transform player;

    private void Start()
    {
        GameObject p = GameObject.FindWithTag("Player");
        player = p?.transform;
    }

    private void Update()
    {
        if (player == null)
            return;

        float distance = Vector3.Distance(player.position, transform.position);

        if (distance <= interactDistance)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                ShopManager.Instance.ToggleShop();
            }
        }
    }
}