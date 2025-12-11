using UnityEngine;

public class MonsterAttack : MonoBehaviour
{
    public float attackDistance = 2.0f;
    public float attackCooldown = 3f;
    public Collider attackCollider;

    public float maxGauge = 100f;
    public float attackDuration = 0.5f;
    public float uiYOffset = -0.8f;
    public UnityEngine.UI.Image gaugeUI;

    private Animator animator;
    private Transform player;
    private bool isAttacking = false;
    private float currentGauge = 0f;
    private float lastAttackTime = -Mathf.Infinity;

    void Awake()
    {
        animator = GetComponent<Animator>();
        if (attackCollider != null)
            attackCollider.enabled = false;
    }

    public void Initialize(Transform target)
    {
        player = target;
    }

    void Update()
    {
        if (player == null || isAttacking == false)
            return;

        UpdateAttackGauge();
    }

    public void TryAttack()
    {
        if (player == null)
            return;

        float distance = Vector3.Distance(transform.position, player.position);

        if(distance <= attackDistance && Time.time - lastAttackTime >= attackCooldown && !isAttacking)
        {
            animator.SetTrigger("attackTrigger");
            lastAttackTime = Time.time;
            StartAttack();
        }
    }

    private void StartAttack()
    {
        if (attackCollider != null)
            attackCollider.enabled = true;

        isAttacking = true;
        currentGauge = 0f;

        if(gaugeUI != null)
        {
            gaugeUI.gameObject.SetActive(false);
        }
    }

    private void EndAttack()
    {
        if (attackCollider != null)
            attackCollider.enabled = false;

        DamagePlayerInRange();

        isAttacking = false;
        currentGauge = 0f;

        if (gaugeUI != null)
            gaugeUI.gameObject.SetActive(false);
    }

    private void DamagePlayerInRange()
    {
        if(attackCollider is SphereCollider sphere)
        {
            Vector3 center = sphere.transform.TransformPoint(sphere.center);
            float radius = sphere.radius * Mathf.Max(sphere.transform.lossyScale.x, sphere.transform.lossyScale.y, sphere.transform.lossyScale.z);

            Collider[] hits = Physics.OverlapSphere(center, radius);
            foreach(var col in hits)
            {
                PlayerHealth pc = col.GetComponent<PlayerHealth>();
                if(pc != null)
                {
                    pc.TakeDamage(10);
                    break;
                }
            }
        }
    }

    private void UpdateAttackGauge()
    {
        if (gaugeUI == null)
            return;

        currentGauge += (Time.deltaTime / attackDuration) * maxGauge;
        currentGauge = Mathf.Clamp(currentGauge, 0f, maxGauge);

        gaugeUI.fillAmount = currentGauge / maxGauge;
        gaugeUI.transform.position = transform.position + Vector3.up * uiYOffset;

        if(currentGauge >= maxGauge)
        {
            EndAttack();
        }

    }
}
