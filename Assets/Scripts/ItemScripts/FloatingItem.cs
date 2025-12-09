using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class FloatingItem : MonoBehaviour
{
    public float amplitude = 0.3f;  // 부유 높이
    public float frequency = 2f;    // 위아래 진동 주기
    public float springStrength = 10f; // 복원력 세기
    public float damping = 2f;      // 감쇠력 (속도 제어)

    private Rigidbody rb;
    private float baseY;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        baseY = transform.position.y;
    }

    void FixedUpdate()
    {
        float targetY = baseY + Mathf.Sin(Time.time * frequency) * amplitude;
        float diff = targetY - transform.position.y;

        // 스프링 복원력 (목표 위치 - 현재 위치)
        float forceY = diff * springStrength - rb.linearVelocity.y * damping;

        rb.AddForce(Vector3.up * forceY, ForceMode.Acceleration);
    }
}