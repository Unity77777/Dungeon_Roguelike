using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Transform targetCamera;
    void Start()
    {
        // 메인 카메라 Trasfrom 가져오기
        targetCamera = Camera.main.transform;
    }

    void LateUpdate()
    {
        // 카메라와 캐릭터 사이의 방향 벡터
        Vector3 direction = targetCamera.position - transform.position;

        // 방향 벡터가 0이 아닐때 회전 적용
        if(direction.sqrMagnitude > 0.001f)
        {
            // 카메라를 바라보는 회전 계산
            Quaternion targetRot = Quaternion.LookRotation(-direction);

            // X축 pitch  제한
            Vector3 euler = targetRot.eulerAngles;

            // Unity EulerAngle은 0~360 -> -180~180으로 변환
            if (euler.x > 180f) euler.x -= 360f;

            // pitch 제한
            euler.x = Mathf.Clamp(euler.x, -60f, 60f); // 위 아래 최대 각도 설정

            targetRot = Quaternion.Euler(euler);

            transform.rotation = targetRot;
        }
    }
}
