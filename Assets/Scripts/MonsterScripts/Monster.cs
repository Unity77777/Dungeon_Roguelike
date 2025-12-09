using UnityEngine;

// ================= 몬스터 스크립트 =================
[RequireComponent(typeof(CharacterController))]
public class Monster : MonoBehaviour
{
    [Header("Player Tracking")]
    public Transform player;
    public float moveSpeed = 3f;
    public float detectionRange = 10f;
    public float stopDistance = 1.5f;
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

    [Header("Separation Setting")]
    public float separationRadius = 1f;
    public LayerMask monsterLayer;

    private CharacterController controller;
    private Animator animator;
    private float verticalVelocity = 0f;
    private bool isDead = false;
    private bool isAttacking = false;

    private float currentGauge = 0f;

    public GameObject damageTextPrefab;
    private Transform worldCanvas;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        currentHP = maxHP;

        if (monsterAtkCol != null)
            monsterAtkCol.enabled = false;

        GameObject obj = GameObject.Find("WorldCanvas");
        if (obj != null)
            worldCanvas = obj.transform;
        else
            Debug.LogError("[Monster] 'WorldCanvas' 오브젝트를 찾을 수 없습니다.");
    }

    void Update()
    {
        if (player == null || isDead) return;

        HandleMovementAndAttack();
        ApplyGravity();
        UpdateAttackGaugeUI();
    }

    private void HandleMovementAndAttack()
    {
        Vector3 toPlayer = player.position - transform.position;
        float distance = toPlayer.magnitude;
        Vector3 move = Vector3.zero;

        if (distance <= detectionRange)
        {
            bool shouldWalk = distance > stopDistance;
            animator.SetBool("isWalking", shouldWalk);

            if (shouldWalk)
            {
                Vector3 moveDir = toPlayer;
                moveDir.y = 0f;
                moveDir.Normalize();
                move += moveDir * moveSpeed;
            }

            if (distance <= attackDistance && Time.time - lastAttackTime >= attackCooldown && !isAttacking)
            {
                animator.SetTrigger("attackTrigger");
                lastAttackTime = Time.time;
                StartAttack();
            }
        }
        else
        {
            animator.SetBool("isWalking", false);
        }

        ApplySeparation(ref move);

        move.y = verticalVelocity;
        controller.Move(move * Time.deltaTime);
    }

    private void ApplySeparation(ref Vector3 move)
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, separationRadius, monsterLayer);
        Vector3 separationOffset = Vector3.zero;

        foreach (var hit in hits)
        {
            if (hit.gameObject == gameObject) continue;

            Vector3 diff = transform.position - hit.transform.position;
            diff.y = 0f;
            float dist = diff.magnitude;

            if (dist < separationRadius && dist > 0f)
                separationOffset += diff.normalized * (separationRadius - dist);
        }

        move += separationOffset;
    }

    private void ApplyGravity()
    {
        if (controller.isGrounded)
            verticalVelocity = 0f;
        else
            verticalVelocity -= 9.81f * Time.deltaTime;
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
        controller.enabled = false;

        if (attackGaugeUI != null)
            attackGaugeUI.gameObject.SetActive(false);

        // 경험치 지급
        playerInventory?.GetComponent<PlayerExperience>()?.GainExp(150);

        // 골드 지급 로직 추가
        GiveGoldReward();

        // 아이템 드랍 시도
        TryDropItem();

        Destroy(gameObject, 3f);
    }

    private void TryDropItem()
    {
        if (itemPrefab == null || playerInventory == null)
            return;

        if (possibleDrops == null || possibleDrops.Length == 0)
        {
            Debug.LogWarning("[TryDropItem] possibleDrops 배열이 비어 있습니다.");
            return;
        }

        // 플레이어의 드랍률 보정 가져오기
        PlayerStats stats = playerInventory.GetComponent<PlayerStats>();
        float playerDropBonus = (stats != null) ? stats.dropRate : 0f;

        // 최종 드랍 확률 계산 (예: 50% + 20% = 70%)
        float finalDropChance = Mathf.Clamp01((dropChance + playerDropBonus) / 100f);

        if (Random.value <= finalDropChance)
        {
            // 아이템 프리팹 생성
            GameObject go = Instantiate(itemPrefab, transform.position + Vector3.up, Quaternion.identity);

            Rigidbody rb = go.GetComponent<Rigidbody>();
            //if (rb != null) rb.isKinematic = true;

            Collider col = go.GetComponent<Collider>();
            //if (col != null) col.isTrigger = true;

            // ScriptableObject 배열에서 랜덤 아이템 선택
            ItemData baseItem = possibleDrops[Random.Range(0, possibleDrops.Length)];

            // 값 복사 및 드랍용 새 인스턴스 생성
            float randomValue = Random.Range(1f, 100f);
            ItemData dropItem = ScriptableObject.CreateInstance<ItemData>();
            dropItem.id = baseItem.id;
            dropItem.itemName = baseItem.itemName;
            dropItem.icon = baseItem.icon;
            dropItem.type = baseItem.type;
            dropItem.value = randomValue;
            dropItem.UpdateRarityByValue();

            // ItemDrop 초기화 호출 (스프라이트, 빛 색상 반영)
            ItemDrop drop = go.GetComponent<ItemDrop>();
            if (drop != null)
                drop.Initialize(dropItem);

            // ItemPickup에도 데이터 전달
            ItemPickup pickup = go.GetComponent<ItemPickup>();
            if (pickup != null)
            {
                pickup.item = dropItem;
                pickup.inventory = playerInventory;
            }

            Debug.Log($"[드랍] {dropItem.itemName} 생성됨 (Value={dropItem.value:F1}, Rarity={dropItem.rarity}) | 최종확률 {finalDropChance * 100f:F1}% (기본 {dropChance}%, 보정 +{playerDropBonus:F1}%)");
        }
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
        if (damageTextPrefab == null || worldCanvas == null) return;

        // 생성 위치 (몬스터 머리 위)
        Vector3 worldPos = transform.position + Vector3.up * 1.5f;

        GameObject dmgObj = DamageTextPool.Instance.Get();
        dmgObj.transform.SetParent(worldCanvas, false);

        dmgObj.transform.position = worldPos; // 월드 캔버스 이므로 실제 좌표로 배치

        // 텍스트 설정
        MonsterDamageText dmgText = dmgObj.GetComponent<MonsterDamageText>();
        if (dmgText != null)
        {
            Color color = isCritical ? Color.red : Color.white;
            dmgText.Setup(damage.ToString(), color);
        }
    }
}