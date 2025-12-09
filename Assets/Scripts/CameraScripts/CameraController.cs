using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Target & Offset")]
    public Transform target;                 // 카메라가 따라갈 대상
    public Vector3 offset;                   // 카메라와 대상 사이 기본 오프셋
    public LayerMask collisionMask;          // 충돌 처리 레이어 (Wall, Ground 등)

    [Header("Camera Control")]
    public float rotateSpeed = 5f;           // 마우스 회전 속도
    public float zoomSpeed = 2f;             // 줌 속도
    public float minZoom = 2f;               // 최소 줌 거리
    public float maxZoom = 15f;              // 최대 줌 거리
    public float smoothSpeed = 10f;          // 카메라 이동 보간 속도
    public float minDistanceToTarget = 2f;   // 플레이어와 카메라 최소 거리
    public float collisionMargin = 0.5f;     // 벽과의 최소 여유 거리

    private float yaw = 0f;                  // 좌우 회전
    private float pitch = 30f;               // 위아래 회전
    private float currentZoom;               // 현재 줌 거리

    void Start()
    {
        if (target != null)
        {
            currentZoom = offset.magnitude;                 // 초기 줌 거리 설정
            transform.position = target.position + offset;  // 초기 카메라 위치
            transform.LookAt(target.position);             // 대상 바라보기
        }
    }

    void Update()
    {
        if (target == null) return;

        // 마우스 스크롤로 줌 조절
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f)
        {
            currentZoom -= scroll * zoomSpeed;
            currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        // 마우스 오른쪽 버튼으로 카메라 회전
        if (Input.GetMouseButton(1))
        {
            yaw += Input.GetAxis("Mouse X") * rotateSpeed;
            pitch -= Input.GetAxis("Mouse Y") * rotateSpeed;
            pitch = Mathf.Clamp(pitch, -20f, 40f);
        }

        // 회전 계산
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
        Vector3 direction = rotation * offset.normalized;

        // 목표 위치 계산
        Vector3 desiredPosition = target.position + direction * currentZoom;

        // 충돌 처리 (Raycast)
        RaycastHit hit;
        float distance = currentZoom;
        if (Physics.Raycast(target.position, desiredPosition - target.position, out hit, currentZoom, collisionMask))
        {
            distance = hit.distance - collisionMargin;       // 벽과의 여유 거리 확보
            distance = Mathf.Max(distance, minDistanceToTarget); // 최소 거리 제한
        }

        Vector3 finalPosition = target.position + direction * distance;

        // 부드러운 카메라 이동
        transform.position = Vector3.Lerp(transform.position, finalPosition, Time.deltaTime * smoothSpeed);

        // 항상 타겟 바라보기
        transform.LookAt(target.position);
    }
}