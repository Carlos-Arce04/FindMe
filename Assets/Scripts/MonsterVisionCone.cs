using UnityEngine;
using UnityEditor;

public class MonsterVisionCone : MonoBehaviour
{
    [Header("Vision Settings")]
    public float visionRange = 10f; // Rango de la visión en metros
    [Range(0, 360)]
    public float visionAngle = 90f; // Ángulo de la visión en grados

    [Header("Target & Obstacle Layers")]
    public LayerMask targetLayer;   // La capa donde se encuentra el jugador/objetivo
    public LayerMask obstacleLayer; // La capa para los obstáculos (paredes, etc.)

    [Header("Detection Status")]
    public bool canSeeTarget = false; // Para saber si el monstruo ve al objetivo

    private Transform targetFound; // Referencia al objetivo encontrado

    void Update()
    {
        FindVisibleTargets();
    }

    void FindVisibleTargets()
    {
        // Reseteamos el estado cada frame
        canSeeTarget = false;
        targetFound = null;

        // 1. Encontrar colliders del objetivo dentro de una esfera (primer filtro de distancia)
        Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, visionRange, targetLayer);

        for (int i = 0; i < targetsInViewRadius.Length; i++)
        {
            Transform target = targetsInViewRadius[i].transform;
            Vector3 directionToTarget = (target.position - transform.position).normalized;

            // 2. Comprobar si el objetivo está dentro del ángulo de visión
            if (Vector3.Angle(transform.forward, directionToTarget) < visionAngle / 2)
            {
                float distanceToTarget = Vector3.Distance(transform.position, target.position);

                // 3. Comprobar si no hay obstáculos en la línea de visión con un Raycast
                if (!Physics.Raycast(transform.position, directionToTarget, distanceToTarget, obstacleLayer))
                {
                    // ¡Objetivo detectado!
                    canSeeTarget = true;
                    targetFound = target;
                    Debug.Log("¡He visto a " + target.name + "!");
                    // Aquí podrías llamar a otras funciones (perseguir, atacar, etc.)
                    break; // Salimos del bucle si ya encontramos un objetivo
                }
            }
        }
    }

    // --- Editor Visuals ---
    #if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        // --- MODIFICADO: El color ahora es siempre amarillo ---
        Color gizmoColor = Color.yellow; 
        Handles.color = gizmoColor;
        Gizmos.color = gizmoColor;

        // --- Visualización del Rango General ---
        Handles.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 0.1f);
        Handles.DrawSolidDisc(transform.position, Vector3.up, visionRange);


        // --- Visualización del Cono 3D ---
        Quaternion upRayRotation = Quaternion.AngleAxis(-visionAngle / 2, transform.right);
        Quaternion downRayRotation = Quaternion.AngleAxis(visionAngle / 2, transform.right);
        Quaternion leftRayRotation = Quaternion.AngleAxis(-visionAngle / 2, transform.up);
        Quaternion rightRayRotation = Quaternion.AngleAxis(visionAngle / 2, transform.up);

        Vector3 upDir = upRayRotation * transform.forward;
        Vector3 downDir = downRayRotation * transform.forward;
        Vector3 leftDir = leftRayRotation * transform.forward;
        Vector3 rightDir = rightRayRotation * transform.forward;
        
        // Se reestablece el color principal para las líneas
        Gizmos.color = gizmoColor;
        Gizmos.DrawRay(transform.position, upDir * visionRange);
        Gizmos.DrawRay(transform.position, downDir * visionRange);
        Gizmos.DrawRay(transform.position, leftDir * visionRange);
        Gizmos.DrawRay(transform.position, rightDir * visionRange);

        // Dibuja el arco en el suelo para una referencia horizontal clara
        Handles.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 0.2f);
        Vector3 horizontalBaseAngle = DirectionFromAngle(-visionAngle / 2, false);
        Handles.DrawSolidArc(transform.position, Vector3.up, horizontalBaseAngle, visionAngle, visionRange);

        // Dibuja una línea directa hacia el objetivo si es detectado
        if (canSeeTarget && targetFound != null)
        {
            // --- MODIFICADO: La línea ahora usa el color base (amarillo) ---
            Gizmos.color = gizmoColor; 
            Gizmos.DrawLine(transform.position, targetFound.position);
        }
    }

    // Función auxiliar para calcular la dirección a partir de un ángulo (solo para el arco horizontal)
    private Vector3 DirectionFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
        {
            angleInDegrees += transform.eulerAngles.y;
        }
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }
    #endif
}