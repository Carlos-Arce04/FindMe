using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI; // Necesario para evitar errores en otras clases

// Asegúrate de que el objeto tenga un NavMeshAgent
[RequireComponent(typeof(NavMeshAgent))] 
public class MonsterClippingStopper : MonoBehaviour
{
    [Header("Configuración del Sensor")]
    // --- ¡NUEVO! Arrastra la capa "Obstacles" aquí ---
    public LayerMask collisionMask; 
    // --------------------------------------------------

    public float stopDelay = 0.5f; 
    
    private NavMeshAgent agent;
    private bool isAvoidanceStopping = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        
        // Verificación de seguridad: el sensor debe ser un Trigger
        if (!GetComponent<Collider>().isTrigger) 
            Debug.LogError("El Clipping Stopper requiere que su collider esté marcado como 'Is Trigger'.");
    }

    // Cuando el sensor ancho toca algo
    private void OnTriggerStay(Collider other) 
    {
        // 1. Verificamos si el objeto tocado NO está en nuestra capa de colisión (Obstacles).
        // El operador `(1 << other.gameObject.layer)` convierte el número de la capa a un bit para la máscara.
        if (((1 << other.gameObject.layer) & collisionMask) == 0) return; 
        
        // 2. Si es un obstáculo VÁLIDO y el agente se está moviendo:
        if (agent.velocity.sqrMagnitude > 0.01f && !isAvoidanceStopping) 
        {
            // Detenemos al agente para evitar clipping visual
            agent.isStopped = true;
            isAvoidanceStopping = true;
        }
    }
    
    // Cuando el sensor se separa de la pared/puerta
    private void OnTriggerExit(Collider other) 
    {
        // Si el objeto que se fue NO está en la capa de colisión, ignorar
        if (((1 << other.gameObject.layer) & collisionMask) == 0) return; 

        // Le damos un pequeño respiro antes de reanudar el movimiento
        Invoke("ResumeMovement", stopDelay); 
    }

    void ResumeMovement()
    {
        if (agent.isActiveAndEnabled && isAvoidanceStopping)
        {
            agent.isStopped = false;
            isAvoidanceStopping = false;
        }
    }
}