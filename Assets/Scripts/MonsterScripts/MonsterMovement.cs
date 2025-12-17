using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class MonsterMovement : MonoBehaviour
{
    [Header("Target")]
    public Transform player;

    [Header("Movement")]
    public float moveSpeed = 3f;
    public float detectionRange = 10f;
    public float stopDistance = 1.5f;

    [Header("Gravity")]
    public float gravity = 9.81f;
    private float verticalVelocity;

    [Header("Separation")]
    public float separationRadius = 1.2f;
    public float separationStrength = 3f;
    public LayerMask monsterLayer;

    private CharacterController controller;
    private Animator animator;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (player == null)
            return;

        Vector3 move = CalculateChaseMovement();
        ApplyGravity(ref move);

        controller.Move(move * Time.deltaTime);
    }

    void LateUpdate()
    {
        ApplySeparation();
    }

    private Vector3 CalculateChaseMovement()
    {
        Vector3 move = Vector3.zero;

        Vector3 toPlayer = player.position - transform.position;
        float distance = toPlayer.magnitude;

        if (distance <= detectionRange)
        {
            bool shouldWalk = distance > stopDistance;
            animator.SetBool("isWalking", shouldWalk);

            if (shouldWalk)
            {
                Vector3 dir = toPlayer;
                dir.y = 0f;
                dir.Normalize();

                move += dir * moveSpeed;
            }
        }
        else
        {
            animator.SetBool("isWalking", false);
        }

        return move;
    }

    private void ApplyGravity(ref Vector3 move)
    {
        if (controller.isGrounded)
            verticalVelocity = 0f;
        else
            verticalVelocity -= gravity * Time.deltaTime;

        move.y = verticalVelocity;
    }

    private void ApplySeparation()
    {
        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            separationRadius,
            monsterLayer
        );

        Vector3 separationMove = Vector3.zero;

        foreach (var hit in hits)
        {
            if (hit.gameObject == gameObject)
                continue;

            Vector3 diff = transform.position - hit.transform.position;
            diff.y = 0f;

            float dist = diff.magnitude;

            if (dist < 0.001f)
            {
                Vector2 randomDir = Random.insideUnitCircle.normalized;
                diff = new Vector3(randomDir.x, 0f, randomDir.y);
                dist = 0.001f;
            }

            if (dist >= separationRadius)
                continue;

            float pushAmount = (separationRadius - dist) / separationRadius;
            separationMove += diff.normalized * pushAmount;
        }

        if (separationMove != Vector3.zero)
        {
            controller.Move(separationMove * separationStrength * Time.deltaTime);
        }
    }
}