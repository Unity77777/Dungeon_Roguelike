using UnityEngine;

public class DropManager : MonoBehaviour
{
    [Header("Prefab Variants")]
    public GameObject itemDropPrefab;

    [Header("Probability by Rarity (%)")]
    public float commonRate = 50f;
    public float uncommonRate = 25f;
    public float epicRate = 13f;
    public float legendaryRate = 7f;
    public float mythicRate = 5f;

    public void DropRandomItem(Vector3 position)
    {
        ItemData item = GenerateRandomItem();
        var obj = Instantiate(itemDropPrefab, position, Quaternion.identity);
        var drop = obj.GetComponent<ItemDrop>();
    }

    private ItemData GenerateRandomItem()
    {
        // 1. 아이템 타입 랜덤
        ItemType randomType = (ItemType)Random.Range(0, 7);

        // 2. 등급 랜덤
        ItemRarity rarity = GetRandomRarity();

        // 3. 값 설정 (1~90 랜덤 + 등급별 배수 적용)
        float baseValue = Random.Range(1f, 100f);
        switch (rarity)
        {
            case ItemRarity.Uncommon: baseValue *= 1.2f; break;
            case ItemRarity.Epic: baseValue *= 1.5f; break;
            case ItemRarity.Legendary: baseValue *= 2f; break;
            case ItemRarity.Mythic: baseValue *= 3f; break;
        }

        return new ItemData
        {
            id = randomType.ToString(),
            name = randomType.ToString(),
            type = randomType,
            rarity = rarity,
            value = baseValue
        };
    }

    private ItemRarity GetRandomRarity()
    {
        float roll = Random.Range(0f, 100f);

        if (roll < commonRate)
            return ItemRarity.Common;
        else if (roll < commonRate + uncommonRate)
            return ItemRarity.Uncommon;
        else if (roll < commonRate + uncommonRate + epicRate)
            return ItemRarity.Epic;
        else if (roll < commonRate + uncommonRate + epicRate + legendaryRate)
            return ItemRarity.Legendary;
        else
            return ItemRarity.Mythic;
    }
}