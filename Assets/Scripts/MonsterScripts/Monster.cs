using UnityEngine;

// ================= 몬스터 스크립트 =================
public class Monster : MonoBehaviour
{
    public Transform player;

    [Header("Health")]
    public int maxHP = 100;
    public int currentHP;

    [Header("Player")]
    public PlayerInventory playerInventory;
    public PlayerHealth playerHealth;

    private Animator animator;
    private bool isDead = false;

    public GameObject damageTextPrefab;
    private Transform worldCanvas;
    private MonsterAttack attackModule;
    private MonsterDropper dropper;
    void Start()
    {
        animator = GetComponent<Animator>();
        currentHP = maxHP;

        attackModule = GetComponent<MonsterAttack>();
        dropper = GetComponent<MonsterDropper>();
        attackModule.Initialize(player);
        GameObject obj = GameObject.Find("WorldCanvas");
        if (obj != null)
        {
            worldCanvas = obj.transform;
            DamageTextPool.Instance.Initialize(worldCanvas);
        }

    }

    void Update()
    {
        if (player == null || isDead)
            return;
        attackModule.TryAttack();
    }

    public void TakeDamage(int damage, bool isCritical = false)
    {
        if (currentHP <= 0) return;

        currentHP -= damage;
        currentHP = Mathf.Max(currentHP, 0);

        // 데미지 텍스트 표시
        ShowDamageText(damage, isCritical);

        if (currentHP <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        animator.SetTrigger("dieTrigger");

        MonsterReward reward = GetComponent<MonsterReward>();
        reward?.GiveReward(playerInventory);

        dropper?.Drop(playerInventory, transform.position);

        Destroy(gameObject, 3f);
    }

    private void ShowDamageText(int damage, bool isCritical)
    {


        GameObject dmgObj = DamageTextPool.Instance.Get();
        dmgObj.transform.position = transform.position + Vector3.up * 1.5f;

        // 텍스트 설정
        var dmgText = dmgObj.GetComponent<MonsterDamageText>();
        if (dmgText != null)
        {
            Color color = isCritical ? Color.red : Color.white;
            dmgText.Setup(damage.ToString(), color);
        }
    }
}