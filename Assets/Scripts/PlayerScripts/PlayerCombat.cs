using UnityEngine;

[RequireComponent(typeof(Animator), typeof(PlayerMovement))]
public class PlayerCombat : MonoBehaviour
{
    public Collider attackCollider;

    private Animator animator;
    private PlayerMovement movement;
    private PlayerStats playerStats;

    private bool isAttacking = false;
    private bool attackQueued = false;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        movement = GetComponent<PlayerMovement>();
        playerStats = GetComponent<PlayerStats>();

        if (attackCollider != null)
            attackCollider.enabled = false;
    }

    // 공격 시도 (PlayerController에서 호출)
    public void TryAttack()
    {
        if (!isAttacking)
            StartAttack();
        else
            attackQueued = true;
    }

    private void StartAttack()
    {
        isAttacking = true;
        movement.CanMove = false;

        // 공격속도에 따라 애니메이션 재생속도 조절
        float animSpeed = playerStats != null ? playerStats.attackSpeed : 1f;
        animator.speed = animSpeed;

        animator.SetTrigger("attackTrigger");
        animator.SetFloat("speed", 0);
    }

    // 애니메이션 이벤트: 공격 시작 시점
    public void OnAttackStart()
    {
        if (attackCollider != null)
            attackCollider.enabled = true;

        ApplyDamage();
    }

    // 애니메이션 이벤트: 공격 종료 시점
    public void OnAttackEnd()
    {
        if (attackCollider != null)
            attackCollider.enabled = false;

        isAttacking = false;
        movement.CanMove = true;

        // 공격 끝나면 애니메이션 속도 복원
        animator.speed = 1f;

        // 큐 대기 중이면 즉시 다음 공격 실행
        if (attackQueued)
        {
            attackQueued = false;
            StartAttack();
        }
    }

    // 실제 공격 판정 수행
    private void ApplyDamage()
    {
        if (attackCollider is SphereCollider sphere)
        {
            Vector3 center = sphere.transform.TransformPoint(sphere.center);
            float radius = sphere.radius * Mathf.Max(
                sphere.transform.lossyScale.x,
                sphere.transform.lossyScale.y,
                sphere.transform.lossyScale.z
            );

            Collider[] hits = Physics.OverlapSphere(center, radius);
            foreach (var col in hits)
            {
                Monster monster = col.GetComponent<Monster>();
                if (monster != null)
                {
                    float baseAttack = playerStats != null ? playerStats.attack : 10f;

                    // 크리티컬 판정
                    bool isCritical = false;
                    if (playerStats != null && playerStats.criticalChance > 0f)
                    {
                        float rand = Random.value * 100f;
                        isCritical = rand < playerStats.criticalChance;
                    }

                    // 크리티컬 배율 적용
                    float finalDamage = baseAttack;
                    if (isCritical)
                    {
                        float critMultiplier = 1.5f + (playerStats.criticalDamage / 100f);
                        finalDamage *= critMultiplier;
                        Debug.Log($"[크리티컬!] {finalDamage:F1} 피해 (배율 {critMultiplier:F2})");
                    }

                    Debug.Log($"[PlayerCombat] Damage={finalDamage}, Critical={isCritical}");

                    // ★ 크리티컬 여부 전달 (중요)
                    monster.TakeDamage(Mathf.RoundToInt(finalDamage), isCritical);

                    // 피흡 처리
                    if (playerStats != null && playerStats.lifeSteal > 0f)
                    {
                        PlayerHealth health = GetComponent<PlayerHealth>();
                        if (health != null)
                        {
                            float healAmount = finalDamage * (playerStats.lifeSteal / 100f);
                            health.Heal(healAmount);
                            Debug.Log($"[피흡] {healAmount:F1} 회복 (피흡률 {playerStats.lifeSteal:F1}%)");
                        }
                    }
                }
            }
        }
    }

    // Scene 뷰에서 공격 범위 표시
    private void OnDrawGizmosSelected()
    {
        if (attackCollider is SphereCollider sphere)
        {
            Gizmos.color = Color.red;
            Vector3 center = sphere.transform.TransformPoint(sphere.center);
            Gizmos.DrawWireSphere(center, sphere.radius);
        }
    }

    public bool IsAttacking => isAttacking;
}