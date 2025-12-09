using TMPro;
using UnityEngine;

public class PurchaseUI : MonoBehaviour
{
    public GameObject panel;
    public TextMeshProUGUI itemNameText;

    private ItemData currentItem;
    private ShopManager shopManager;

    public void Init(ShopManager manager)
    {
        shopManager = manager;
    }

    public void Open(ItemData item)
    {
        currentItem = item;

        if (item != null)
            itemNameText.text = item.itemName + "을 구매하시겠습니까?";

        panel.SetActive(true);
    }

    public void Close()
    {
        panel.SetActive(false);
    }

    public void OnClickYes()
    {
        if (shopManager != null && currentItem != null)
            shopManager.TryBuyItem(currentItem);

        Close();
    }

    public void OnClickNo()
    {
        Close();
    }
}