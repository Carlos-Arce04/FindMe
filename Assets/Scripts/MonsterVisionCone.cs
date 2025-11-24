using UnityEngine;

public class MonsterVisionCone : MonoBehaviour
{
    [Header("Vision Settings")]
    [Tooltip("Rango de visión en unidades del mundo.")]
    [SerializeField] private float visionRange = 10f;

    [Tooltip("Ángulo de visión en grados (0 - 360).")]
    [Range(0f, 360f)]
    [SerializeField] private float visionAngle = 90f;

    [Header("Target & Obstacle Layers")]
    [Tooltip("Capa donde está el jugador u otros objetivos.")]
    [SerializeField] private LayerMask targetLayer;

    [Tooltip("Capa de obstáculos que bloquean la visión (paredes, etc.).")]
    [SerializeField] private LayerMask obstacleLayer;

    [Header("Estado de detección (solo lectura)")]
    [SerializeField] private bool canSeeTarget = false;
    [SerializeField] private Transform currentTarget;

    [Header("Referencia al MonsterAI")]
    [Tooltip("Referencia al script MonsterAI que controlará la IA.")]
    [SerializeField] private MonsterAI monsterAI;

    // Propiedad para que el menú pueda leer el rango actual
    public float VisionRange => visionRange;

    // ---- MÉTODOS DE CONFIGURACIÓN DESDE MENÚ ----

    /// <summary>
    /// Cambia el rango de visión. Llamado desde el menú.
    /// </summary>
    public void SetVisionRange(float value)
    {
        visionRange = Mathf.Max(0f, value);
    }

    // ---- CICLO DE VIDA ----

    private void Awake()
    {
        // Si no se asignó por Inspector, intenta buscar en el mismo GameObject o padres.
        if (!monsterAI)
        {
            monsterAI = GetComponent<MonsterAI>();
            if (!monsterAI)
                monsterAI = GetComponentInParent<MonsterAI>();
        }
    }

    private void Update()
    {
        DetectTargets();
    }

    // ---- LÓGICA DE DETECCIÓN ----

    private void DetectTargets()
    {
        bool sawTargetThisFrame = false;
        Transform bestTarget = null;

        // Buscar posibles objetivos en un radio
        Collider[] hits = Physics.OverlapSphere(transform.position, visionRange, targetLayer);

        float closestDistance = Mathf.Infinity;

        foreach (Collider col in hits)
        {
            Transform candidate = col.transform;
            Vector3 dirToTarget = (candidate.position - transform.position).normalized;
            float distanceToTarget = Vector3.Distance(transform.position, candidate.position);

            // Comprobar ángulo
            float angleToTarget = Vector3.Angle(transform.forward, dirToTarget);
            if (angleToTarget > visionAngle * 0.5f)
                continue;

            // Comprobar línea de visión (raycast contra obstáculos)
            if (Physics.Raycast(transform.position, dirToTarget, out RaycastHit hitInfo, distanceToTarget, obstacleLayer))
            {
                // Hay un obstáculo entre medio
                continue;
            }

            // Si llega aquí, el objetivo es visible
            if (distanceToTarget < closestDistance)
            {
                closestDistance = distanceToTarget;
                bestTarget = candidate;
            }
        }

        if (bestTarget != null)
        {
            sawTargetThisFrame = true;
        }

        // Cambios de estado de visión
        if (sawTargetThisFrame)
        {
            if (!canSeeTarget || currentTarget != bestTarget)
            {
                // Recién vio al jugador o cambió de objetivo
                currentTarget = bestTarget;
                canSeeTarget = true;

                if (monsterAI && currentTarget != null)
                {
                    monsterAI.OnSeePlayer(currentTarget);
                }
            }
        }
        else
        {
            if (canSeeTarget)
            {
                // Dejó de ver al jugador
                canSeeTarget = false;

                if (monsterAI)
                {
                    monsterAI.OnLosePlayer();
                }

                currentTarget = null;
            }
        }
    }

    // ---- GIZMOS PARA VER EL CONO EN LA ESCENA ----

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        // Círculo de rango
        Gizmos.color = new Color(1f, 1f, 0f, 0.2f); // amarillo transparente
        Gizmos.DrawWireSphere(transform.position, visionRange);

        // Líneas del cono
        Gizmos.color = Color.yellow;
        Vector3 leftBoundary = DirectionFromAngle(-visionAngle / 2f, true);
        Vector3 rightBoundary = DirectionFromAngle(visionAngle / 2f, true);

        Gizmos.DrawLine(transform.position, transform.position + leftBoundary * visionRange);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary * visionRange);

        // Si ve un objetivo, dibuja una línea
        if (canSeeTarget && currentTarget != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, currentTarget.position);
        }
    }

    /// <summary>
    /// Convierte un ángulo en una dirección (solo en el plano XZ).
    /// </summary>
    private Vector3 DirectionFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
        {
            angleInDegrees += transform.eulerAngles.y;
        }

        float rad = angleInDegrees * Mathf.Deg2Rad;
        return new Vector3(Mathf.Sin(rad), 0f, Mathf.Cos(rad));
    }
#endif
}
