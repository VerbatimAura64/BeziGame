using UnityEngine;
using UnityEngine.AI;

/// <summary>NavMesh-driven AI that chases the nearest non-eliminated rival wagon and rams it,
/// reusing the same Rigidbody-based movement feel as <see cref="HorseWagonController"/>.</summary>
public sealed class WagonAIController : MonoBehaviour
{
    private const float InputDeadZone = 0.01f;

    [Header("Targeting")]
    [SerializeField] private float retargetIntervalSeconds = 1f;
    [SerializeField] private float ramDetectionRange = 6f;

    [Header("Movement")]
    [SerializeField] private float maximumSpeed = 26f;
    [SerializeField] private float turnSpeed = 60f;
    [SerializeField] private float acceleration = 14f;
    [SerializeField] private float braking = 20f;

    private Rigidbody wagonRigidbody;
    [SerializeField]private Animator wagonAnimator;
    private NavMeshAgent navMeshAgent;
    [SerializeField] private WagonHealth ownHealth;
    private float retargetTimer;
    private Vector3 steeringTarget;
    private float throttle;

    /// <summary>The rival wagon this AI is currently chasing, or null if none are alive.</summary>
    public WagonHealth Target { get; private set; }

    private void Awake()
    {
        wagonRigidbody = GetComponent<Rigidbody>();
        wagonAnimator = GetComponent<Animator>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        ownHealth = GetComponent<WagonHealth>();

        if (navMeshAgent != null)
        {
            navMeshAgent.updatePosition = false;
            navMeshAgent.updateRotation = false;
        }

        if (ownHealth != null)
        {
            ownHealth.Eliminated += HandleOwnElimination;
        }
    }

    private void OnDestroy()
    {
        if (ownHealth != null)
        {
            ownHealth.Eliminated -= HandleOwnElimination;
        }
    }

    private void Update()
    {
        retargetTimer -= Time.deltaTime;
        if (retargetTimer <= 0f)
        {
            retargetTimer = retargetIntervalSeconds;
            Retarget();
        }

        if (Target != null && navMeshAgent != null && navMeshAgent.isOnNavMesh)
        {
            navMeshAgent.SetDestination(Target.transform.position);
        }

        float steering = ComputeSteering(steeringTarget);
        if (Mathf.Abs(steering) > InputDeadZone)
        {
            float direction = Mathf.Abs(throttle) > InputDeadZone ? Mathf.Sign(throttle) : 1f;
            float turnAmount = steering * direction * turnSpeed * Time.fixedDeltaTime;
            wagonRigidbody.MoveRotation(wagonRigidbody.rotation * Quaternion.Euler(0f, turnAmount, 0f));
        }
    }

    private void FixedUpdate()
    {
        if (wagonRigidbody == null || navMeshAgent == null)
        {
            return;
        }

        steeringTarget = navMeshAgent.desiredVelocity;
        bool isRamming = Target != null && Vector3.Distance(transform.position, Target.transform.position) <= ramDetectionRange;
        if (isRamming)
        {
            steeringTarget = Target.transform.position - transform.position;
        }

        throttle = ComputeThrottle(steeringTarget);
        float steering = ComputeSteering(steeringTarget);

        Vector3 currentPlanarVelocity = new Vector3(wagonRigidbody.linearVelocity.x, 0f, wagonRigidbody.linearVelocity.z);
        Vector3 targetPlanarVelocity = transform.forward * (throttle * maximumSpeed);
        float speedChange = Mathf.Abs(throttle) > InputDeadZone ? acceleration : braking;
        Vector3 nextPlanarVelocity = Vector3.MoveTowards(
            currentPlanarVelocity,
            targetPlanarVelocity,
            speedChange * Time.fixedDeltaTime);

        wagonRigidbody.linearVelocity = new Vector3(
            nextPlanarVelocity.x,
            wagonRigidbody.linearVelocity.y,
            nextPlanarVelocity.z);

        if (Mathf.Abs(steering) > InputDeadZone)
        {
            float direction = Mathf.Abs(throttle) > InputDeadZone ? Mathf.Sign(throttle) : 1f;
            float turnAmount = steering * direction * turnSpeed * Time.fixedDeltaTime;
            wagonRigidbody.MoveRotation(wagonRigidbody.rotation * Quaternion.Euler(0f, turnAmount, 0f));
        }

        //UpdateAnimator(throttle, steering);

        if (navMeshAgent.isOnNavMesh)
        {
            navMeshAgent.nextPosition = transform.position;
        }
    }

    /// <summary>Sets the wagon this AI should chase and ram.</summary>
    public void SetTarget(WagonHealth target)
    {
        Target = target;
    }

    private void Retarget()
    {
        WagonHealth[] allWagons = FindObjectsByType<WagonHealth>(FindObjectsSortMode.None);
        WagonHealth nearest = null;
        float nearestDistanceSquared = float.MaxValue;

        foreach (WagonHealth candidate in allWagons)
        {
            if (candidate == ownHealth || candidate.IsEliminated)
            {
                continue;
            }

            float distanceSquared = (candidate.transform.position - transform.position).sqrMagnitude;
            if (distanceSquared < nearestDistanceSquared)
            {
                nearestDistanceSquared = distanceSquared;
                nearest = candidate;
            }
        }

        SetTarget(nearest);
    }

    private float ComputeThrottle(Vector3 desiredDirection)
    {
        if (desiredDirection.sqrMagnitude < InputDeadZone * InputDeadZone)
        {
            return 0f;
        }

        return Mathf.Clamp(Vector3.Dot(transform.forward, desiredDirection.normalized), -1f, 1f);
    }

    private float ComputeSteering(Vector3 desiredDirection)
    {
        if (desiredDirection.sqrMagnitude < InputDeadZone * InputDeadZone)
        {
            return 0f;
        }

        float signedAngle = Vector3.SignedAngle(transform.forward, desiredDirection, Vector3.up);
        return Mathf.Clamp(signedAngle / 90f, -1f, 1f);
    }

    private void UpdateAnimator(float throttle, float steering)
    {
        if (wagonAnimator == null)
        {
            return;
        }

        foreach (AnimatorControllerParameter parameter in wagonAnimator.parameters)
        {
            if (parameter.type == AnimatorControllerParameterType.Float && parameter.name == "Vertical")
            {
                wagonAnimator.SetFloat(parameter.name, throttle);
            }
            else if (parameter.type == AnimatorControllerParameterType.Float && parameter.name == "Horizontal")
            {
                wagonAnimator.SetFloat(parameter.name, steering);
            }
        }
    }

    private void HandleOwnElimination(WagonHealth eliminatedWagon)
    {
        if (navMeshAgent != null)
        {
            navMeshAgent.enabled = false;
        }

        enabled = false;
    }
}
