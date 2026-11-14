using UnityEngine;

public class BirdFollowCamera : MonoBehaviour
{
    public BirdFlightController target;
    public LayerMask obstructionLayers;

    [Header("Position")]
    public float distance = 8f;
    public float height = 4f;
    public float lookAhead = 4f;
    public float smoothTime = 0.18f;

    [Header("Shake")]
    public float speedShakeAmount = 0.09f;
    public float crashShakeAmount = 0.35f;

    private Vector3 followPosition;
    private Vector3 smoothVelocity;
    private Quaternion followRotation;
    private bool initialized;

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 birdPosition = target.transform.position;
        Quaternion yaw = Quaternion.Euler(0f, target.Yaw, 0f);

        Vector3 desired =
            birdPosition + yaw * new Vector3(0f, height, -distance);

        Vector3 aim =
            birdPosition + target.Heading * Vector3.forward * lookAhead;

        if (!initialized ||
            Vector3.Distance(followPosition, desired) > 30f)
        {
            followPosition = desired;
            smoothVelocity = Vector3.zero;
            followRotation = Quaternion.LookRotation(aim - desired);
            initialized = true;
        }

        followPosition = Vector3.SmoothDamp(
            followPosition,
            desired,
            ref smoothVelocity,
            Mathf.Max(0.01f, smoothTime));

        Vector3 pivot = birdPosition + Vector3.up * 0.35f;
        Vector3 offset = followPosition - pivot;
        Vector3 cameraPosition = followPosition;

        if (offset.magnitude > 0.01f &&
            Physics.SphereCast(
                pivot,
                0.2f,
                offset.normalized,
                out RaycastHit hit,
                offset.magnitude,
                obstructionLayers,
                QueryTriggerInteraction.Ignore))
        {
            cameraPosition =
                pivot + offset.normalized *
                Mathf.Max(0.05f, hit.distance - 0.05f);
        }

        Quaternion desiredRotation =
            Quaternion.LookRotation(aim - cameraPosition);

        followRotation = Quaternion.Slerp(
            followRotation,
            desiredRotation,
            1f - Mathf.Exp(-7f * Time.deltaTime));

        float speedShake =
            target.State == BirdFlightController.BirdState.Flying
            ? Mathf.InverseLerp(15f, 32f, target.Speed) * speedShakeAmount
            : 0f;

        float shake =
            speedShake + target.ImpactShake * crashShakeAmount;

        float t = Time.time * 22f;

        Vector3 noise = new Vector3(
            Mathf.PerlinNoise(t, 3.7f) - 0.5f,
            Mathf.PerlinNoise(9.2f, t) - 0.5f,
            0f) * 2f;





        transform.SetPositionAndRotation(
            cameraPosition + followRotation * noise * shake,
            followRotation);
    }
}