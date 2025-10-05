using UnityEngine;
using System.Collections;

public class DoubleDoorInteraction : MonoBehaviour
{
    [Header("Configuración de puertas dobles")]
    public Transform leftDoor;  // Subpuerta izquierda
    public Transform rightDoor; // Subpuerta derecha
    public float openSpeed = 2f;  // Velocidad de apertura
    public float openDistance = 3f;  // Distancia de apertura (cuánto se moverán las puertas)
    private bool isOpen = false;
    private Transform playerTransform;  // Referencia a la posición del jugador
    private float interactDistance = 3f; // Distancia de interacción
    private float lookAtAngle = 45f; // Ángulo en el que el jugador puede ver la puerta para interactuar (en grados)

    private Vector3 leftDoorInitialPosition;  // Guardamos la posición inicial de la puerta izquierda
    private Vector3 rightDoorInitialPosition;  // Guardamos la posición inicial de la puerta derecha

    private Rigidbody leftDoorRb;  // Rigidbody de la puerta izquierda
    private Rigidbody rightDoorRb; // Rigidbody de la puerta derecha

    void Start()
    {
        playerTransform = Camera.main.transform;  // Asumimos que la cámara principal es la del jugador
        leftDoorInitialPosition = leftDoor.position;  // Guardamos la posición inicial de la puerta izquierda
        rightDoorInitialPosition = rightDoor.position;  // Guardamos la posición inicial de la puerta derecha

        // Obtener los Rigidbodies de las puertas
        leftDoorRb = leftDoor.GetComponent<Rigidbody>();
        rightDoorRb = rightDoor.GetComponent<Rigidbody>();
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
            StartCoroutine(OpenDoubleDoors());
        }
        else
        {
            StartCoroutine(CloseDoubleDoors());
        }
    }

    private IEnumerator OpenDoubleDoors()
    {
        // Desactivar la física de las puertas mientras las movemos
        if (leftDoorRb != null) leftDoorRb.isKinematic = true;
        if (rightDoorRb != null) rightDoorRb.isKinematic = true;

        // Calcular las posiciones finales para las puertas
        Vector3 leftTargetPosition = leftDoorInitialPosition + new Vector3(-openDistance, 0, 0);  // Puerta izquierda se mueve hacia la izquierda
        Vector3 rightTargetPosition = rightDoorInitialPosition + new Vector3(openDistance, 0, 0);  // Puerta derecha se mueve hacia la derecha

        float elapsedTime = 0f;
        float journeyTime = 1f / openSpeed;  // Tiempo necesario para la apertura

        // Si ambas puertas no se han abierto completamente, seguimos moviéndolas
        while (elapsedTime < journeyTime)
        {
            float fractionOfJourney = elapsedTime / journeyTime;

            leftDoor.position = Vector3.MoveTowards(leftDoor.position, leftTargetPosition, fractionOfJourney * openSpeed * Time.deltaTime);
            rightDoor.position = Vector3.MoveTowards(rightDoor.position, rightTargetPosition, fractionOfJourney * openSpeed * Time.deltaTime);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Asegurarse de que ambas puertas lleguen a la rotación final
        leftDoor.position = leftTargetPosition;
        rightDoor.position = rightTargetPosition;
        isOpen = true;

        // Volver a habilitar las físicas después de abrir
        if (leftDoorRb != null) leftDoorRb.isKinematic = false;
        if (rightDoorRb != null) rightDoorRb.isKinematic = false;
    }

    private IEnumerator CloseDoubleDoors()
    {
        // Desactivar la física de las puertas mientras las movemos
        if (leftDoorRb != null) leftDoorRb.isKinematic = true;
        if (rightDoorRb != null) rightDoorRb.isKinematic = true;

        float elapsedTime = 0f;
        float journeyTime = 1f / openSpeed;  // Tiempo necesario para el cierre

        // Mientras las puertas no estén cerradas completamente, seguimos moviéndolas hacia su posición inicial
        while (elapsedTime < journeyTime)
        {
            float fractionOfJourney = elapsedTime / journeyTime;

            leftDoor.position = Vector3.MoveTowards(leftDoor.position, leftDoorInitialPosition, fractionOfJourney * openSpeed * Time.deltaTime);
            rightDoor.position = Vector3.MoveTowards(rightDoor.position, rightDoorInitialPosition, fractionOfJourney * openSpeed * Time.deltaTime);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Asegurarse de que ambas puertas lleguen a la posición inicial
        leftDoor.position = leftDoorInitialPosition;
        rightDoor.position = rightDoorInitialPosition;
        isOpen = false;

        // Volver a habilitar las físicas después de cerrar
        if (leftDoorRb != null) leftDoorRb.isKinematic = false;
        if (rightDoorRb != null) rightDoorRb.isKinematic = false;
    }
}
