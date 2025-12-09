using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [Header("Inventory UI")]
    public GameObject inventoryPanel;
    public Transform slotParent;
    public GameObject slotPrefab;

    [Header("Drop Item Prefabs")]
    public GameObject itemPrefab;

    [Header("Rarity Border Objects (passed to slots)")]
    public GameObject[] rarityBorders; // Common, Uncommon, Epic, Legend, Mythic 순서

    public List<ItemData> inventory = new List<ItemData>();

    private Canvas uiCanvas;
    private List<SlotUI> slotUIs = new List<SlotUI>();

    public PlayerStats playerStats;

    private void Awake()
    {
        uiCanvas = slotParent.GetComponentInParent<Canvas>();

        // 슬롯 생성 및 초기화
        for (int i = 0; i < 16; i++)
        {
            inventory.Add(null);

            GameObject slot = Instantiate(slotPrefab, slotParent);
            SlotUI slotUI = slot.GetComponent<SlotUI>();
            if (slotUI != null)
            {
                // rarityBorders 배열 전달 (핵심)
                slotUI.rarityBorders = this.rarityBorders;

                // 초기화
                slotUI.Setup(null, i, this, uiCanvas);
                slotUIs.Add(slotUI);
            }
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
            ToggleInventory();
    }

    public void ToggleInventory()
    {
        if (inventoryPanel == null)
            return;

        bool isActive = inventoryPanel.activeSelf;
        inventoryPanel.SetActive(!isActive);

        if (!isActive)
            UpdateUI();
    }

    public bool AddItem(ItemData newItem)
    {
        // 1. 기존에 같은 타입이 있는지 검사
        for (int i = 0; i < inventory.Count; i++)
        {
            var existing = inventory[i];
            if (existing == null) continue;

            if (existing.type == newItem.type)
            {
                if (newItem.value > existing.value)
                {
                    inventory[i] = newItem;
                }
                // 교체든 폐기든 재계산
                playerStats.RecalculateStats(inventory);
                UpdateUI();
                return true;
            }
        }

        // 2. 빈칸 추가
        for (int i = 0; i < inventory.Count; i++)
        {
            if (inventory[i] == null)
            {
                inventory[i] = newItem;
                playerStats.RecalculateStats(inventory);
                UpdateUI();
                return true;
            }
        }

        return false;
    }

    public void UpdateUI()
    {
        for (int i = 0; i < slotUIs.Count; i++)
        {
            slotUIs[i].rarityBorders = this.rarityBorders;

            slotUIs[i].Setup(inventory[i], i, this, uiCanvas);
        }
    }

    public void SwapItems(int fromIndex, int toIndex)
    {
        if (fromIndex == toIndex)
            return;

        ItemData temp = inventory[toIndex];
        inventory[toIndex] = inventory[fromIndex];
        inventory[fromIndex] = temp;

        playerStats.RecalculateStats(inventory);
        UpdateUI();
    }


    public void MoveItem(int fromIndex, int toIndex)
    {
        if (fromIndex == toIndex)
            return;

        inventory[toIndex] = inventory[fromIndex];
        inventory[fromIndex] = null;

        CleanupDragState();
        playerStats.RecalculateStats(inventory);
        UpdateUI();
    }

    private void CleanupDragState()
    {
        if (SlotUI.draggingIcon != null)
        {
            Destroy(SlotUI.draggingIcon);
            SlotUI.draggingIcon = null;
        }

        SlotUI.draggingItem = null;
        SlotUI.fromSlot = null;
    }
}