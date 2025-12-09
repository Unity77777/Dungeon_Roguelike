using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SlotUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    public int slotIndex;
    public ItemData item;
    public Image iconImage;

    private PlayerInventory inventory;
    private Canvas uiCanvas;
    private CanvasGroup canvasGroup;

    public static GameObject draggingIcon;
    public static ItemData draggingItem;
    public static SlotUI fromSlot;

    [Header("Rarity Border Objects")]
    public GameObject[] rarityBorders;

    private GameObject currentBorder;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        if (iconImage == null)
            iconImage = GetComponentInChildren<Image>();

        if (iconImage != null)
            iconImage.enabled = true;
    }

    public void Setup(ItemData newItem, int index, PlayerInventory playerInventory, Canvas canvas)
    {
        slotIndex = index;
        inventory = playerInventory;
        uiCanvas = canvas;
        item = newItem;

        if (item != null && item.icon != null)
        {
            iconImage.sprite = item.icon;
            iconImage.color = Color.white;
        }
        else
        {
            iconImage.sprite = null;
            iconImage.color = new Color(1, 1, 1, 0);
        }

        if (iconImage != null)
            iconImage.rectTransform.localScale = Vector3.one * 0.75f;

        if (currentBorder != null)
            Destroy(currentBorder);

        if (item != null && rarityBorders != null && rarityBorders.Length > 0)
        {
            int rarityIndex = (int)item.rarity;
            if (rarityIndex >= 0 && rarityIndex < rarityBorders.Length && rarityBorders[rarityIndex] != null)
            {
                currentBorder = Instantiate(rarityBorders[rarityIndex], iconImage.transform);
                currentBorder.transform.SetAsFirstSibling();
            }
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // 왼쪽 버튼이 아니면 드래그 시작하지 않음
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        if (item == null || item.icon == null) return;

        fromSlot = this;
        draggingItem = item;

        draggingIcon = new GameObject("Dragging Icon");
        draggingIcon.transform.SetParent(uiCanvas.transform, false);

        Image img = draggingIcon.AddComponent<Image>();
        img.sprite = item.icon;
        img.raycastTarget = false;

        RectTransform rt = draggingIcon.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(60, 60);
        rt.localScale = Vector3.one * 0.75f;

        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        // 왼쪽 버튼만 허용
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        if (draggingIcon != null)
            draggingIcon.transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // 왼쪽 버튼만 허용
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        if (draggingIcon != null)
            Destroy(draggingIcon);

        draggingIcon = null;
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (fromSlot == null || fromSlot.item == null || inventory == null) return;

        int fromIndex = fromSlot.slotIndex;
        int toIndex = this.slotIndex;

        if (inventory.inventory[toIndex] == null)
            inventory.MoveItem(fromIndex, toIndex);
        else
            inventory.SwapItems(fromIndex, toIndex);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (item == null)
            return;

        string info = item.GetTooltipText();
        TooltipManager.Show(info);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        TooltipManager.Hide();
    }
}