using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class PlayerExperience : MonoBehaviour
{
    public int level = 1;
    public int currentExp = 0;
    public TextMeshProUGUI expText;
    public TextMeshProUGUI levelText;
    public Image expBar;

    private Coroutine expCoroutine;
    private int queuedExp;
    private int pendingLevelUps = 0;

    public LevelUpUI levelUpUI;
    private PlayerStats playerStats; // 추가

    public delegate void OnLevelUp(int newLevel);
    public event OnLevelUp LevelUpEvent;

    void Awake()
    {
        // PlayerStats 참조 가져오기
        playerStats = GetComponent<PlayerStats>();
        if (playerStats == null)
            Debug.LogWarning("PlayerStats 컴포넌트를 찾을 수 없습니다.");
    }

    public void GainExp(int amount)
    {
        if (amount <= 0) return;

        // expGain 반영
        float gainMultiplier = playerStats != null ? playerStats.expGain : 1f;
        int finalExp = Mathf.RoundToInt(amount * gainMultiplier);

        queuedExp += finalExp;
        if (expCoroutine == null)
            expCoroutine = StartCoroutine(GainExpCoroutine());
    }

    private IEnumerator GainExpCoroutine()
    {
        while (queuedExp > 0)
        {
            int expToAdd = queuedExp;
            queuedExp = 0;

            while (expToAdd > 0)
            {
                int expToNext = GetExpToNextLevel(level) - currentExp;
                int gain = Mathf.Min(expToAdd, expToNext);
                expToAdd -= gain;

                int startExp = currentExp;
                int targetExp = currentExp + gain;
                float duration = 0.4f;
                float elapsed = 0f;

                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    currentExp = Mathf.RoundToInt(Mathf.Lerp(startExp, targetExp, elapsed / duration));
                    UpdateUI();
                    yield return null;
                }

                currentExp = targetExp;

                if (currentExp >= GetExpToNextLevel(level))
                {
                    currentExp -= GetExpToNextLevel(level);
                    level++;
                    pendingLevelUps++;
                    LevelUpEvent?.Invoke(level);
                }

                UpdateUI();
            }
        }

        expCoroutine = null;

        if (pendingLevelUps > 0 && levelUpUI != null)
        {
            ShowLevelUpUI();
        }
    }

    private void ShowLevelUpUI()
    {
        levelUpUI.gameObject.SetActive(true);
        levelUpUI.OpenLevelUpUI(this);
    }

    public void OnLevelUpChoiceSelected()
    {
        pendingLevelUps--;
        if (pendingLevelUps > 0)
            ShowLevelUpUI();
    }

    private int GetExpToNextLevel(int level)
    {
        return Mathf.RoundToInt(100 * Mathf.Pow(1.2f, level - 1));
    }

    private void UpdateUI()
    {
        if (levelText != null) levelText.text = $"{level} Lv";
        if (expText != null) expText.text = $"{currentExp} / {GetExpToNextLevel(level)}";
        if (expBar != null) expBar.fillAmount = (float)currentExp / GetExpToNextLevel(level);
    }
}