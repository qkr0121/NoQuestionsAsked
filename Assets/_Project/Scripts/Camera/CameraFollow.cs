using UnityEngine;
using UnityEngine.InputSystem;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Vector3 offset = new Vector3(0f, 12f, -8f);
    [SerializeField] private float followSpeed = 8f;

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
        zoomLevel = initialZoom;
    }

    private void Update()
    {
        Mouse mouse = Mouse.current;
        if (mouse == null) return;

        float scroll = mouse.scroll.ReadValue().y;
        if (scroll != 0f)
        {
            zoomLevel -= scroll * zoomSpeed * Time.deltaTime;
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
