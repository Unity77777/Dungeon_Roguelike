using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    // 기존 스탯
    public float attack = 10f;
    public float attackSpeed = 1f;
    public float hp = 100f;
    public float moveSpeed = 5f;
    public float expGain = 1f;
    public float gold = 0f;

    // 새로 추가할 아이템 기반 스탯
    public float goldGain = 0f;         // 골드 획득량 증가 (%)
    public float criticalChance = 0f;   // 크리티컬 확률 (%)
    public float criticalDamage = 0f;   // 크리티컬 데미지 배율 (%)
    public float healthRegen = 0f;      // 초당 체력 회복량
    public float defense = 0f;          // 방어력 (% or 절대값)
    public float dropRate = 0f;         // 드랍률 증가 (%)
    public float lifeSteal = 0f;        // 공격 시 피흡 (%)

    private PlayerHealth playerHealth;

    public bool TrySpendGold(float amount)
    {
        if(gold >= amount)
        {
            gold -= amount;
            return true;
        }
        return false;
    }
       
    private void Awake()
    {
        playerHealth = GetComponent<PlayerHealth>();
    }

    // 능력치 적용 메서드
    public void ApplyStatUpgrade(string stat, float percent)
    {
        switch (stat)
        {
            case "Attack":
                attack *= (1 + percent / 100f);
                break;

            case "AttackSpeed":
                attackSpeed *= (1 + percent / 100f);
                break;

            case "Hp":
                float oldHp = hp; // 증가 전 최대 체력 저장
                hp *= (1 + percent / 100f); // 최대 체력 증가

                // PlayerHealth에 체력 비례 회복 반영 요청
                if (playerHealth != null)
                    playerHealth.OnMaxHpIncreased(oldHp);
                break;

            case "MoveSpeed":
                moveSpeed *= (1 + percent / 100f);
                break;

            case "Exp":
                expGain *= (1 + percent / 100f);
                break;

            case "AllStat":
                float prevHp = hp; // 증가 전 최대 체력 저장
                attack *= (1 + percent / 100f);
                attackSpeed *= (1 + percent / 100f);
                hp *= (1 + percent / 100f);
                moveSpeed *= (1 + percent / 100f);
                expGain *= (1 + percent / 100f);

                // HP가 변했으므로 PlayerHealth에 반영
                if (playerHealth != null)
                    playerHealth.OnMaxHpIncreased(prevHp);
                break;

            default:
                Debug.LogWarning($"Unknown stat type: {stat}");
                break;
        }

        Debug.Log($"[스탯 적용 완료] {stat} +{percent}%");
    }
    public void ResetStats()
    {
        // 아이템 효과는 덮어씌워지는 구조이므로 초기화 필요
        criticalChance = 0f;
        criticalDamage = 0f;
        goldGain = 0f;
        healthRegen = 0f;
        defense = 0f;
        dropRate = 0f;
        lifeSteal = 0f;
        expGain = 0f;
    }

    public void RecalculateStats(List<ItemData> inventory)
    {
        ResetStats();

        Dictionary<ItemType, float> highestValues = new Dictionary<ItemType, float>();

        foreach (var item in inventory)
        {
            if (item == null) continue;

            if (!highestValues.ContainsKey(item.type) || item.value > highestValues[item.type])
            {
                highestValues[item.type] = item.value;
            }
        }

        foreach (var kvp in highestValues)
        {
            float v = kvp.Value;

            switch (kvp.Key)
            {
                case ItemType.CriticalChance:
                    criticalChance = v;
                    break;

                case ItemType.CriticalDamage:
                    criticalDamage = v;
                    break;

                case ItemType.Defense:
                    defense = v;
                    break;

                case ItemType.HealthRegen:
                    healthRegen = v / 10f;
                    break;

                case ItemType.LifeSteal:
                    lifeSteal = v / 10f;
                    break;

                case ItemType.GoldGain:
                    goldGain = v;
                    break;

                case ItemType.DropRate:
                    dropRate = v;
                    break;

                case ItemType.ExpGain:
                    expGain = (1 + v / 100);
                    break;

                case ItemType.AllStatBoost:
                    criticalChance += v;
                    criticalDamage += v;
                    defense += v;
                    healthRegen += v / 10f;
                    lifeSteal += v / 10f;
                    goldGain += v;
                    dropRate += v;
                    expGain += (1 + v / 100f);
                    break;

                case ItemType.CriticalDamage_Attack:
                    criticalDamage += v;
                    attack += v;
                    break;

                case ItemType.DropRate_ExpGain:
                    dropRate += v;
                    expGain += (1 + v / 100f);
                    break;
            }
        }
    }
}