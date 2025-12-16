using UnityEngine;

public class OrcHpBar : MonoBehaviour
{
    public MonsterHealth targetHealth; // 체력을 가져올 몬스터
    public Material hpMaterial;   // Inspector에서 넣어준 Material

    private Material instanceMaterial; // 각자 독립적인 Material

    [Range(0f, 1f)]
    public float hpPercent = 1f;

    void Awake()
    {
        if(targetHealth == null)
        {
            targetHealth = GetComponentInParent<MonsterHealth>();
        }

        if(hpMaterial != null)
        {
            instanceMaterial = new Material(hpMaterial);

            var renderer = GetComponent<Renderer>();
            if(renderer != null)
            {
                renderer.material = instanceMaterial;
            }
        }
    }

    void Update()
    {
        if (targetHealth == null || instanceMaterial == null) return;

        // 몬스터 체력에 맞춰 비율 계산
        hpPercent = (float)targetHealth.CurrentHP / targetHealth.MaxHP;

        // Shader에 _HP 값 전달
        instanceMaterial.SetFloat("_HP", hpPercent);
    }
}