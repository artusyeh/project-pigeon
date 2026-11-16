using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody), typeof(SphereCollider))]
public class BirdFlightController : MonoBehaviour
{
    public enum BirdState { Grounded, TakingOff, Flying, Stunned, noMove }


    [Header("WASD movement")]
    public float movementAcceleration = 15f;

    private Vector2 moveInput;
    
    [HideInInspector] public LayerMask groundLayers;

    [Header("Floor Impact Sound")]
    public AudioClip floorHitSFX;
    [Range(0f, 1f)] public float floorHitVolume = 1f;

    private AudioSource floorHitAudioSource;
    
    [Header("Ground and takeoff")]
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

    [Header("Flight Bounds")]
    public float maxFlightHeight = 50f;

    [Header("Flying Sound")]
    public AudioClip flyingSFX;
    [Range(0f, 1f)] public float flyingVolume = 1f;

    public BirdState State { get; private set; }
    public float Speed => body.linearVelocity.magnitude;
    public float Yaw => yaw;
    public Quaternion Heading => Quaternion.Euler(-pitch, yaw, 0f);
    public float ImpactShake { get; private set; }

    private Rigidbody body;
    private AudioSource flyingAudioSource;
    private SphereCollider sphere;
    private Vector3 startPosition;
    private float startYaw, yaw, pitch;
    private float stunUntil, ignoreGroundUntil;
    private int pendingFlaps;
    private bool launchPending;
    private readonly HashSet<Collider> groundContacts = new HashSet<Collider>();

    private BirdState stateBeforeNoMove;
    private Vector3 velocityBeforeNoMove;
    private Vector3 angularVelocityBeforeNoMove;
    private bool kinematicBeforeNoMove;
    private RigidbodyInterpolation interpolationBeforeNoMove;
    private CollisionDetectionMode collisionModeBeforeNoMove;
    private float noMoveStartedAt;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        sphere = GetComponent<SphereCollider>();


        flyingAudioSource = gameObject.AddComponent<AudioSource>();
        flyingAudioSource.playOnAwake = false;
        flyingAudioSource.loop = true;
        flyingAudioSource.spatialBlend = 0f;

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


        floorHitAudioSource = gameObject.AddComponent<AudioSource>();
        floorHitAudioSource.playOnAwake = false;
        floorHitAudioSource.loop = false;
        floorHitAudioSource.spatialBlend = 0f;
    }

private void Update()
{
    if (State != BirdState.noMove)
    {
        ImpactShake = Mathf.MoveTowards(
            ImpactShake, 0f, Time.deltaTime * 2f);
    }

    Keyboard keys = Keyboard.current;
    Mouse mouse = Mouse.current;

    moveInput = Vector2.zero;

    if (keys == null || mouse == null) return;


    if (keys.escapeKey.wasPressedThisFrame)
        LockMouse(false);
    else if (mouse.leftButton.wasPressedThisFrame)
        LockMouse(true);


    if (State == BirdState.noMove) return;

    if (keys.rKey.wasPressedThisFrame)
        ResetBird();

    if (Cursor.lockState != CursorLockMode.Locked)
    {
        launchPending = false;
        pendingFlaps = 0;
        return;
    }

    if (State == BirdState.Stunned) return;

    // WASD controls movement, not rotation.
    if (keys.wKey.isPressed) moveInput.y += 1f;
    if (keys.sKey.isPressed) moveInput.y -= 1f;
    if (keys.aKey.isPressed) moveInput.x -= 1f;
    if (keys.dKey.isPressed) moveInput.x += 1f;


    moveInput = Vector2.ClampMagnitude(moveInput, 1f);

    // mouse controls where the bird faces
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

    // one F press takes off from the ground; Space is no longer used
    if (State == BirdState.Grounded && keys.fKey.wasPressedThisFrame)
        launchPending = true;
}

    private void FixedUpdate()
    {
        // the kinematic Rigidbody stays frozen without gravity or flight forces
        if (State == BirdState.noMove) return;

        if (body.position.y < -80f)
        {
            ResetBird();
            return;
        }

        bool grounded = Time.time >= ignoreGroundUntil && GroundBelow();

        // ground contact must never cancel an active crash stun
        if (State == BirdState.Stunned)
        {
            if (Time.time >= stunUntil)
                State = grounded ? BirdState.Grounded : BirdState.Flying;
        }
        else
        {
            State = grounded ? BirdState.Grounded : BirdState.Flying;
            if (grounded) pitch = 0f;
        }

        if (launchPending)
        {
            launchPending = false;
            if (State == BirdState.Grounded)
            {
                State = BirdState.Flying;
                pitch = 15f;
                ignoreGroundUntil = Time.time + 0.2f;
                groundContacts.Clear();

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

            if (grounded)
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

    private void LateUpdate()
    {
        UpdateFlyingSound();
    }

    private void UpdateFlyingSound()
    {
        if (flyingAudioSource == null) return;

        flyingAudioSource.volume = flyingVolume;

        // allow the clip to be changed or cleared in the Inspector.
        if (flyingAudioSource.clip != flyingSFX)
        {
            flyingAudioSource.Stop();
            flyingAudioSource.clip = flyingSFX;
        }

        bool shouldPlay = isActiveAndEnabled &&
            State == BirdState.Flying && flyingSFX != null;

        if (shouldPlay)
        {
            if (!flyingAudioSource.isPlaying)
                flyingAudioSource.Play();
        }
        else if (flyingAudioSource.isPlaying)
        {
            flyingAudioSource.Stop();
        }
    }

    private bool GroundBelow()
    {
        groundContacts.RemoveWhere(other => other == null || !other.enabled ||
            !other.gameObject.activeInHierarchy || other.isTrigger);
        if (groundContacts.Count > 0) return true;

        if (body.linearVelocity.y > 0.5f) return false;

        Bounds bounds = sphere.bounds;
        RaycastHit[] hits = Physics.RaycastAll(
            bounds.center,
            Vector3.down,
            bounds.extents.y + 0.03f,
            Physics.AllLayers,
            QueryTriggerInteraction.Ignore);

        float nearestDistance = float.PositiveInfinity;
        bool foundGround = false;


        foreach (RaycastHit hit in hits)
        {
            Collider other = hit.collider;
            if (other.attachedRigidbody == body ||
                other.transform.IsChildOf(transform) ||
                Physics.GetIgnoreCollision(sphere, other))
                continue;

            if (hit.distance < nearestDistance)
            {
                nearestDistance = hit.distance;
                foundGround = hit.normal.y > 0.55f;
            }
        }

        return foundGround;
    }

    private void OnCollisionEnter(Collision collision)
    {
        CheckLanding(collision, true);
    }

    private void OnCollisionStay(Collision collision)
    {
        CheckLanding(collision, false);
    }

    private void OnCollisionExit(Collision collision)
    {
        groundContacts.Remove(collision.collider);
    }

    private void CheckLanding(Collision collision, bool newImpact)
    {
        if (State == BirdState.noMove) return;

        bool landed = false;
        float impact = 0f;

        for (int i = 0; i < collision.contactCount; i++)
        {
            Vector3 normal = collision.GetContact(i).normal;

            if (normal.y > 0.55f)
                landed = true;

            impact = Mathf.Max(
                impact,
                Mathf.Abs(Vector3.Dot(
                    collision.relativeVelocity, normal)));
        }

        if (landed)
            groundContacts.Add(collision.collider);
        else
            groundContacts.Remove(collision.collider);

        // keep tracking the floor while stunned, without restarting the timer
        if (State == BirdState.Stunned) return;

        // let takeoff clear the floordwad
        if (landed && Time.time < ignoreGroundUntil && body.linearVelocity.y > 0f)
            return;

        bool crashed = newImpact && (
            impact >= hardImpactSpeed ||
            (landed &&
             collision.relativeVelocity.magnitude >=
             hardLandingTotalSpeed));

        if (!landed && !crashed) return;

        State = crashed ? BirdState.Stunned : BirdState.Grounded;

        if (crashed)
        {
            stunUntil = Time.time + stunTime;
            ImpactShake = 1f;
        }

        pitch = 0f;
        pendingFlaps = 0;
        launchPending = false;

        if (landed && newImpact && floorHitSFX != null)
        {
            floorHitAudioSource.PlayOneShot(floorHitSFX, floorHitVolume);
        }
    }



    /// freezes the bird, or resumes its previous state and momentum
    /// call this method instead of assigning State directly
    public void SetNoMove(bool frozen)
    {
        if (body == null) return;

        if (frozen)
        {
            // repeated freeze requests must not overwrite the saved state.
            if (State == BirdState.noMove) return;

            stateBeforeNoMove = State;
            velocityBeforeNoMove = body.linearVelocity;
            angularVelocityBeforeNoMove = body.angularVelocity;
            kinematicBeforeNoMove = body.isKinematic;
            interpolationBeforeNoMove = body.interpolation;
            collisionModeBeforeNoMove = body.collisionDetectionMode;
            noMoveStartedAt = Time.time;

            State = BirdState.noMove;
            moveInput = Vector2.zero;

            if (!body.isKinematic)
            {
                body.linearVelocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
            }

            body.interpolation = RigidbodyInterpolation.None;
            body.collisionDetectionMode = CollisionDetectionMode.Discrete;
            body.isKinematic = true;
        }
        else
        {
            if (State != BirdState.noMove) return;

            float pausedDuration = Time.time - noMoveStartedAt;
            stunUntil += pausedDuration;
            ignoreGroundUntil += pausedDuration;

            body.isKinematic = kinematicBeforeNoMove;
            body.collisionDetectionMode = collisionModeBeforeNoMove;
            body.interpolation = interpolationBeforeNoMove;

            if (!body.isKinematic)
            {
                body.linearVelocity = velocityBeforeNoMove;
                body.angularVelocity = angularVelocityBeforeNoMove;
                body.WakeUp();
            }

            State = stateBeforeNoMove;
            groundContacts.Clear();
        }

        // Uqdqwdqw
        UpdateFlyingSound();
    }

    [ContextMenu("Freeze Bird (noMove)")]
    public void EnterNoMove()
    {
        SetNoMove(true);
    }

    [ContextMenu("Resume Bird")]
    public void ExitNoMove()
    {
        SetNoMove(false);
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
        stunUntil = 0f;
        launchPending = false;
        groundContacts.Clear();
        State = BirdState.Grounded;
    }

    private void LockMouse(bool locked)
    {
        Cursor.lockState =
            locked ? CursorLockMode.Locked : CursorLockMode.None;

        Cursor.visible = !locked;
    }

    private void OnDisable()
    {
        if (flyingAudioSource != null)
            flyingAudioSource.Stop();

        LockMouse(false);
        groundContacts.Clear();
    }

    private void OnDestroy()
    {
        if (flyingAudioSource != null)
            Destroy(flyingAudioSource);
    }

    private void OnApplicationFocus(bool focused)
    {
        if (!focused)
        {
            LockMouse(false);
            if (State == BirdState.noMove) return;

            pendingFlaps = 0;
            launchPending = false;
        }
    }
}
