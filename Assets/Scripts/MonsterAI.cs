using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class MonsterAI : MonoBehaviour
{
    public enum State { PATRULLANDO, INVESTIGANDO, PERSIGUIENDO }
    public enum PatrolMode { Waypoints, RandomInArea }

    [Header("Estado actual")]
    public State currentState = State.PATRULLANDO;

    [Header("Velocidades base")]
    [SerializeField] private float patrolSpeed = 3.5f;
    [SerializeField] private float investigateSpeed = 5f;
    [SerializeField] private float chaseSpeed = 6f;

    [Header("Ajustes dinámicos (modificables en menú)")]
    [Tooltip("Multiplicador global de velocidad (afecta todos los estados).")]
    [SerializeField, Range(0.5f, 3f)]
    private float movementSpeedMultiplier = 1f;

    [Tooltip("Tiempo de reacción antes de cambiar de estado (segundos).")]
    [SerializeField, Range(0f, 2f)]
    private float reactionTime = 0.5f;

    [Header("Navegación")]
    [SerializeField] private float arrivalThreshold = 0.8f;
    [SerializeField] private float maxSampleDistance = 4f;
    [SerializeField] private float stoppingDistance = 0.3f;

    [Header("Pausas de patrulla (dwell)")]
    [SerializeField] private float dwellTimeMin = 1.0f;
    [SerializeField] private float dwellTimeMax = 2.5f;

    [Header("Patrulla")]
    [SerializeField] private PatrolMode patrolMode = PatrolMode.Waypoints;
    [Tooltip("Si usas Waypoints, arrástralos aquí en orden.")]
    [SerializeField] private Transform[] waypoints;
    [Tooltip("Si usas RandomInArea, asigna un BoxCollider que delimite el área.")]
    [SerializeField] private BoxCollider patrolArea;
    [Tooltip("Si no hay BoxCollider, patrulla alrededor del punto de inicio en este radio (XZ).")]
    [SerializeField] private float fallbackRadius = 10f;

    [Header("Investigación - pausa en punto oído")]
    [SerializeField] private float investigateDwellMin = 5f;
    [SerializeField] private float investigateDwellMax = 7f;
    [SerializeField] private bool useRandomInvestigateDwell = true;

    [Header("Audio - Fuentes")]
    [SerializeField] private AudioSource loopSource;   // respiración / música de estado
    [SerializeField] private AudioSource sfxSource;    // pasos, gruñidos, rugidos

    [Header("Audio - Loops por estado")]
    [SerializeField] private AudioClip patrolLoop;
    [SerializeField] private AudioClip investigateLoop;
    [SerializeField] private AudioClip chaseLoop;

    [Header("Audio - SFX opcionales")]
    [SerializeField] private AudioClip[] patrolFootsteps;
    [SerializeField] private AudioClip[] chaseFootsteps;
    [SerializeField] private AudioClip[] investigateGrunts;
    [SerializeField] private AudioClip[] roarsOnChaseStart;

    // Internos
    private NavMeshAgent agent;
    private Animator animator;
    private int waypointIndex = 0;
    private bool isPatrolDwell = false;
    private bool isInvestigatingDwell = false;
    private float dwellTimer = 0f;
    private Vector3 startAnchor;
    private Transform target;
    private Vector3 lastKnownTargetPos = Vector3.zero;

    // Corrutina de reacción (para no apilar reacciones)
    private Coroutine reactionCoroutine;

    // Animator
    private static readonly int HashIsMoving = Animator.StringToHash("IsMoving");
    private static readonly int HashSpeed = Animator.StringToHash("Speed");

    // Propiedades para que las lea el menú / GameManager
    public float MovementSpeedMultiplier => movementSpeedMultiplier;
    public float ReactionTime => reactionTime;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        startAnchor = transform.position;

        if (agent)
        {
            agent.stoppingDistance = stoppingDistance;
            agent.autoBraking = true;
        }
    }

    void Start()
    {
        EnterPatrolState(true);
    }

    void Update()
    {
        // Animación de movimiento
        if (animator && agent != null)
        {
            float speed = agent.velocity.magnitude;
            animator.SetBool(HashIsMoving, speed > 0.02f);
            animator.SetFloat(HashSpeed, speed);
        }

        // Máquina de estados
        switch (currentState)
        {
            case State.PATRULLANDO:
                UpdatePatrol();
                break;
            case State.INVESTIGANDO:
                UpdateInvestigate();
                break;
            case State.PERSIGUIENDO:
                UpdateChase();
                break;
        }
    }

    // =======================
    //  PATRULLA
    // =======================
    private void EnterPatrolState(bool pickNewPoint)
    {
        currentState = State.PATRULLANDO;
        if (!agent || !agent.isOnNavMesh) return;

        isPatrolDwell = false;
        isInvestigatingDwell = false;

        agent.isStopped = false;
        UpdateAgentSpeedForCurrentState();

        PlayStateLoop(patrolLoop);

        if (pickNewPoint)
        {
            Vector3 next = GetNextPatrolPoint();
            SafeSetDestination(next);
        }
    }

    private void UpdatePatrol()
    {
        if (!agent || agent.pathPending) return;

        if (isPatrolDwell)
        {
            dwellTimer -= Time.deltaTime;
            if (dwellTimer <= 0f)
            {
                isPatrolDwell = false;
                Vector3 next = GetNextPatrolPoint();
                SafeSetDestination(next);
            }
            return;
        }

        if (agent.remainingDistance <= arrivalThreshold)
        {
            StartPatrolDwell();
        }
    }

    private void StartPatrolDwell()
    {
        isPatrolDwell = true;
        dwellTimer = Random.Range(dwellTimeMin, dwellTimeMax);

        if (agent && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }
    }

    private Vector3 GetNextPatrolPoint()
    {
        switch (patrolMode)
        {
            case PatrolMode.Waypoints:
                if (waypoints != null && waypoints.Length > 0)
                {
                    Vector3 p = waypoints[waypointIndex].position;
                    waypointIndex = (waypointIndex + 1) % waypoints.Length;
                    return ProjectToNavMesh(p, maxSampleDistance);
                }
                return GetRandomPointInArea();

            case PatrolMode.RandomInArea:
            default:
                return GetRandomPointInArea();
        }
    }

    private Vector3 GetRandomPointInArea()
    {
        if (patrolArea != null)
        {
            Vector3 half = patrolArea.size * 0.5f;
            Vector3 localRandom =
                patrolArea.center +
                new Vector3(
                    Random.Range(-half.x, half.x),
                    Random.Range(-half.y, half.y),
                    Random.Range(-half.z, half.z)
                );

            Vector3 worldCandidate = patrolArea.transform.TransformPoint(localRandom);
            return ProjectToNavMesh(worldCandidate, maxSampleDistance);
        }
        else
        {
            Vector2 rnd = Random.insideUnitCircle * fallbackRadius;
            Vector3 candidate = new Vector3(
                startAnchor.x + rnd.x,
                startAnchor.y,
                startAnchor.z + rnd.y
            );
            return ProjectToNavMesh(candidate, maxSampleDistance);
        }
    }

    private Vector3 ProjectToNavMesh(Vector3 candidate, float sampleDist)
    {
        if (NavMesh.SamplePosition(candidate, out var hit, sampleDist, NavMesh.AllAreas))
            return hit.position;

        if (NavMesh.SamplePosition(transform.position, out hit, sampleDist, NavMesh.AllAreas))
            return hit.position;

        return transform.position;
    }

    private void SafeSetDestination(Vector3 worldPos)
    {
        if (!agent || !agent.isActiveAndEnabled || !agent.isOnNavMesh) return;
        agent.isStopped = false;
        agent.SetDestination(worldPos);
    }

    // =======================
    //  INVESTIGACIÓN
    // =======================
    private void UpdateInvestigate()
    {
        if (!agent || agent.pathPending) return;

        if (isInvestigatingDwell)
        {
            dwellTimer -= Time.deltaTime;
            if (dwellTimer <= 0f)
            {
                isInvestigatingDwell = false;
                EnterPatrolState(true);
            }
            return;
        }

        if (agent.remainingDistance <= arrivalThreshold)
        {
            StartInvestigateDwell();
        }
    }

    private void StartInvestigateDwell()
    {
        isInvestigatingDwell = true;

        float t = useRandomInvestigateDwell
            ? Random.Range(investigateDwellMin, investigateDwellMax)
            : investigateDwellMin;

        dwellTimer = t;

        if (agent && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }
    }

    // Llamado internamente (tras tiempo de reacción) para ir a investigar
    private void GoToInvestigateState(Vector3 locationToInvestigate)
    {
        if (!agent || !agent.isOnNavMesh) return;

        currentState = State.INVESTIGANDO;
        isPatrolDwell = false;
        isInvestigatingDwell = false;

        agent.isStopped = false;
        UpdateAgentSpeedForCurrentState();

        PlayStateLoop(investigateLoop);

        Vector3 dest = ProjectToNavMesh(locationToInvestigate, maxSampleDistance);
        SafeSetDestination(dest);
    }

    // Interface pública que usará el sistema de oído
    public void OnHearNoise(Vector3 position)
    {
        if (reactionCoroutine != null)
            StopCoroutine(reactionCoroutine);

        reactionCoroutine = StartCoroutine(ReactionInvestigateCoroutine(position));
    }

    private IEnumerator ReactionInvestigateCoroutine(Vector3 position)
    {
        if (reactionTime > 0f)
            yield return new WaitForSeconds(reactionTime);

        GoToInvestigateState(position);
        reactionCoroutine = null;
    }

    // =======================
    //  PERSECUCIÓN
    // =======================
    private void UpdateChase()
    {
        if (!agent) return;

        if (target != null)
        {
            lastKnownTargetPos = target.position;
            SafeSetDestination(target.position);
        }
        else
        {
            if (lastKnownTargetPos != Vector3.zero)
                GoToInvestigateState(lastKnownTargetPos);
            else
                EnterPatrolState(true);
        }
    }

    // Llamado internamente (tras tiempo de reacción) para perseguir
    private void GoToChaseState(Transform chaseTarget)
    {
        if (!agent || !agent.isOnNavMesh) return;

        target = chaseTarget;
        currentState = State.PERSIGUIENDO;
        isPatrolDwell = false;
        isInvestigatingDwell = false;

        agent.isStopped = false;
        UpdateAgentSpeedForCurrentState();

        PlayStateLoop(chaseLoop);
        PlayRandomSFX(roarsOnChaseStart, 1.0f);

        if (target)
            SafeSetDestination(target.position);
    }

    // Interface pública que usará el sistema de visión
    public void OnSeePlayer(Transform player)
    {
        if (reactionCoroutine != null)
            StopCoroutine(reactionCoroutine);

        reactionCoroutine = StartCoroutine(ReactionChaseCoroutine(player));
    }

    private IEnumerator ReactionChaseCoroutine(Transform player)
    {
        if (reactionTime > 0f)
            yield return new WaitForSeconds(reactionTime);

        GoToChaseState(player);
        reactionCoroutine = null;
    }

    public void OnLosePlayer()
    {
        if (target != null)
            lastKnownTargetPos = target.position;

        target = null;
    }

    // =======================
    //  AUDIO
    // =======================
    #region Audio

    private void PlayStateLoop(AudioClip clip)
    {
        if (!loopSource) return;

        if (clip == null)
        {
            if (loopSource.isPlaying)
            {
                loopSource.Stop();
                loopSource.clip = null;
            }
            return;
        }

        if (loopSource.clip == clip && loopSource.isPlaying) return;

        loopSource.clip = clip;
        loopSource.loop = true;
        loopSource.Play();
    }

    private void PlayRandomSFX(AudioClip[] clips, float volume = 1f)
    {
        if (!sfxSource || clips == null || clips.Length == 0) return;

        int index = Random.Range(0, clips.Length);
        AudioClip clip = clips[index];
        if (!clip) return;

        sfxSource.pitch = Random.Range(0.95f, 1.05f);
        sfxSource.PlayOneShot(clip, volume);
    }

    public void OnFootstep()
    {
        switch (currentState)
        {
            case State.PATRULLANDO:
                PlayRandomSFX(patrolFootsteps, 0.7f);
                break;
            case State.PERSIGUIENDO:
                PlayRandomSFX(chaseFootsteps, 0.9f);
                break;
        }
    }

    public void OnInvestigateGrunt()
    {
        if (currentState == State.INVESTIGANDO)
        {
            PlayRandomSFX(investigateGrunts, 0.8f);
        }
    }

    #endregion

    // =======================
    //  AJUSTES DINÁMICOS PARA EL MENÚ
    // =======================

    /// <summary>
    /// Cambia el multiplicador global de velocidad. Llamado desde el menú.
    /// </summary>
    public void SetMovementSpeedMultiplier(float value)
    {
        movementSpeedMultiplier = value;
        UpdateAgentSpeedForCurrentState();
    }

    /// <summary>
    /// Cambia el tiempo de reacción ante estímulos. Llamado desde el menú.
    /// </summary>
    public void SetReactionTime(float value)
    {
        reactionTime = Mathf.Max(0f, value);
    }

    /// <summary>
    /// Aplica el multiplicador de velocidad según el estado actual.
    /// </summary>
    private void UpdateAgentSpeedForCurrentState()
    {
        if (!agent) return;

        float baseSpeed = patrolSpeed;

        switch (currentState)
        {
            case State.PATRULLANDO:
                baseSpeed = patrolSpeed;
                break;
            case State.INVESTIGANDO:
                baseSpeed = investigateSpeed;
                break;
            case State.PERSIGUIENDO:
                baseSpeed = chaseSpeed;
                break;
        }

        agent.speed = baseSpeed * movementSpeedMultiplier;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        // Área de patrulla
        Gizmos.color = Color.cyan;

        if (patrolArea != null)
        {
            Matrix4x4 old = Gizmos.matrix;
            Gizmos.matrix = patrolArea.transform.localToWorldMatrix;
            Gizmos.DrawWireCube(patrolArea.center, patrolArea.size);
            Gizmos.matrix = old;
        }
        else
        {
            Vector3 center = Application.isPlaying ? startAnchor : transform.position;
            Gizmos.DrawWireSphere(center, fallbackRadius);
        }
    }
#endif
}
