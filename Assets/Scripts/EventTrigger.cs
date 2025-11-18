using UnityEngine;
using UnityEngine.Events; // ¡Muy importante para usar UnityEvent!

[RequireComponent(typeof(Collider))] // Fuerza a que este objeto tenga un collider
public class EventTrigger : MonoBehaviour
{
    [Header("Configuración del Trigger")]
    [Tooltip("El tag del objeto que debe activar este trigger (normalmente 'Player').")]
    public string targetTag = "Player";

    [Tooltip("¿Este trigger solo debe dispararse una vez?")]
    public bool fireOnce = true;

    [Header("Eventos")]
    [Tooltip("Acciones que se dispararán cuando el jugador entre en la zona.")]
    public UnityEvent OnPlayerEnter;

    private bool hasFired = false; // Para controlar si ya se disparó

    private void Awake()
    {
        // Asegurarnos de que el collider esté configurado como Trigger
        // para que no sea una pared física.
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Comprobamos si el objeto que entró tiene el tag correcto
        if (other.CompareTag(targetTag))
        {
            // Si solo debe dispararse una vez y ya lo hizo, no hacemos nada
            if (fireOnce && hasFired)
            {
                return;
            }

            // ¡Es el jugador y podemos disparar el evento!
            Debug.Log("¡Evento de trigger disparado por " + other.name + "!");
            OnPlayerEnter.Invoke(); // <-- Aquí ocurre la magia
            hasFired = true; // Marcamos que ya se ha disparado
        }
    }
}