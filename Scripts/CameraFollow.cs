using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float smoothSpeed = 6f;
    public Vector2 worldMin = new Vector2(-12f, -7f);
    public Vector2 worldMax = new Vector2(12f, 7f);

    private Camera cam;
    private float nextSearchTime;

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    private void LateUpdate()
    {
        if (cam == null)
        {
            return;
        }

        if (target == null && Time.time >= nextSearchTime)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                target = playerObject.transform;
            }

            nextSearchTime = Time.time + 1f;
        }

        if (target == null)
        {
            return;
        }

        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;

        float clampedX = Mathf.Clamp(target.position.x, worldMin.x + halfWidth, worldMax.x - halfWidth);
        float clampedY = Mathf.Clamp(target.position.y, worldMin.y + halfHeight, worldMax.y - halfHeight);

        Vector3 targetPosition = new Vector3(clampedX, clampedY, transform.position.z);
        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);
    }
}
