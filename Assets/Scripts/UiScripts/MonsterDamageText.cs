using UnityEngine;
using TMPro;

public class MonsterDamageText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textMesh; // 반드시 연결
    public float Speed = 1f;
    public float lifetime = 1f;
    private float timer = 0f;

    public void Setup(string text, Color color)
    {
        if (textMesh == null)
            textMesh = GetComponent<TextMeshProUGUI>();

        textMesh.text = text;
        textMesh.color = color;
        timer = 0f;
    }

    private void Update()
    {
        transform.position += Vector3.up * Speed * Time.deltaTime;

        timer += Time.deltaTime;
        if(timer >= lifetime)
        {
            DamageTextPool.Instance.ReturnToPool(gameObject);
        }
    }

    public void ResetState()
    {
        timer = 0f;

        if(textMesh != null)
        {
            textMesh.text = string.Empty;
            textMesh.color = Color.white;
        }

        transform.position = Vector3.zero;
    }
}