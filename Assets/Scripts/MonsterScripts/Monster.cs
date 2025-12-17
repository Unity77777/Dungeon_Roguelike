using UnityEngine;

// ================= 몬스터 스크립트 =================
public class Monster : MonoBehaviour
{
    [Header("Player")]
    public PlayerInventory playerInventory;
    public PlayerHealth playerHealth;

    public Transform player;
    private Transform worldCanvas;


    private Animator animator;
    private MonsterAttack attackModule;
    private MonsterDropper dropper;
    private MonsterHealth health;

    void Start()
    {
        animator = GetComponent<Animator>();
        attackModule = GetComponent<MonsterAttack>();
        dropper = GetComponent<MonsterDropper>();
        health = GetComponent<MonsterHealth>();

        attackModule.Initialize(player);

        health.OnDead += HandleDead;
    }

    void Update()
    {
        if (player == null || health.IsDead)
            return;
        attackModule.TryAttack();
    }

    private void HandleDead()
    {
        animator.SetTrigger("dieTrigger");

        MonsterReward reward =  GetComponent<MonsterReward>();
        reward?.GiveReward(playerInventory);

        dropper?.Drop(playerInventory, transform.position);

        Destroy(gameObject, 3f);
    }
}