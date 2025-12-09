using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class LevelUpUI : MonoBehaviour
{
    public Button[] optionButtons;

    private PlayerExperience playerExp;
    private PlayerStats playerStats;

    private Dictionary<string, int> statPercentMap = new Dictionary<string, int>();
    private string[] allStats = { "Attack", "AttackSpeed", "Hp", "MoveSpeed", "Exp", "AllStat" };

    public Sprite attackIcon;
    public Sprite attackSpeedIcon;
    public Sprite hpIcon;
    public Sprite moveSpeedIcon;
    public Sprite expIcon;
    public Sprite allStatIcon;

    public Sprite commonSprite;
    public Sprite uncommonSprite;
    public Sprite epicSprite;
    public Sprite legendSprite;
    public Sprite mythSprite;

    private bool isPaused = false;

    public void OpenLevelUpUI(PlayerExperience exp)
    {
        playerExp = exp;
        playerStats = exp.GetComponent<PlayerStats>();
        GenerateRandomOptions();
        PauseGame(true);
    }

    private void OnEnable()
    {
        if (playerExp != null)
            GenerateRandomOptions();
    }

    private void GenerateRandomOptions()
    {
        statPercentMap.Clear();
        List<string> availableStats = new List<string>(allStats);

        for (int i = 0; i < optionButtons.Length; i++)
        {
            if (availableStats.Count == 0) break;

            int randomIndex = Random.Range(0, availableStats.Count);
            string stat = availableStats[randomIndex];
            availableStats.RemoveAt(randomIndex);

            int rarityRoll = Random.Range(1, 101);
            string rarity = GetWeightedRarity(rarityRoll);

            (string minPercent, string maxPercent) rarityRange = rarity switch
            {
                "Common" => ("1", "2"),
                "UnCommon" => ("3", "4"),
                "Epic" => ("5", "8"),
                "Legend" => ("9", "16"),
                "Myth" => ("17", "32"),
                _ => ("1", "32")
            };

            int statPercent = rarity switch
            {
                "Common" => Random.Range(1, 3),
                "UnCommon" => Random.Range(3, 5),
                "Epic" => Random.Range(5, 9),
                "Legend" => Random.Range(9, 17),
                "Myth" => Random.Range(17, 33),
                _ => Random.Range(1, 33)
            };

            statPercentMap[stat] = statPercent;

            Transform rarityObj = optionButtons[i].transform.Find("rarity");
            if (rarityObj != null)
            {
                Image rarityImage = rarityObj.GetComponent<Image>();
                if (rarityImage != null)
                    rarityImage.sprite = GetRaritySprite(rarity);
            }

            Image buttonImage = optionButtons[i].GetComponent<Image>();
            if (buttonImage != null)
                buttonImage.sprite = GetStatIcon(stat);

            TextMeshProUGUI text = optionButtons[i].GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
                text.text = $"{stat}\n{statPercent}%\n({rarityRange.minPercent}% ~ {rarityRange.maxPercent}%)";

            optionButtons[i].onClick.RemoveAllListeners();
            optionButtons[i].onClick.AddListener(() => OnSelectStat(stat));
        }
    }

    private void OnSelectStat(string stat)
    {
        Debug.Log($"{stat} 선택됨");

        if (playerStats != null && statPercentMap.ContainsKey(stat))
        {
            playerStats.ApplyStatUpgrade(stat, statPercentMap[stat]);
        }

        gameObject.SetActive(false);
        PauseGame(false);

        if (playerExp != null)
            playerExp.OnLevelUpChoiceSelected();
    }

    private string GetWeightedRarity(int roll)
    {
        if (roll <= 50) return "Common";
        else if (roll <= 75) return "UnCommon";
        else if (roll <= 88) return "Epic";
        else if (roll <= 95) return "Legend";
        else return "Myth";
    }

    private Sprite GetRaritySprite(string rarity)
    {
        return rarity switch
        {
            "Common" => commonSprite,
            "UnCommon" => uncommonSprite,
            "Epic" => epicSprite,
            "Legend" => legendSprite,
            "Myth" => mythSprite,
            _ => null
        };
    }

    private Sprite GetStatIcon(string stat)
    {
        return stat switch
        {
            "Attack" => attackIcon,
            "AttackSpeed" => attackSpeedIcon,
            "Hp" => hpIcon,
            "MoveSpeed" => moveSpeedIcon,
            "Exp" => expIcon,
            "AllStat" => allStatIcon,
            _ => null
        };
    }

    private void PauseGame(bool pause)
    {
        if (pause && !isPaused)
        {
            Time.timeScale = 0f;
            isPaused = true;
        }
        else if (!pause && isPaused)
        {
            Time.timeScale = 1f;
            isPaused = false;
        }
    }
}