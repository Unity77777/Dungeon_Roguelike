using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    // 전투 스탯
    public Stat attack;           // 공격력
    public Stat attackSpeed;      // 공격 속도 (공격 간격 감소)
    public Stat maxHp;            // 최대 체력
    public Stat moveSpeed;        // 이동 속도

    // 확률 보조 스탯
    public Stat criticalChance;   // 크리티컬 확률 (%)
    public Stat criticalDamage;   // 크리티컬 데미지 배율 (%)
    public Stat defense;          // 방어력 (% or 절대값)
    public Stat dropRate;         // 드랍률 증가 (%)
    public Stat goldGain;         // 골드 획득량 증가 (%)
    public Stat expGain;          // 경험치 획득량 배율 (%)
    public Stat healthRegen;      // 초당 체력 회복량
    public Stat lifeSteal;        // 공격 시 피흡 (%)

    // 누적 자원
    public float gold;

    private PlayerHealth playerHealth;

    private void Awake()
    {
        playerHealth = GetComponent<PlayerHealth>();

        attack.baseValue = 10f;
        attackSpeed.baseValue = 1f;
        maxHp.baseValue = 100f;
        moveSpeed.baseValue = 5f;
        expGain.baseValue = 1f;

        criticalDamage.baseValue = 1.5f; // 기본 크리티컬 데미지 배율 150%
    }

    public bool TrySpendGold(float amount)
    {
        if(gold < amount)
            return false;

        gold -= amount;
        return true;
    }
       
    public void ResetStatModifiers()
    {
        attack.ResetModifier();
        attackSpeed.ResetModifier();
        maxHp.ResetModifier();
        moveSpeed.ResetModifier();
        expGain.ResetModifier();

        criticalChance.ResetModifier();
        criticalDamage.ResetModifier();
        defense.ResetModifier();
        dropRate.ResetModifier();
        goldGain.ResetModifier();
        lifeSteal.ResetModifier();
        healthRegen.ResetModifier();
    }
    // 능력치 적용 메서드
    public void RecalculateStats(List<ItemData> inventory)
    {
        float prevMaxHp = maxHp.Value;

        ResetStatModifiers();

        Dictionary<ItemType, float> highestValues = new Dictionary<ItemType, float>();

        foreach (var item in inventory)
        {
            if (item == null)
                continue;

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
                    criticalChance.addValue += v;
                    break;

                case ItemType.CriticalDamage:
                    criticalDamage.mulValue *= (1f + v / 100f);
                    break;

                case ItemType.Defense:
                    defense.addValue += v;
                    break;

                case ItemType.HealthRegen:
                    healthRegen.addValue += v / 10f;
                    break;

                case ItemType.LifeSteal:
                    lifeSteal.addValue += v / 10f;
                    break;

                case ItemType.GoldGain:
                    goldGain.addValue += v;
                    break;

                case ItemType.DropRate:
                    dropRate.addValue += v;
                    break;

                case ItemType.ExpGain:
                    expGain.mulValue *= (1f + v / 100f);
                    break;

                case ItemType.HealthRegen_LifeSteal:
                    healthRegen.addValue += v / 10f;
                    lifeSteal.addValue += v / 10f;
                    break;

                case ItemType.CriticalDamage_Attack:
                    criticalDamage.mulValue *= (1f + v / 100f);
                    attack.mulValue *= (1f + v / 100f);
                    break;

                case ItemType.DropRate_ExpGain:
                    dropRate.addValue += v;
                    expGain.mulValue *= (1f + v / 100f);
                    break;
            }
        }

        if (playerHealth != null)
        {
            playerHealth.OnMaxHpIncreased(prevMaxHp);
        }
    }
    public void ApplyStatUpgrade(string stat, float percent)
    {
        switch (stat)
        {
            case "Attack":
                attack.mulValue *= (1f + percent / 100f);
                break;

            case "AttackSpeed":
                attackSpeed.mulValue *= (1f + percent / 100f);
                break;

            case "Hp":
                float prevHp = maxHp.Value;
                maxHp.mulValue *= (1f + percent / 100f);

                if (playerHealth != null)
                    playerHealth.OnMaxHpIncreased(prevHp);
                break;

            case "MoveSpeed":
                moveSpeed.mulValue *= (1f + percent / 100f);
                break;

            case "Exp":
                expGain.mulValue *= (1f + percent / 100f);
                break;

            case "AllStat":
                float prevAllHp = maxHp.Value;

                attack.mulValue *= (1f + percent / 100f);
                attackSpeed.mulValue *= (1f + percent / 100f);
                moveSpeed.mulValue *= (1f + percent / 100f);
                expGain.mulValue *= (1f + percent / 100f);
                maxHp.mulValue *= (1f + percent / 100f);

                if (playerHealth != null)
                    playerHealth.OnMaxHpIncreased(prevAllHp);
                break;

            default:
                Debug.LogWarning($"Unknown stat type: {stat}");
                break;
        }
    }
    public void AddGold(int baseGold)
    {
        float finalGold = baseGold * (1f + goldGain.Value / 100f);
        gold += Mathf.FloorToInt(finalGold);
    }
}