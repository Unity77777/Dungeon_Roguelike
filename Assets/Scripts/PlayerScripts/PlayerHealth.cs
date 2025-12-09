using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    public TextMeshProUGUI hpText;
    public Image hpBar;

    public bool IsDead { get; private set; }

    public delegate void OnPlayerDeath();
    public event OnPlayerDeath OnDeath;

    private Animator animator;
    private PlayerStats playerStats;
    private float currentHP;
    private Coroutine regenCoroutine;
    private bool canRegen = true; // 최근 피격 시 회복 제한용

    [Header("Health Regeneration")]
    public float regenDelayAfterHit = 5f; // 피격 후 회복 재개 대기 시간

    public float CurrentHP => currentHP;
    public float MaxHP => playerStats != null ? playerStats.hp : 0f;

    void Start()
    {
        animator = GetComponent<Animator>();
        playerStats = GetComponent<PlayerStats>();

        if (playerStats == null)
        {
            Debug.LogError("PlayerStats 컴포넌트를 찾을 수 없습니다. Player 오브젝트에 붙어있는지 확인하세요.");
            return;
        }

        currentHP = playerStats.hp;
        UpdateUI();

        // 체력 재생 루프 시작
        regenCoroutine = StartCoroutine(HealthRegenLoop());
    }

    public void TakeDamage(float damage)
    {
        if (IsDead) return;

        // 방어력 적용 (최대 80% 감소)
        if (playerStats != null && playerStats.defense > 0f)
        {
            float reductionRate = Mathf.Clamp(playerStats.defense / 100f, 0f, 0.8f);
            damage *= (1f - reductionRate);
        }

        currentHP = Mathf.Max(currentHP - damage, 0);
        UpdateUI();

        if (animator != null)
            animator.SetTrigger("hitTrigger");

        // 피격 시 일정 시간 리젠 중단
        if (regenCoroutine != null)
            StopCoroutine(regenCoroutine);
        canRegen = false;
        StartCoroutine(RegenDelayCoroutine());

        if (currentHP <= 0)
            Die();
    }



    private IEnumerator RegenDelayCoroutine()
    {
        // 피격 시 기존 루프 완전히 정지
        if (regenCoroutine != null)
        {
            StopCoroutine(regenCoroutine);
            regenCoroutine = null;
        }

        canRegen = false;
        yield return new WaitForSeconds(regenDelayAfterHit);

        canRegen = true;

        // 중복 방지: 루프가 이미 돌고 있지 않을 때만 실행
        if (regenCoroutine == null)
            regenCoroutine = StartCoroutine(HealthRegenLoop());
    }

    private void Die()
    {
        if (IsDead) return;
        IsDead = true;

        if (animator != null)
        {
            animator.ResetTrigger("hitTrigger");
            animator.SetTrigger("dieTrigger");
            StartCoroutine(FreezeOnDiePose(0));
        }

        var movement = GetComponent<PlayerMovement>();
        if (movement != null)
            movement.enabled = false;

        OnDeath?.Invoke();
    }

    private IEnumerator FreezeOnDiePose(int layerIndex)
    {
        while (true)
        {
            var st = animator.GetCurrentAnimatorStateInfo(layerIndex);
            if (st.IsName("Die")) break;
            yield return null;
        }

        while (true)
        {
            var st = animator.GetCurrentAnimatorStateInfo(layerIndex);
            if (st.normalizedTime >= 0.99f)
                break;
            yield return null;
        }

        animator.speed = 0f;
    }

    public void Heal(float amount)
    {
        if (IsDead) return;

        currentHP = Mathf.Min(currentHP + amount, playerStats.hp);
        UpdateUI();
    }

    public void OnMaxHpIncreased(float oldMaxHp)
    {
        if (playerStats == null) return;
        float newMaxHp = playerStats.hp;

        if (oldMaxHp > 0f)
        {
            float increaseAmount = newMaxHp - oldMaxHp;
            if (increaseAmount > 0f)
            {
                float ratio = newMaxHp / oldMaxHp;
                currentHP = Mathf.Min(currentHP * ratio, newMaxHp);
            }
        }

        UpdateUI();
        Canvas.ForceUpdateCanvases();
    }

    private void UpdateUI()
    {
        if (hpText != null)
            hpText.text = $"{Mathf.RoundToInt(currentHP)} / {Mathf.RoundToInt(playerStats.hp)}";

        if (hpBar != null)
            hpBar.fillAmount = playerStats.hp > 0 ? currentHP / playerStats.hp : 0f;
    }

    public void RefreshMaxHp()
    {
        if (currentHP > playerStats.hp)
            currentHP = playerStats.hp;

        UpdateUI();
    }

    public void Revive(float reviveHpRatio = 1f)
    {
        if (animator != null)
        {
            animator.speed = 1f;
            animator.ResetTrigger("dieTrigger");
            animator.Rebind();
            animator.Update(0f);
        }

        IsDead = false;

        currentHP = Mathf.Clamp(playerStats != null ? playerStats.hp * reviveHpRatio : 0f, 0f, playerStats != null ? playerStats.hp : 0f);
        UpdateUI();

        var movement = GetComponent<PlayerMovement>();
        if (movement != null)
            movement.enabled = true;

        // 부활 후 리젠 재시작
        if (regenCoroutine != null)
            StopCoroutine(regenCoroutine);
        regenCoroutine = StartCoroutine(HealthRegenLoop());
    }

    // 체력 재생 루프
    private IEnumerator HealthRegenLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);

            if (!canRegen) continue;
            if (IsDead) break;
            if (playerStats == null) continue;
            if (playerStats.healthRegen <= 0f) continue;
            if (currentHP >= playerStats.hp) continue;

            Heal(playerStats.healthRegen);
            Debug.Log($"[HP 리젠] +{playerStats.healthRegen:F1} 회복 → {currentHP:F1}/{playerStats.hp}");
        }

        regenCoroutine = null; // 완전히 끝났음을 표시
    }
}