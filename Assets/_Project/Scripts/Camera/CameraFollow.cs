using UnityEngine;
using UnityEngine.InputSystem;

public class CameraFollow : MonoBehaviour
{
    [Header("추적")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 12f, -8f);
    [SerializeField] private float followSpeed = 8f;

    [Header("줌")]
    [SerializeField] private float initialZoom = 1f;
    [SerializeField] private float zoomSpeed = 10f;
    [SerializeField] private float minZoom = 0.5f;
    [SerializeField] private float maxZoom = 1.0f;

    private Transform target;
    private float zoomLevel = 1f;

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    private void Awake()
    {
        // 씬마다 다른 기본 줌으로 시작(씬 이름 조건문 없이 인스펙터 값 차이로 처리)
        zoomLevel = initialZoom;
    }

    private void Update()
    {
        Mouse mouse = Mouse.current;
        if (mouse == null) return;

        float scroll = mouse.scroll.ReadValue().y;
        if (scroll != 0f)
        {
            zoomLevel -= scroll * zoomSpeed * 0.01f;
            // 휠을 위로(양수) 밀면 zoomLevel 감소 -> offset이 작아짐 ->  카메라가 가까워짐(줌인)
            zoomLevel = Mathf.Clamp(zoomLevel, minZoom, maxZoom);
        }
    }

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 desirePosition = target.position + offset * zoomLevel;
        transform.position = Vector3.Lerp(transform.position, desirePosition, followSpeed * Time.deltaTime);
    }
}
