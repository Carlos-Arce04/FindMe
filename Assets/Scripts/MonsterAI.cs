using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class MonsterAI : MonoBehaviour
{
    public enum State { PATRULLANDO, INVESTIGANDO, PERSIGUIENDO }
    public State currentState;

    [Header("Navigation & Movement")]
    private NavMeshAgent agent;
    public float patrolSpeed = 3.5f;
    public float investigationSpeed = 6f;

    private Animator animator;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();  // Obtener el Animator del monstruo
    }

    void Start()
    {
        GoToPatrolState(); // Inicia en patrullaje
    }

    void Update()
    {
        // Controlar las animaciones en base a la velocidad del agente
        bool isMoving = agent.velocity.sqrMagnitude > 0.01f; // Detectar si se mueve

        animator.SetBool("IsMoving", isMoving);  // Actualiza el parámetro en el Animator

        switch (currentState)
        {
            case State.INVESTIGANDO:
                if (!agent.pathPending && agent.remainingDistance < 1.0f)
                    GoToPatrolState();
                break;

            case State.PATRULLANDO:
                // Lógica de patrullaje
                break;

            case State.PERSIGUIENDO:
                // Lógica de persecución
                break;
        }
    }

    public void GoToInvestigateState(Vector3 locationToInvestigate)
    {
        currentState = State.INVESTIGANDO;
        agent.speed = investigationSpeed;
        agent.SetDestination(locationToInvestigate);
    }

    public void GoToPatrolState()
    {
        currentState = State.PATRULLANDO;
        agent.speed = patrolSpeed;
        if (agent.isOnNavMesh && agent.isActiveAndEnabled)
            agent.SetDestination(transform.position);
    }
}
