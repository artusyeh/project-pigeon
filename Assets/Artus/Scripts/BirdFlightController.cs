using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody), typeof(SphereCollider))]
public class BirdFlightController : MonoBehaviour
{
    // bird states
    public enum BirdState { Grounded, TakingOff, Flying, Stunned }


    [Header("WASD movement")]
    public float movementAcceleration = 15f;

    private Vector2 moveInput;
    
    [Header("Ground and takeoff")]
    public LayerMask groundLayers;
    public float chargeTime = 0.8f;
    public float beatTolerance = 0.16f;
    public float launchForwardSpeed = 12f;
    public float launchUpSpeed = 7f;

    [Header("Flight")]
    public float mouseSensitivity = 0.12f;
    public float turnStrength = 2.5f;
    public float glideSpeed = 10f;
    public float maxSpeed = 35f;
    public float flapUpBoost = 2.5f;
    public float flapForwardBoost = 1.8f;

    [Header("Landing")]
    public float hardImpactSpeed = 8f;
    public float hardLandingTotalSpeed = 18f;
    public float stunTime = 2.5f;

    public BirdState State { get; private set; }
    public float Speed => body.linearVelocity.magnitude;
    public float Yaw => yaw;
    public Quaternion Heading => Quaternion.Euler(-pitch, yaw, 0f);
    public float ImpactShake { get; private set; }

    private Rigidbody body;
    private SphereCollider sphere;
    private Vector3 startPosition;
    private float startYaw, yaw, pitch, charge, lastBeat;
    private float stunUntil, ignoreGroundUntil;
    private int beats, pendingFlaps;
    private bool launchPending;
    // private string message = "Hold Space to prepare takeoff.";
    private readonly float[] beatGaps = { 0.8f, 0.6f, 0.4f };

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        sphere = GetComponent<SphereCollider>();

        body.useGravity = false;
        body.isKinematic = false;
        body.linearDamping = 0f;
        body.constraints = RigidbodyConstraints.FreezeRotation;
        body.interpolation = RigidbodyInterpolation.Interpolate;
        body.collisionDetectionMode =
            CollisionDetectionMode.ContinuousDynamic;

        startPosition = body.position;
        startYaw = yaw = transform.eulerAngles.y;
        State = BirdState.Grounded;
    }

private void Update()
{
    ImpactShake = Mathf.MoveTowards(
        ImpactShake, 0f, Time.deltaTime * 2f);

    Keyboard keys = Keyboard.current;
    Mouse mouse = Mouse.current;

    moveInput = Vector2.zero;

    if (keys == null || mouse == null) return;


    if (keys.escapeKey.wasPressedThisFrame)
        LockMouse(false);
    else if (mouse.leftButton.wasPressedThisFrame)
        LockMouse(true);

    if (keys.rKey.wasPressedThisFrame)
        ResetBird();

    if (Cursor.lockState != CursorLockMode.Locked)
    {
        if (State == BirdState.TakingOff)
            CancelTakeoff();

        return;
    }

    if (State == BirdState.Stunned) return;

    // WASD controls movement, not rotation.
    if (keys.wKey.isPressed) moveInput.y += 1f;
    if (keys.sKey.isPressed) moveInput.y -= 1f;
    if (keys.aKey.isPressed) moveInput.x -= 1f;
    if (keys.dKey.isPressed) moveInput.x += 1f;


    moveInput = Vector2.ClampMagnitude(moveInput, 1f);

    // mouse controls where the bird faces.
    Vector2 mouseDelta = mouse.delta.ReadValue();
    yaw += mouseDelta.x * mouseSensitivity;

    if (State == BirdState.Flying)
    {
        pitch = Mathf.Clamp(
            pitch + mouseDelta.y * mouseSensitivity,
            -70f,
            70f);

        if (keys.fKey.wasPressedThisFrame)
            pendingFlaps++;

        return;
    }

    // hold Space to prepare takeoff.
    if (!keys.spaceKey.isPressed)
    {
        if (State == BirdState.TakingOff)
            CancelTakeoff();

        return;
    }

    State = BirdState.TakingOff;
    charge += Time.deltaTime;

    if (charge < chargeTime || launchPending) return;

    if (beats > 0 &&
        Time.time - lastBeat >
        beatGaps[beats - 1] + beatTolerance)
    {
        beats = 0;
    }

    if (!keys.fKey.wasPressedThisFrame) return;

    if (beats == 0)
    {
        beats = 1;
        lastBeat = Time.time;
        return;
    }

    float timingError = Mathf.Abs(
        Time.time - lastBeat - beatGaps[beats - 1]);

    if (timingError > beatTolerance)
    {
        beats = 0;
        return;
    }

    beats++;
    lastBeat = Time.time;

    if (beats == 4)
        launchPending = true;
}

    private void FixedUpdate()
    {
        if (body.position.y < -80f)
        {
            ResetBird();
            return;
        }

        if (State == BirdState.Stunned && Time.time >= stunUntil)
        {
            State = GroundBelow()
                ? BirdState.Grounded
                : BirdState.Flying;
        }

        if ((State == BirdState.Grounded ||
             State == BirdState.TakingOff) && !GroundBelow())
        {
            State = BirdState.Flying;
            launchPending = false;
        }

        if (launchPending)
        {
            launchPending = false;
            State = BirdState.Flying;
            pitch = 15f;
            ignoreGroundUntil = Time.time + 0.2f;

            Vector3 forward =
                Quaternion.Euler(0f, yaw, 0f) * Vector3.forward;

            Vector3 launch =
                forward * launchForwardSpeed +
                Vector3.up * launchUpSpeed;

            body.AddForce(
                launch - body.linearVelocity,
                ForceMode.VelocityChange);

            return;
        }

        if (State == BirdState.Flying)
        {
            Vector3 velocity = body.linearVelocity;
            float speed = velocity.magnitude;
            Vector3 forward = Heading * Vector3.forward;

            Vector3 direction =
                speed > 0.01f ? velocity / speed : forward;

            // steer across the airflow instead of adding forward thrust.
            Vector3 steering =
                (forward * speed - velocity) * turnStrength;

            steering = Vector3.ProjectOnPlane(steering, direction);
            steering = Vector3.ClampMagnitude(steering, 24f);

            // lift weakens when the bird loses speed.
            float liftRatio = Mathf.Clamp01(
                speed / Mathf.Max(0.1f, glideSpeed));

            Vector3 lift =
                Vector3.ProjectOnPlane(-Physics.gravity, direction)
                * liftRatio * liftRatio * 0.9f;

            body.AddForce(
                Physics.gravity + lift + steering - velocity * 0.025f,
                ForceMode.Acceleration);

            if (pendingFlaps > 0)
            {
                body.AddForce(
                    (Vector3.up * flapUpBoost +
                     forward * flapForwardBoost) * pendingFlaps,
                    ForceMode.VelocityChange);

                pendingFlaps = 0;
            }

            if (speed > maxSpeed)
                body.linearVelocity = velocity.normalized * maxSpeed;
        }
        else
        {
            body.AddForce(Physics.gravity, ForceMode.Acceleration);

            if (GroundBelow())
            {
                Vector3 horizontal = Vector3.ProjectOnPlane(
                    body.linearVelocity, Vector3.up);

                body.AddForce(
                    Vector3.ClampMagnitude(
                        -horizontal / Time.fixedDeltaTime, 20f),
                    ForceMode.Acceleration);
            }
        }
    }

    private bool GroundBelow()
    {
        float radius =
            sphere.radius * Mathf.Abs(transform.lossyScale.y);

        return Physics.Raycast(
            transform.TransformPoint(sphere.center),
            Vector3.down,
            out RaycastHit hit,
            radius + 0.12f,
            groundLayers,
            QueryTriggerInteraction.Ignore)
            && hit.normal.y > 0.55f;
    }

    private void OnCollisionEnter(Collision collision)
    {
        CheckLanding(collision);
    }

    private void OnCollisionStay(Collision collision)
    {
        CheckLanding(collision);
    }

    private void CheckLanding(Collision collision)
    {
        if (State != BirdState.Flying ||
            Time.time < ignoreGroundUntil) return;

        bool groundLayer =
            (groundLayers.value & (1 << collision.gameObject.layer)) != 0;

        bool landed = false;
        float impact = 0f;

        for (int i = 0; i < collision.contactCount; i++)
        {
            Vector3 normal = collision.GetContact(i).normal;

            if (groundLayer && normal.y > 0.55f)
                landed = true;

            impact = Mathf.Max(
                impact,
                Mathf.Abs(Vector3.Dot(
                    collision.relativeVelocity, normal)));
        }

        bool crashed =
            impact >= hardImpactSpeed ||
            (landed &&
             collision.relativeVelocity.magnitude >=
             hardLandingTotalSpeed);

        if (!landed && !crashed) return;

        State = crashed ? BirdState.Stunned : BirdState.Grounded;

        if (crashed)
        {
            stunUntil = Time.time + stunTime;
            ImpactShake = 1f;
        }

        pitch = charge = 0f;
        beats = pendingFlaps = 0;
        launchPending = false;
    }

    private void CancelTakeoff()
    {
        State = BirdState.Grounded;
        charge = 0f;
        beats = 0;
        launchPending = false;
    }

    private void ResetBird()
    {
        body.position = startPosition;
        body.linearVelocity = Vector3.zero;
        body.angularVelocity = Vector3.zero;

        yaw = startYaw;
        pitch = ImpactShake = 0f;
        pendingFlaps = 0;
        ignoreGroundUntil = 0f;

        CancelTakeoff();
    }

    private void LockMouse(bool locked)
    {
        Cursor.lockState =
            locked ? CursorLockMode.Locked : CursorLockMode.None;

        Cursor.visible = !locked;
    }

    private void OnDisable()
    {
        LockMouse(false);
    }

    private void OnApplicationFocus(bool focused)
    {
        if (!focused)
        {
            LockMouse(false);
            pendingFlaps = 0;

            if (State == BirdState.TakingOff)
                CancelTakeoff();
        }
    }
}