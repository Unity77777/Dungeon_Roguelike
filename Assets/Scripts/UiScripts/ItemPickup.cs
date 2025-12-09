using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ItemPickup : MonoBehaviour
{
    [Header("Item Data")]
    public ItemData item;
    public PlayerInventory inventory;

    [Header("Pickup Settings")]
    public KeyCode pickupKey = KeyCode.E;   // 눌러야 하는 키
    public float destroyDelay = 0.05f;      // 아이템 삭제 지연 (VFX용)

    private bool canPickUp = false;  // 플레이어가 근처에 있는가
    private bool pickedUp = false;   // 이미 먹었는가

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            inventory = other.GetComponent<PlayerInventory>();
            canPickUp = true;
            Debug.Log("[ItemPickup] 플레이어가 근처에 접근함");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            canPickUp = false;
            Debug.Log("[ItemPickup] 플레이어가 범위를 벗어남");
        }
    }

    private void Update()
    {
        // 플레이어가 근처에 있고 아직 안 먹었으며, 지정한 키를 눌렀을 때만 줍기
        if (canPickUp && !pickedUp && Input.GetKeyDown(pickupKey))
        {
            TryPickUp();
        }
    }

    public void TryPickUp()
    {
        if (inventory == null || item == null)
        {
            Debug.LogWarning("[ItemPickup] inventory 또는 item이 null입니다.");
            return;
        }

        bool added = inventory.AddItem(item);
        if (added)
        {
            pickedUp = true;
            Debug.Log($"[ItemPickup] {item.itemName} 획득 완료");

            // 약간의 지연 후 파괴 (효과음/VFX 표시 여유)
            Destroy(gameObject, destroyDelay);
        }
    }
}