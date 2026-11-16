using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(BirdFlightController), typeof(Rigidbody))]
public class BirdPoop : MonoBehaviour
{
    public Material whiteMaterial;
    [Min(0.02f)] public float poopDiameter = 0.22f;
    [Min(0f)] public float downwardSpeed = 2f;
    [Min(0.05f)] public float cooldown = 0.35f;

    private BirdFlightController bird;
    private Rigidbody birdBody;
    private Collider[] birdColliders;
    private SphereCollider birdSphere;
    private float nextPoopTime;
    private bool dropPending;

    private void Awake()
    {
        bird = GetComponent<BirdFlightController>();
        birdBody = GetComponent<Rigidbody>();
        birdSphere = GetComponent<SphereCollider>();
        birdColliders = GetComponentsInChildren<Collider>();
    }

    private void Update()
    {
        if (Keyboard.current == null ||
            Cursor.lockState != CursorLockMode.Locked ||
            bird.State != BirdFlightController.BirdState.Flying)
            return;

        if (Keyboard.current.pKey.wasPressedThisFrame &&
            Time.time >= nextPoopTime)
        {
            dropPending = true;
            nextPoopTime = Time.time + cooldown;
        }
    }

    private void FixedUpdate()
    {
        if (!dropPending) return;
        dropPending = false;

        if (bird.State != BirdFlightController.BirdState.Flying) return;
        if (whiteMaterial == null)
        {
            Debug.LogWarning("Assign a white material to BirdPoop.", this);
            return;
        }

        float diameter = Mathf.Max(0.02f, poopDiameter);
        Bounds bounds = birdSphere.bounds;
        Vector3 spawnPosition = new Vector3(
            bounds.center.x,
            bounds.min.y - diameter * 0.5f - 0.05f,
            bounds.center.z);

        GameObject poop = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        poop.name = "Poop";
        poop.transform.position = spawnPosition;
        poop.transform.localScale = Vector3.one * diameter;
        poop.GetComponent<Renderer>().sharedMaterial = whiteMaterial;

        Collider poopCollider = poop.GetComponent<Collider>();
        foreach (Collider birdCollider in birdColliders)
        {
            if (birdCollider != null)
                Physics.IgnoreCollision(poopCollider, birdCollider);
        }

        Rigidbody body = poop.AddComponent<Rigidbody>();
        body.useGravity = true;
        body.interpolation = RigidbodyInterpolation.Interpolate;
        body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        body.linearVelocity = birdBody.linearVelocity + Vector3.down * downwardSpeed;

        PoopProjectile projectile = poop.AddComponent<PoopProjectile>();
        projectile.groundLayers = bird.groundLayers;
    }
}
