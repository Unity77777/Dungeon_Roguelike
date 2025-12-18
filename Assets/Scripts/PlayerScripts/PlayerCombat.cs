using System.Collections.Generic;
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
    private bool inputLocked = false;
    private bool damageAppliedThisAttack = false;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        movement = GetComponent<PlayerMovement>();
        playerStats = GetComponent<PlayerStats>();

        if (attackCollider != null)
            attackCollider.enabled = false;
    }

    // 외부 입력에서 호출
    public void TryAttack()
    {
        if (inputLocked)
            return;

        inputLocked = true;

        if (!isAttacking)
            StartAttack();
        else
            attackQueued = true;
    }

    private void StartAttack()
    {
        isAttacking = true;
        damageAppliedThisAttack = false;

        movement.CanMove = false;

        float animSpeed = playerStats != null ? playerStats.attackSpeed.Value : 1f;
        animator.speed = animSpeed;

        animator.ResetTrigger("attackTrigger");
        animator.SetTrigger("attackTrigger");
        animator.SetFloat("speed", 0f);
    }

    // 애니메이션 이벤트
    public void OnAttackStart()
    {
        if (damageAppliedThisAttack)
            return;

        damageAppliedThisAttack = true;

        if (attackCollider != null)
            attackCollider.enabled = true;

        ApplyDamage();
    }

    // 애니메이션 이벤트
    public void OnAttackEnd()
    {
        if (attackCollider != null)
            attackCollider.enabled = false;

        isAttacking = false;
        movement.CanMove = true;
        animator.speed = 1f;

        inputLocked = false;

        if (attackQueued)
        {
            attackQueued = false;
            StartAttack();
        }
    }

    private void ApplyDamage()
    {
        if (!(attackCollider is SphereCollider sphere))
            return;

        Vector3 center = sphere.transform.TransformPoint(sphere.center);
        float radius = sphere.radius * Mathf.Max(
            sphere.transform.lossyScale.x,
            sphere.transform.lossyScale.y,
            sphere.transform.lossyScale.z
        );

        Collider[] hits = Physics.OverlapSphere(center, radius);

        HashSet<MonsterHealth> damagedTargets = new HashSet<MonsterHealth>();

        foreach (Collider col in hits)
        {
            MonsterHealth health = col.GetComponentInParent<MonsterHealth>();
            if (health == null || health.IsDead)
                continue;

            if (damagedTargets.Contains(health))
                continue;

            damagedTargets.Add(health);

            DealDamageTo(health);
        }
    }

    private void DealDamageTo(MonsterHealth health)
    {
        float baseAttack = playerStats != null ? playerStats.attack.Value : 10f;

        bool isCritical = false;
        if (playerStats != null && playerStats.criticalChance.Value > 0f)
        {
            float rand = Random.value * 100f;
            isCritical = rand < playerStats.criticalChance.Value;
        }

        float finalDamage = baseAttack;

        if (isCritical)
        {
            float critMultiplier = 1.5f + (playerStats.criticalDamage.Value / 100f);
            finalDamage *= critMultiplier;
        }

        health.TakeDamage(Mathf.RoundToInt(finalDamage), isCritical);
        HandleLifeSteal(finalDamage);
    }

    private void HandleLifeSteal(float damage)
    {
        if (playerStats == null || playerStats.lifeSteal.Value <= 0f)
            return;

        PlayerHealth health = GetComponent<PlayerHealth>();
        if (health == null)
            return;

        float healAmount = damage * (playerStats.lifeSteal.Value / 100f);
        health.Heal(healAmount);
    }

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