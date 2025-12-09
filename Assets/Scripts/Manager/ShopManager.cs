using System.Collections.Generic;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance;

    public ShopSlot[] slots;
    public ItemData[] itemsForSale;

    public PlayerInventory playerInventory;
    public PlayerStats playerStats;
    public GameObject shopPanel;

    public PurchaseUI purchaseUI;

    public ItemData rerollItem;

    public int rerollPrice = 50;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (purchaseUI != null)
            purchaseUI.Init(this);

        SetupShop();
        shopPanel.SetActive(false);
    }

    private void SetupShop()
    {
        int slotCount = slots.Length;

        int normalItemCount = Random.Range(1, 4);

        if (normalItemCount >= slotCount)
            normalItemCount = slotCount - 1;

        List<ItemData> randomList = new List<ItemData>();

        for (int i = 0; i < normalItemCount; i++)
        {
            int index = Random.Range(0, itemsForSale.Length);
            ItemData baseItem = itemsForSale[index];

            float randomValue = Random.Range(1f, 100f);

            ItemData clone = ScriptableObject.CreateInstance<ItemData>();
            clone.id = baseItem.id;
            clone.itemName = baseItem.itemName;
            clone.icon = baseItem.icon;
            clone.type = baseItem.type;
            clone.description = baseItem.description;

            clone.value = randomValue;
            clone.UpdateRarityByValue();
            clone.price = GetPrice(clone.rarity, clone.value);

            randomList.Add(clone);
        }

        for (int i = 0; i < normalItemCount; i++)
        {
            slots[i].Setup(randomList[i], this);
        }

        int rerollIndex = normalItemCount;

        if (rerollIndex < slotCount)
        {
            rerollItem.price = rerollPrice;
            slots[rerollIndex].Setup(rerollItem, this);
        }

        for (int i = rerollIndex + 1; i < slotCount; i++)
        {
            slots[i].Setup(null, this);
        }
    }

    public void TryBuyItem(ItemData item)
    {
        if (item == null)
            return;

        if (item.type == ItemType.RerollShop)
        {
            if (playerStats.TrySpendGold(item.price))
            {
                rerollPrice *= 2;
                SetupShop();
            }
            else
            {
                Debug.Log("골드가 부족하여 리롤을 할 수 없습니다.");
            }
            return;
        }

        int cost = item.price;

        if (playerStats.TrySpendGold(cost))
        {
            playerInventory.AddItem(item);
            RemoveItemFromShop(item);
        }
        else
        {
            Debug.Log("골드가 부족하여 구매할 수 없습니다.");
        }
    }

    private void RemoveItemFromShop(ItemData item)
    {
        foreach (var slot in slots)
        {
            if (slot.item == item)
            {
                slot.Setup(null, this);
                break;
            }
        }
    }

    public void ToggleShop()
    {
        if (shopPanel.activeSelf)
            CloseShop();
        else
            OpenShop();
    }

    public void OpenShop()
    {
        shopPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void CloseShop()
    {
        shopPanel.SetActive(false);
        Time.timeScale = 1f;
    }

    public void OpenPurchaseUI(ItemData item)
    {
        if (item == null)
            return;

        purchaseUI.Open(item);
    }

    public int GetPrice(ItemRarity rarity, float value)
    {
        float min = 1f;
        float max = 1f;

        switch (rarity)
        {
            case ItemRarity.Common:
                min = 1f; max = 6f; break;
            case ItemRarity.Uncommon:
                min = 8f; max = 13f; break;
            case ItemRarity.Epic:
                min = 15f; max = 20f; break;
            case ItemRarity.Legendary:
                min = 20f; max = 30f; break;
            case ItemRarity.Mythic:
                min = 30f; max = 45f; break;
        }

        float p = value * Random.Range(min, max);
        return Mathf.RoundToInt(p);
    }
}