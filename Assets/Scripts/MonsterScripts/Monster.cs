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

        health.OnDamaged += HandleDamaged;
        health.OnDead += HandleDead;

        GameObject obj = GameObject.Find("WorldCanvas");
        if (obj != null)
        {
            worldCanvas = obj.transform;
            DamageTextPool.Instance.Initialize(worldCanvas);
        }
    }

    void Update()
    {
        if (player == null || health.IsDead)
            return;
        attackModule.TryAttack();
    }

    private void HandleDamaged(int damage, bool isCritical)
    {
        ShowDamageText(damage, isCritical);
    }

    private void HandleDead()
    {
        animator.SetTrigger("dieTrigger");

        MonsterReward reward =  GetComponent<MonsterReward>();
        reward?.GiveReward(playerInventory);

        dropper?.Drop(playerInventory, transform.position);

        Destroy(gameObject, 3f);
    }

    private void ShowDamageText(int damage, bool isCritical)
    {
        GameObject dmgObj = DamageTextPool.Instance.Get();
        dmgObj.transform.position = transform.position + Vector3.up * 1.5f;

        var dmgText = dmgObj.GetComponent<MonsterDamageText>();
        if(dmgText != null)
        {
            Color color = isCritical ? Color.red : Color.white;
            dmgText.Setup(damage.ToString(), color);
        }
    }
}