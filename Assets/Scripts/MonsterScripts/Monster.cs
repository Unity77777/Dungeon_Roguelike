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

    [Header("Item Drop")]
    public GameObject itemPrefab;
    public float dropChance = 0.5f;
    public Sprite dropIcon;

    [Header("Drop Item inventory")]
    public ItemData[] possibleDrops;

    private Animator animator;
    private bool isDead = false;

    public GameObject damageTextPrefab;
    private Transform worldCanvas;
    private MonsterAttack attackModule;
    void Start()
    {
        animator = GetComponent<Animator>();
        currentHP = maxHP;

        attackModule = GetComponent<MonsterAttack>();
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

        // 경험치 지급
        playerInventory?.GetComponent<PlayerExperience>()?.GainExp(150);

        // 골드 지급 로직 추가
        GiveGoldReward();

        // 아이템 드랍 시도
        DropSystem.TryDrop(possibleDrops, dropChance, transform.position, playerInventory, itemPrefab);

        Destroy(gameObject, 3f);
    }
    private void GiveGoldReward()
    {
        if (playerInventory == null)
            return;

        PlayerStats stats = playerInventory.GetComponent<PlayerStats>();
        if (stats == null)
            return;

        // 기본 골드 1~100 랜덤
        int baseGold = Random.Range(1, 101);

        // 골드 증가율(goldGain%) 적용
        float finalGold = baseGold * (1f + stats.goldGain / 100f);

        // 정수화 및 누적
        int gainedGold = Mathf.RoundToInt(finalGold);
        stats.gold += gainedGold;

        Debug.Log($"[골드 획득] {baseGold} → 최종 {gainedGold} (보너스 {stats.goldGain:F1}%) / 총 보유: {stats.gold}");
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