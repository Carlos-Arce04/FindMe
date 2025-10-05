using UnityEngine;
using System.Collections;

public class DoorInteraction : MonoBehaviour
{
    public float openSpeed = 2f;  // Velocidad de la puerta (cuánto tiempo se tarda en abrirla)
    public float openAngle = 90f;  // Ángulo máximo de apertura de la puerta
    private bool isOpen = false;
    private Transform playerTransform;  // Referencia a la posición del jugador
    private float interactDistance = 3f; // Distancia de interacción con la puerta
    private float lookAtAngle = 45f; // Ángulo en el que el jugador puede ver la puerta para interactuar (en grados)

    private Quaternion initialRotation;  // Guardamos la rotación inicial
    private float openDuration = 1f;  // Duración en segundos para abrir la puerta

    // **Sonidos de apertura y cierre**
    public AudioClip openSound;  // Sonido para cuando se abre la puerta
    public AudioClip closeSound;  // Sonido para cuando se cierra la puerta
    private AudioSource audioSource;  // Componente para reproducir los sonidos

    void Start()
    {
        playerTransform = Camera.main.transform;  // Asumimos que la cámara principal es la del jugador
        initialRotation = transform.rotation;  // Guardamos la rotación inicial de la puerta
        audioSource = GetComponent<AudioSource>();  // Obtenemos el AudioSource del objeto
    }

    void Update()
    {
        // Verificar si el jugador está cerca y mirando hacia la puerta
        if (IsPlayerNear() && IsPlayerLookingAtDoor() && Input.GetKeyDown(KeyCode.E))
        {
            ToggleDoor();
        }
    }

    private bool IsPlayerNear()
    {
        // Verificar si el jugador está dentro del rango de interacción
        float distance = Vector3.Distance(transform.position, playerTransform.position);
        return distance <= interactDistance;
    }

    private bool IsPlayerLookingAtDoor()
    {
        // Verificar si el jugador está mirando hacia la puerta dentro de un ángulo
        Vector3 directionToDoor = transform.position - playerTransform.position;
        float angle = Vector3.Angle(playerTransform.forward, directionToDoor);

        return angle <= lookAtAngle; // Si el jugador está mirando dentro del rango de ángulo
    }

    private void ToggleDoor()
    {
        if (!isOpen)
        {
            StartCoroutine(OpenDoor());
        }
        else
        {
            StartCoroutine(CloseDoor());
        }
    }

    private IEnumerator OpenDoor()
    {
        // Reproducir sonido de apertura
        if (openSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(openSound);  // Reproduce el sonido de apertura
        }

        // Calcular la rotación final que queremos para la puerta (por ejemplo, 90 grados)
        Quaternion targetRotation = initialRotation * Quaternion.Euler(0, openAngle, 0);

        float elapsedTime = 0f;

        while (elapsedTime < openDuration)
        {
            transform.rotation = Quaternion.Slerp(initialRotation, targetRotation, elapsedTime / openDuration);
            elapsedTime += Time.deltaTime * openSpeed;  // Controla la velocidad de apertura
            yield return null;
        }

        transform.rotation = targetRotation;  // Asegúrate de que llegue exactamente a la rotación final
        isOpen = true;
    }

    private IEnumerator CloseDoor()
    {
        // Reproducir sonido de cierre
        if (closeSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(closeSound);  // Reproduce el sonido de cierre
        }

        float elapsedTime = 0f;

        while (elapsedTime < openDuration)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, initialRotation, elapsedTime / openDuration);
            elapsedTime += Time.deltaTime * openSpeed;  // Controla la velocidad de cierre
            yield return null;
        }

        transform.rotation = initialRotation;  // Asegúrate de que llegue exactamente a la rotación inicial
        isOpen = false;
    }
}
