using UnityEngine;

[CreateAssetMenu(menuName = "Item/New Item Data")]
public class ItemData : ScriptableObject
{
    public string id;
    public string itemName;
    public Sprite icon;
    public ItemType type;
    public ItemRarity rarity;
    public float value;
    public int price;

    [TextArea(2, 4)]
    public string description;

    public void UpdateRarityByValue()
    {
        if (value <= 42f)
            rarity = ItemRarity.Common;
        else if (value <= 72f)
            rarity = ItemRarity.Uncommon;
        else if (value <= 87f)
            rarity = ItemRarity.Epic;
        else if (value <= 97f)
            rarity = ItemRarity.Legendary;
        else
            rarity = ItemRarity.Mythic;
    }

    public string GetTooltipText()
    {
        int v = Mathf.FloorToInt(value);

        string effect = "";

        switch (type)
        {
            case ItemType.CriticalChance:
                effect = "크리티컬 확률 +" + v + "%";
                break;

            case ItemType.GoldGain:
                effect = "골드 획득 +" + v + "%";
                break;

            case ItemType.HealthRegen:
                effect = "체력 재생 +" + v / 10;
                break;

            case ItemType.Defense:
                effect = "방어력 +" + v;
                break;

            case ItemType.DropRate:
                effect = "드랍률 +" + v + "%";
                break;

            case ItemType.LifeSteal:
                effect = "피흡 +" + v / 10 + "%";
                break;

            case ItemType.CriticalDamage:
                effect = "크리티컬 데미지 +" + v + "%";
                break;

            case ItemType.AllStatBoost:
                effect = "모든 능력치 +" + v + "%";
                break;

            case ItemType.CriticalDamage_Attack:
                effect =
                    "크리티컬 데미지 +" + v + "%\n" +
                    "공격력 +" + v + "%";
                break;

            case ItemType.DropRate_ExpGain:
                effect =
                    "드랍률 +" + v + "%\n" +
                    "경험치 +" + v + "%";
                break;


        }

        string tooltip =
            itemName + "\n" +
            "(" + rarity.ToString() + ")\n" +
            effect + "\n\n" +
            description + "\n\n" +
            "가격 ： " + price;

        return tooltip;
    }

    public void InitializeGeneratedItem(float newValue, ItemData baseItem)
    {
        id = baseItem.id;
        itemName = baseItem.itemName;
        icon = baseItem.icon;
        type = baseItem.type;
        description = baseItem.description;

        value = newValue;
        UpdateRarityByValue();

        UpdatePrice();
    }

    public void UpdatePrice()
    {
        float rarityMultiplier = 1f;

        switch(rarity)
        {
            case ItemRarity.Common: rarityMultiplier = 1f; break;
            case ItemRarity.Uncommon: rarityMultiplier = 1.5f; break;
            case ItemRarity.Epic: rarityMultiplier = 2f; break;
            case ItemRarity.Legendary: rarityMultiplier = 3f; break;
            case ItemRarity.Mythic: rarityMultiplier = 5f; break;
        }

        price = Mathf.FloorToInt(value * rarityMultiplier);
    }

}