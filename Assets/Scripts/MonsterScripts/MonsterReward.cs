using UnityEngine;

public class MonsterReward : MonoBehaviour
{
    public int baseExp = 150;
    public int minGold = 1;
    public int maxGold = 100;

    public void GiveReward(PlayerInventory inventory)
    {
        if (inventory == null) return;

        PlayerExperience exp = inventory.GetComponent<PlayerExperience>();
        PlayerStats stats = inventory.GetComponent<PlayerStats>();

        if (exp != null)
        {
            exp.GainExp(baseExp);
        }

        if (stats != null)
        {
            int baseGold = Random.Range(minGold, maxGold + 1);
            stats.AddGold(baseGold);
        }
    }
}