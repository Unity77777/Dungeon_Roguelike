using UnityEngine;

public class OrcHpBar : MonoBehaviour
{
    public Monster targetMonster; // 체력을 가져올 몬스터
    public Material hpMaterial;   // Inspector에서 넣어준 Material
    private Material instanceMaterial; // 각자 독립적인 Material

    [Range(0, 1)]
    public float hpPercent = 1f;

    void Awake()
    {
        if (hpMaterial != null)
        {
            // Material 인스턴스화 (복제)
            instanceMaterial = new Material(hpMaterial);

            // Renderer가 있다면 적용
            var renderer = GetComponent<Renderer>();
            if (renderer != null)
                renderer.material = instanceMaterial;
        }
    }

    void Update()
    {
        if (targetMonster == null || instanceMaterial == null) return;

        // 몬스터 체력에 맞춰 비율 계산
        hpPercent = (float)targetMonster.currentHP / targetMonster.maxHP;

        // Shader에 _HP 값 전달
        instanceMaterial.SetFloat("_HP", hpPercent);
    }
}