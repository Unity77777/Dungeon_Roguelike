using UnityEngine;

public class PlayerHpBar : MonoBehaviour
{
    public PlayerHealth targetPlayer; // 체력을 참조할 플레이어
    public Material hpMaterial;       // 체력바용 머티리얼 (Quad 등)
    [Range(0, 1)]
    public float hpPercent = 1f;      // Shader에 전달될 HP 비율

    void Update()
    {
        if (targetPlayer == null || hpMaterial == null) return;

        // PlayerHealth의 프로퍼티를 사용해 체력 비율 계산
        hpPercent = targetPlayer.MaxHP > 0 ? targetPlayer.CurrentHP / targetPlayer.MaxHP : 0f;

        // Shader에 _HP 값 전달
        hpMaterial.SetFloat("_HP", hpPercent);
    }
}