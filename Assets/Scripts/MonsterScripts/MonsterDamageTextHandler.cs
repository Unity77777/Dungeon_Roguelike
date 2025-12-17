using UnityEngine;

public class MonsterDamageTextHandler : MonoBehaviour
{
    private MonsterHealth health;

    void Awake()
    {
        health = GetComponent<MonsterHealth>();
    }

    void OnEnable()
    {
        if (health != null)
            health.OnDamaged += ShowDamageText;
    }

    void OnDisable()
    {
        if (health != null)
            health.OnDamaged -= ShowDamageText;
    }

    private void ShowDamageText(int damage, bool isCritical)
    {
        if (DamageTextPool.Instance == null)
            return;

        GameObject dmgObj = DamageTextPool.Instance.Get();
        if (dmgObj == null)
            return;

        dmgObj.transform.position = transform.position + Vector3.up * 1.5f + Vector3.right * 0.4f;

        MonsterDamageText dmgText = dmgObj.GetComponent<MonsterDamageText>();
        if (dmgText != null)
        {
            Color color = isCritical ? Color.red : Color.white;
            dmgText.Setup(damage.ToString(), color);
        }
    }
}