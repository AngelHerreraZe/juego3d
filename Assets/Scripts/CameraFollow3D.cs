using UnityEngine;

public class CameraFollow3D : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float fixedOffsetZ = -6.5f;
    [SerializeField] private float offsetX = 0.0f;
    [SerializeField] private float fixedHeightY = 3.5f;
    [SerializeField] private float smoothTimeX = 0.12f;
    [SerializeField] private float pitchAngle = 20.0f;

    private float currentVelocityX;

    void Start()
    {
        if (target == null)
        {
            PlayerBallController player = FindAnyObjectByType<PlayerBallController>();
            if (player != null) target = player.transform;
        }

        if (fixedHeightY == 0)
        {
            fixedHeightY = transform.position.y;
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        float targetX = target.position.x + offsetX;
        float newX = Mathf.SmoothDamp(transform.position.x, targetX, ref currentVelocityX, smoothTimeX);
        float newZ = target.position.z + fixedOffsetZ;
        float newY = fixedHeightY;

        transform.position = new Vector3(newX, newY, newZ);
        transform.rotation = Quaternion.Euler(pitchAngle, 0f, 0f);
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}
