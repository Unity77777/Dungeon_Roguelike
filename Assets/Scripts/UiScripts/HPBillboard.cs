using UnityEngine;

public class HPBillboard : MonoBehaviour
{
    private Transform cam;

    void Start()
    {
        if (Camera.main != null)
            cam = Camera.main.transform;
    }

    void LateUpdate()
    {
        if (cam == null) return;

        // 월드 스페이스 Canvas + RectTransform용 LookAt
        Vector3 lookPos = transform.position + cam.rotation * Vector3.forward;
        Vector3 up = cam.rotation * Vector3.up;
        transform.rotation = Quaternion.LookRotation(lookPos - transform.position, up);
    }
}