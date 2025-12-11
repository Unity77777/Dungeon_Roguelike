using UnityEngine;

public class MonsterDropper : MonoBehaviour
{
    public ItemData[] possibleDrops;
    public float dropChance = 0.5f;
    public GameObject itemPrefab;
    public Sprite dropIcon;

    public void Drop(PlayerInventory inventory, Vector3 postion)
    {
        DropSystem.TryDrop(possibleDrops, dropChance, postion, inventory, itemPrefab);
    }
}
