using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    public float baseSpeed = 5f;        // 기본 이동 속도 (PlayerStats가 없을 때 사용)
    public float jumpHeight = 2f;       // 점프 높이
    public float gravity = -9.81f;      // 중력
    public Transform cam;               // 카메라 참조
    public float rotationSpeed = 10f;   // 회전 속도
    public float airControl = 0.05f;    // 공중 입력 반영 비율
    public float airDrag = 0.98f;       // 공중 감속 비율
    public SpriteRenderer spriteRenderer;

    private CharacterController controller;
    private PlayerStats playerStats;
    private Vector3 velocity;
    private Vector3 horizontalVelocity;

    public float Horizontal { get; private set; }
    public float Vertical { get; private set; }
    public bool IsJumping { get; private set; }
    public bool IsRunning { get; private set; }

    [HideInInspector] public bool CanMove = true;
    [HideInInspector] public bool CanJump = true;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        playerStats = GetComponent<PlayerStats>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    public Vector3 GetHorizontalVelocity()
    {
        return horizontalVelocity;
    }

    void Update()
    {
        float h = CanMove ? Input.GetAxisRaw("Horizontal") : 0f; // 입력 즉시 감지용 Raw 사용
        float v = CanMove ? Input.GetAxisRaw("Vertical") : 0f;
        Horizontal = h;
        Vertical = v;

        Vector3 camForward = cam.forward;
        Vector3 camRight = cam.right;
        camForward.y = camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 inputDir = camForward * v + camRight * h;
        if (inputDir.magnitude > 1f) inputDir.Normalize();

        bool grounded = controller.isGrounded;

        float currentSpeed = playerStats != null ? playerStats.moveSpeed : baseSpeed;

        if (Input.GetKey(KeyCode.LeftShift) && grounded && inputDir.sqrMagnitude > 0.01f)
        {
            currentSpeed *= 2f;
            IsRunning = true;
        }
        else
        {
            IsRunning = false;
        }

        if (grounded)
        {
            horizontalVelocity = inputDir * currentSpeed;
        }
        else
        {
            Vector3 targetVelocity = inputDir * currentSpeed;
            horizontalVelocity = Vector3.Lerp(horizontalVelocity, targetVelocity, airControl);
            horizontalVelocity *= airDrag;
        }

        if (CanJump && Input.GetButtonDown("Jump") && grounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            IsJumping = true;
        }

        // 좌우 방향 입력에 따라 즉시 반전
        if (spriteRenderer != null)
        {
            if (Input.GetKey(KeyCode.A))
                spriteRenderer.flipX = true;  // 오른쪽
            else if (Input.GetKey(KeyCode.D))
                spriteRenderer.flipX = false;   // 왼쪽
        }

        Vector3 lookDir = horizontalVelocity;
        lookDir.y = 0f;
        if (lookDir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(lookDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }

        if (grounded && velocity.y < 0f)
        {
            velocity.y = -2f;
            IsJumping = false;
        }
        else
        {
            velocity.y += gravity * Time.deltaTime;
        }

        Vector3 finalMove = horizontalVelocity;
        finalMove.y = velocity.y;
        controller.Move(finalMove * Time.deltaTime);
    }
}