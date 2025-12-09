using UnityEngine;
using UnityEngine.VFX;

[RequireComponent(typeof(Collider))]
public class ItemDrop : MonoBehaviour
{
    private ItemData itemData;
    public SpriteRenderer iconRenderer;
    public GameObject glowObject;
    private VisualEffect vfx;

    [Header("Prefab Variants")]
    public GameObject commonBeam;
    public GameObject uncommonBeam;
    public GameObject epicBeam;
    public GameObject legendaryBeam;
    public GameObject mythicBeam;

    private bool playerInRange = false;
    private PlayerInventory playerInventory;

    void Awake()
    {
        if (iconRenderer == null)
            iconRenderer = GetComponentInChildren<SpriteRenderer>();
        if (glowObject != null)
            vfx = glowObject.GetComponentInChildren<VisualEffect>();
    }

    public void Initialize(ItemData data)
    {
        itemData = data;
        if (iconRenderer == null)
            iconRenderer = GetComponentInChildren<SpriteRenderer>();
        if (data.icon != null && iconRenderer != null)
            iconRenderer.sprite = data.icon;

        ReplaceBeamByRarity(itemData.rarity);
    }

    private void ReplaceBeamByRarity(ItemRarity rarity)
    {
        if (glowObject != null)
            Destroy(glowObject);

        GameObject prefabToUse = null;
        switch (rarity)
        {
            case ItemRarity.Common: prefabToUse = commonBeam; break;
            case ItemRarity.Uncommon: prefabToUse = uncommonBeam; break;
            case ItemRarity.Epic: prefabToUse = epicBeam; break;
            case ItemRarity.Legendary: prefabToUse = legendaryBeam; break;
            case ItemRarity.Mythic: prefabToUse = mythicBeam; break;
        }

        if (prefabToUse != null)
        {
            glowObject = Instantiate(prefabToUse, transform);
            vfx = glowObject.GetComponentInChildren<VisualEffect>();
        }
        else
        {
            Debug.LogWarning($"[ItemDrop] {rarity} 등급의 Beam 프리팹이 지정되지 않았습니다.");
        }
    }

    private void Update()
    {
        // E키 눌렀을 때 아이템 줍기
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            if (playerInventory != null)
            {
                playerInventory.AddItem(itemData);
                Destroy(gameObject);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // CharacterController로 인한 중복 감지 방지
        if (other.GetComponent<CharacterController>() != null)
            return;

        if (other.CompareTag("Player"))
        {
            playerInventory = other.GetComponent<PlayerInventory>();
            playerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            playerInventory = null;
        }
    }
}