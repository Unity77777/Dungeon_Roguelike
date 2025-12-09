using UnityEngine;
using TMPro;

public class MonsterDamageText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textMesh; // 반드시 연결
    public float floatSpeed = 1f;
    public float lifetime = 1f;

    public void Setup(string text, Color color)
    {
        if (textMesh == null)
            textMesh = GetComponent<TextMeshProUGUI>();

        textMesh.text = text;
        textMesh.color = color;

        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        transform.position += Vector3.up * floatSpeed * Time.deltaTime;
    }
}