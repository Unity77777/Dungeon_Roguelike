using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ShopSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public ItemData item;
    public Image iconImage;

    // 등급별 테두리 프리팹 (Common, Uncommon, Epic, Legendary, Mythic 순)
    public GameObject[] rarityBorderPrefabs;

    private GameObject currentBorder;
    private ShopManager shopManager;

    public void Setup(ItemData newItem, ShopManager manager)
    {
        item = newItem;
        shopManager = manager;

        // 기존 테두리 제거
        if (currentBorder != null)
        {
            Destroy(currentBorder);
            currentBorder = null;
        }

        if (item != null)
        {
            iconImage.enabled = true;
            iconImage.sprite = item.icon;

            CreateBorder(item.rarity);
        }
        else
        {
            iconImage.enabled = false;
            iconImage.sprite = null;
        }
    }

    private void CreateBorder(ItemRarity rarity)
    {
        int index = (int)rarity;
        if (index < 0 || index >= rarityBorderPrefabs.Length)
            return;

        GameObject prefab = rarityBorderPrefabs[index];
        if (prefab == null)
            return;

        currentBorder = Instantiate(prefab, transform);

        RectTransform rt = currentBorder.GetComponent<RectTransform>();
        RectTransform slotRT = GetComponent<RectTransform>(); // 슬롯 전체 RectTransform

        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = slotRT.rect.size;
        rt.localScale = Vector3.one;

        // border를 아이콘보다 위로 올리기
        currentBorder.transform.SetSiblingIndex(iconImage.transform.GetSiblingIndex() + 1);
    }

    public void OnClickBuy()
    {
        if (item != null)
            shopManager.TryBuyItem(item);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (item == null) return;
        TooltipManager.Show(item.GetTooltipText());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        TooltipManager.Hide();
    }

    public void OnClickOpenPurchase()
    {
        if (item == null)
            return;
        Debug.Log("클릭한 슬롯의 아이템: " + item.itemName);
        shopManager.OpenPurchaseUI(item);
    }
}