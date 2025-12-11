using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class MonsterMovement : MonoBehaviour
{
    public Transform player;
    public float moveSpeed = 3f;
    public float detectionRange = 10f;
    public float stopDistance = 1.5f;

    [Header("Separation")]
    public float separationRadius = 1f;
    public LayerMask monsterLayer;

    private CharacterController controller;
    private Animator animator;
    private float verticalVelocity = 0f;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (player == null)
            return;

        HandleMovement();
        ApplyGravity();
    }

    private void HandleMovement()
    {
        Vector3 toPlayer = player.position - transform.position;
        float distance = toPlayer.magnitude;
        Vector3 move = Vector3.zero;

        if(distance <= detectionRange)
        {
            bool shouldWalk = distance > stopDistance;
            animator.SetBool("isWalking", shouldWalk);

            if(shouldWalk)
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

        ApplySeparation(ref move);

        move.y = verticalVelocity;
        controller.Move(move * Time.deltaTime);
    }

    private void ApplySeparation(ref Vector3 move)
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, separationRadius, monsterLayer);
        Vector3 separationOffset = Vector3.zero;

        foreach(var hit in hits)
        {
            if (hit.gameObject == gameObject) continue;

            Vector3 diff = transform.position - hit.transform.position;
            diff.y = 0f;

            float dist = diff.magnitude;
            if (dist > 0f && dist < separationRadius)
                separationOffset += diff.normalized * (separationRadius - dist);
        }
        move += separationOffset;
    }

    private void ApplyGravity()
    {
        if (controller.isGrounded)
            verticalVelocity = 0f;
        else
            verticalVelocity -= 9.81f * Time.deltaTime;
    }
}
