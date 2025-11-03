using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class MonsterLightReaction : MonoBehaviour
{
    public MonoBehaviour monsterAIScript;   // arrastra tu MonsterAI
    public float fleeSpeed = 5f;
    public float fleeDistance = 4f;         // cuánto se aleja cada vez
    public float recheckTime = 0.2f;        // cada cuánto recalculamos destino

    private Transform player;
    private NavMeshAgent agent;
    private float originalSpeed;
    private float recheckTimer = 0f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        originalSpeed = agent.speed;
    }

    void Update()
    {
        if (player == null) return;

        // mientras la luz defensiva esté activa
        if (FlashlightController.playerIsUsingDefensiveLight)
        {
  
            // apagar IA
            if (monsterAIScript != null && monsterAIScript.enabled)
                monsterAIScript.enabled = false;

            // el agente debe poder moverse
            if (agent.isStopped)
                agent.isStopped = false;

            agent.speed = fleeSpeed;

            // cada cierto rato recalculamos un punto de fuga
            recheckTimer -= Time.deltaTime;
            if (recheckTimer <= 0f)
            {
                recheckTimer = recheckTime;

                Vector3 awayDir = (transform.position - player.position).normalized;
                Vector3 fleePos = transform.position + awayDir * fleeDistance;

                // mandamos al agente a ese punto
                agent.SetDestination(fleePos);
            }
            

            return;
        }

        // si la luz YA no está activa, restauramos todo
        if (monsterAIScript != null && !monsterAIScript.enabled)
            monsterAIScript.enabled = true;

        if (!agent.isStopped)
            agent.speed = originalSpeed;
    }
}
