using UnityEngine;

public static class DropSystem
{
    public static void TryDrop(ItemData[] possibleDrops, float baseDropChance, Vector3 position, PlayerInventory playerinventory, GameObject itemPrefab)
    {
        if (possibleDrops == null || possibleDrops.Length == 0)
        {
            Debug.LogWarning("drop system possibleDrops 배열이 비어 있습니다.");
                return;
        }

        PlayerStats stats = playerinventory.GetComponent<PlayerStats>();
        float playerDropBonus = (stats != null) ? stats.dropRate : 0f;

        float finalChance = Mathf.Clamp01((baseDropChance + playerDropBonus) / 100f);

        if (Random.value > finalChance)
            return;

        if(itemPrefab == null)
        {
            Debug.LogError("drop system itemdropprefab을 찾을 수 없습니다.");
            return;
        }

        GameObject go = Object.Instantiate(itemPrefab, position + Vector3.up, Quaternion.identity);

        ItemData baseItem = possibleDrops[Random.Range(0, possibleDrops.Length)];
        float randomValue = Random.Range(1f, 100f);

        ItemData dropItem = ScriptableObject.CreateInstance<ItemData>();
        dropItem.InitializeGeneratedItem(randomValue, baseItem);

        ItemDrop drop = go.GetComponent<ItemDrop>();
        if(drop != null)
            drop.Initialize(dropItem);

        ItemPickup pickup = go.GetComponent<ItemPickup>();
        if(pickup != null)
        {
            pickup.item = dropItem;
            pickup.inventory = playerinventory;
        }

        Debug.Log($"[드랍] {dropItem.itemName} 생성됨 (Value={dropItem.value:F1}, Rarity={dropItem.rarity}) | {finalChance * 100f:F1}% 확률");
    }
}
