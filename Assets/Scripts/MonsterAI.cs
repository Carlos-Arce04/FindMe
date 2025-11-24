using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class MonsterAI : MonoBehaviour
{
    public enum State { PATRULLANDO, INVESTIGANDO, PERSIGUIENDO }
    public enum PatrolMode { Waypoints, RandomInArea }

    [Header("Estado actual")]
    public State currentState = State.PATRULLANDO;

    [Header("Velocidades")]
    [SerializeField] private float patrolSpeed = 3.5f;
    [SerializeField] private float investigateSpeed = 5f;
    [SerializeField] private float chaseSpeed = 6f;

    [Header("Navegación")]
    [SerializeField] private float arrivalThreshold = 0.8f;  // distancia para considerar "llegó"
    [SerializeField] private float maxSampleDistance = 4f;   // radio para ajustar puntos al NavMesh
    [SerializeField] private float stoppingDistance = 0.3f;

    [Header("Pausas de patrulla (dwell)")]
    [SerializeField] private float dwellTimeMin = 1.0f;
    [SerializeField] private float dwellTimeMax = 2.5f;

    [Header("Patrulla")]
    [SerializeField] private PatrolMode patrolMode = PatrolMode.Waypoints;
    [Tooltip("Si usas Waypoints, arrástralos aquí en orden.")]
    [SerializeField] private Transform[] waypoints;
    [Tooltip("Si usas RandomInArea, asigna un BoxCollider que delimite el área.")]
    [SerializeField] private BoxCollider patrolArea; // opcional
    [Tooltip("Si no hay BoxCollider, patrulla alrededor del punto de inicio en este radio (XZ).")]
    [SerializeField] private float fallbackRadius = 10f;

    [Header("Investigación - pausa en punto oído")]
    [SerializeField] private float investigateDwellMin = 5f;
    [SerializeField] private float investigateDwellMax = 7f;
    [SerializeField] private bool useRandomInvestigateDwell = true;

    // Internos
    private NavMeshAgent agent;
    private Animator animator;
    private int waypointIndex = 0;
    private bool isPatrolDwell = false;          // pausa propia de patrulla
    private bool isInvestigatingDwell = false;   // pausa propia de INVESTIGANDO
    private float dwellTimer = 0f;
    private Vector3 startAnchor;                 // ancla si no hay área de patrulla
    private Transform target;                    // para PERSIGUIENDO (opcional)
    private Vector3 lastKnownTargetPos = Vector3.zero;

    // Animator (opcional)
    private static readonly int HashIsMoving = Animator.StringToHash("IsMoving");
    private static readonly int HashSpeed = Animator.StringToHash("Speed");

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
        if (animator && agent != null)
        {
            float speed = agent.velocity.magnitude;
            animator.SetBool(HashIsMoving, speed > 0.02f);
            animator.SetFloat(HashSpeed, speed);
        }

        switch (currentState)
        {
            case State.PATRULLANDO:   UpdatePatrol();      break;
            case State.INVESTIGANDO:  UpdateInvestigate(); break;
            case State.PERSIGUIENDO:  UpdateChase();       break;
        }
    }

    // =======================
    //        PATRULLA
    // =======================
    private void EnterPatrolState(bool pickNewPoint)
    {
        currentState = State.PATRULLANDO;
        if (!agent || !agent.isOnNavMesh) return;

        isPatrolDwell = false;
        isInvestigatingDwell = false;

        agent.isStopped = false;
        agent.speed = patrolSpeed;

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
            Vector3 candidate = new Vector3(startAnchor.x + rnd.x, startAnchor.y, startAnchor.z + rnd.y);
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
    //      INVESTIGACIÓN
    // =======================
    private void UpdateInvestigate()
    {
        if (!agent || agent.pathPending) return;

        // Pausa en el punto oído (5–7 s por defecto)
        if (isInvestigatingDwell)
        {
            dwellTimer -= Time.deltaTime;
            if (dwellTimer <= 0f)
            {
                isInvestigatingDwell = false;
                EnterPatrolState(true); // tras esperar, vuelve a patrullar
            }
            return;
        }

        // ¿Llegó al punto?
        if (agent.remainingDistance <= arrivalThreshold)
        {
            StartInvestigateDwell(); // iniciar la espera de 5–7 s
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
            agent.isStopped = true; // quedarse quieto
            agent.ResetPath();
        }
    }

    // Llamado por tu sistema de "oído"
    public void GoToInvestigateState(Vector3 locationToInvestigate)
    {
        if (!agent || !agent.isOnNavMesh) return;

        currentState = State.INVESTIGANDO;
        isPatrolDwell = false;
        isInvestigatingDwell = false;

        agent.isStopped = false;
        agent.speed = investigateSpeed;

        Vector3 dest = ProjectToNavMesh(locationToInvestigate, maxSampleDistance);
        SafeSetDestination(dest);
    }

    // Alias por comodidad
    public void OnHearNoise(Vector3 position) => GoToInvestigateState(position);

    // =======================
    //       PERSECUCIÓN (opcional)
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
            // Si perdiste al jugador y quieres que vaya a la última pos vista y
            // aplique la misma pausa de INVESTIGANDO:
            if (lastKnownTargetPos != Vector3.zero)
                GoToInvestigateState(lastKnownTargetPos);
            else
                EnterPatrolState(true);
        }
    }

    public void GoToChaseState(Transform chaseTarget)
    {
        if (!agent || !agent.isOnNavMesh) return;

        target = chaseTarget;
        currentState = State.PERSIGUIENDO;
        isPatrolDwell = false;
        isInvestigatingDwell = false;

        agent.isStopped = false;
        agent.speed = chaseSpeed;

        if (target) SafeSetDestination(target.position);
    }

    public void OnSeePlayer(Transform player) => GoToChaseState(player);

    public void OnLosePlayer()
    {
        if (target != null) lastKnownTargetPos = target.position;
        target = null;
        // Deja que UpdateChase() redirija a INVESTIGANDO en la última posición
    }

#if UNITY_EDITOR
    // Gizmos para depurar el área de patrulla
    void OnDrawGizmosSelected()
    {
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
