using UnityEngine;

// ================= 몬스터 스크립트 =================
public class Monster : MonoBehaviour
{
    public Transform player;
    public float attackDistance = 2.0f;

    [Header("Attack")]
    public Collider monsterAtkCol;
    public float attackCooldown = 3f;
    private float lastAttackTime = -Mathf.Infinity;

    [Header("Health")]
    public int maxHP = 100;
    public int currentHP;

    [Header("Attack Gauge UI")]
    public float maxGauge = 100f;
    public float attackDuration = 0.5f;
    public float attackUIYOffset = -0.8f;
    public UnityEngine.UI.Image attackGaugeUI;

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
    private bool isAttacking = false;

    private float currentGauge = 0f;

    public GameObject damageTextPrefab;
    private Transform worldCanvas;

    void Start()
    {
        animator = GetComponent<Animator>();
        currentHP = maxHP;

        if (monsterAtkCol != null)
            monsterAtkCol.enabled = false;

        GameObject obj = GameObject.Find("WorldCanvas");
        if (obj != null)
        {
            worldCanvas = obj.transform;
            DamageTextPool.Instance.Initialize(worldCanvas);
        }
        else
            Debug.LogError("[Monster] 'WorldCanvas' 오브젝트를 찾을 수 없습니다.");
    }

    void Update()
    {
        if (player == null || isDead)
            return;

        HandleAttack();
        UpdateAttackGaugeUI();
    }

    private void HandleAttack()
    {
        Vector3 toPlayer = player.position - transform.position;
        float distance = toPlayer.magnitude;

        if (distance <= attackDistance &&Time.time - lastAttackTime >= attackCooldown && !isAttacking)
        {
            animator.SetTrigger("attackTrigger");
            lastAttackTime = Time.time;
            StartAttack();
        }
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

        if (attackGaugeUI != null)
            attackGaugeUI.gameObject.SetActive(false);

        // 경험치 지급
        playerInventory?.GetComponent<PlayerExperience>()?.GainExp(150);

        // 골드 지급 로직 추가
        GiveGoldReward();

        // 아이템 드랍 시도
        DropSystem.TryDrop(possibleDrops, dropChance, transform.position, playerInventory, itemPrefab);

        Destroy(gameObject, 3f);
    }

    private void StartAttack()
    {
        if (monsterAtkCol != null)
            monsterAtkCol.enabled = true;

        isAttacking = true;
        currentGauge = 0f;

        if (attackGaugeUI != null)
        {
            attackGaugeUI.fillAmount = 0f;
            attackGaugeUI.gameObject.SetActive(true);
        }
    }

    private void EndAttack()
    {
        if (monsterAtkCol != null)
            monsterAtkCol.enabled = false;

        if (monsterAtkCol is SphereCollider sphere)
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
                PlayerHealth pc = col.GetComponent<PlayerHealth>();
                if (pc != null)
                {
                    pc.TakeDamage(10);
                    break;
                }
            }
        }

        isAttacking = false;
        currentGauge = 0f;

        if (attackGaugeUI != null)
            attackGaugeUI.gameObject.SetActive(false);
    }

    private void UpdateAttackGaugeUI()
    {
        if (attackGaugeUI == null || !isAttacking) return;

        currentGauge += (Time.deltaTime / attackDuration) * maxGauge;
        currentGauge = Mathf.Clamp(currentGauge, 0f, maxGauge);

        attackGaugeUI.fillAmount = currentGauge / maxGauge;
        attackGaugeUI.transform.position = transform.position + Vector3.up * attackUIYOffset;

        if (currentGauge >= maxGauge)
            EndAttack();
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