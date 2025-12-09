using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
public class PlayerController : MonoBehaviour
{
    private Animator animator;
    private PlayerMovement movement;
    private PlayerHealth health;
    private PlayerExperience experience;
    private PlayerCombat combat;

    private bool isDead;

    void Awake()
    {
        animator = GetComponent<Animator>();
        movement = GetComponent<PlayerMovement>();
        health = GetComponent<PlayerHealth>();
        experience = GetComponent<PlayerExperience>();
        combat = GetComponent<PlayerCombat>();

        if (health != null)
            health.OnDeath += OnPlayerDeath;

        if (experience != null)
            experience.LevelUpEvent += OnLevelUp;
    }

    void Update()
    {
        if (isDead) return;

        // 공격 입력
        if (Input.GetMouseButtonDown(0) && combat != null)
        {
            combat.TryAttack();
        }

        // 이동 애니메이션 (공격 중엔 속도 0)
        float speed = (combat != null && combat.IsAttacking)
            ? 0f
            : Mathf.Abs(movement.Horizontal) + Mathf.Abs(movement.Vertical);

        animator.SetFloat("speed", Mathf.Clamp01(speed));
    }

    private void OnLevelUp(int newLevel)
    {
        // 이 함수는 단순히 “레벨이 올랐다”는 신호만 처리
        Debug.Log($"레벨업! 현재 레벨: {newLevel}");
        // 실제 스탯 강화나 체력 증가는 PlayerStats 내부에서 처리
    }

    private void OnPlayerDeath()
    {
        isDead = true;

        if (movement != null)
            movement.CanMove = false;

        if (animator != null)
            animator.SetFloat("speed", 0f);

        Debug.Log("플레이어 사망");
    }
}