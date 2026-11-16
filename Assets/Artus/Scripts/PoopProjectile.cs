using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(SphereCollider))]
public class PoopProjectile : MonoBehaviour
{
    [Tooltip("Leave splat")]
    public bool restrictToGroundLayers = false;
    public LayerMask groundLayers;
    public float splatSize = 0.85f;
    public float splatLifetime = 8f;

    private const float Thickness = 0.025f;
    private bool splashed;
    private float splashTime;
    private float expiresAt;
    private Vector3 startScale;
    private Vector3 endScale;

    private void Awake()
    {
        // also remove projectiles that miss the ground.
        expiresAt = Time.time + 20f;
    }

    private void Update()
    {
        if (Time.time >= expiresAt)
        {
            Destroy(gameObject);
            return;
        }

        if (!splashed) return;

        float progress = Mathf.Clamp01((Time.time - splashTime) / 0.12f);
        float eased = 1f - (1f - progress) * (1f - progress);
        transform.localScale = Vector3.Lerp(startScale, endScale, eased);
    }

    private void OnCollisionEnter(Collision collision)
    {
        CheckForFloor(collision);
    }

    private void OnCollisionStay(Collision collision)
    {
        CheckForFloor(collision);
    }

    private void CheckForFloor(Collision collision)
    {
        if (splashed) return;

        // another falling poop sphere isnt a floor
        if (collision.rigidbody != null &&
            collision.rigidbody.GetComponent<PoopProjectile>() != null)
            return;

        if (restrictToGroundLayers &&
            (groundLayers.value & (1 << collision.collider.gameObject.layer)) == 0)
            return;

        for (int i = 0; i < collision.contactCount; i++)
        {
            ContactPoint contact = collision.GetContact(i);
            if (contact.normal.y <= 0.5f) continue;

            Splash(contact);
            break;
        }
    }

    private void Splash(ContactPoint contact)
    {
        splashed = true;
        splashTime = Time.time;
        expiresAt = Time.time + splatLifetime;

        // the falling sphere becomes a flattened splat.
        GetComponent<Collider>().enabled = false;
        Rigidbody body = GetComponent<Rigidbody>();
        body.interpolation = RigidbodyInterpolation.None;
        body.collisionDetectionMode = CollisionDetectionMode.Discrete;
        body.isKinematic = true;

        transform.SetPositionAndRotation(
            contact.point + contact.normal * (Thickness * 0.5f + 0.003f),
            Quaternion.FromToRotation(Vector3.up, contact.normal));

        startScale = new Vector3(0.12f, Thickness, 0.12f);
        endScale = new Vector3(splatSize, Thickness, splatSize);
        transform.localScale = startScale;

        Material material = GetComponent<Renderer>().sharedMaterial;

        // SMALLER SPOTS
        for (int i = 0; i < 7; i++)
        {
            float angle = (i / 7f) * Mathf.PI * 2f + Random.Range(-0.2f, 0.2f);
            float radius = Random.Range(0.5f, 0.85f);
            float size = Random.Range(0.12f, 0.25f);

            GameObject spot = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            spot.name = "Splat spot";
            Collider spotCollider = spot.GetComponent<Collider>();
            spotCollider.enabled = false;
            Destroy(spotCollider);
            spot.GetComponent<Renderer>().sharedMaterial = material;

            spot.transform.SetParent(transform, false);
            spot.transform.localPosition = new Vector3(
                Mathf.Cos(angle) * radius, -0.15f, Mathf.Sin(angle) * radius);
            spot.transform.localScale = new Vector3(size, 0.7f, size);
        }
    }
}
