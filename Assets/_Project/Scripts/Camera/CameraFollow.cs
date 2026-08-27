using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Vector3 offset = new Vector3(0f, 12f, -8f);
    [SerializeField] private float followSpeed = 8f;

    private Transform target;

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 desirePosition = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, desirePosition, followSpeed * Time.deltaTime);
    }
}
