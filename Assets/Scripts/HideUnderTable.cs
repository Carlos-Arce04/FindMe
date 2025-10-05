using UnityEngine;

public class HideUnderTable : MonoBehaviour
{
    [Header("Configuración de escondite")]
    public Transform table;                    // La mesa debajo de la cual el jugador se esconderá
    public float interactDistance = 3f;        // Distancia mínima para interactuar
    public Vector3 cameraRotationEuler = new Vector3(10f, 0f, 0f); // Rotación cámara mientras está escondido
    public float headOffset = 0.1f;           // Espacio libre entre la cabeza y la mesa

    private bool isHiding = false;

    private Transform playerRoot;
    private FirstPersonMovement movement;
    private FirstPersonLook look;
    private Crouch crouch;
    private Rigidbody rb;
    private CapsuleCollider col;

    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private bool originalKinematic;
    private float originalHeight;
    private Vector3 originalCenter;
    private Vector3 originalCameraLocalPos;

    void Start()
    {
        playerRoot = Camera.main.transform.parent; // Root del jugador
        movement = playerRoot.GetComponent<FirstPersonMovement>();
        look = playerRoot.GetComponentInChildren<FirstPersonLook>();
        crouch = playerRoot.GetComponent<Crouch>();
        rb = playerRoot.GetComponent<Rigidbody>();
        col = playerRoot.GetComponent<CapsuleCollider>();

        originalCameraLocalPos = look.transform.localPosition;
        originalRotation = playerRoot.rotation;
        originalHeight = col.height;
        originalCenter = col.center;
        originalKinematic = rb.isKinematic;
    }

    void Update()
    {
        if (Vector3.Distance(playerRoot.position, table.position) <= interactDistance && Input.GetKeyDown(KeyCode.E))
        {
            ToggleHide();
        }
    }

    private void ToggleHide()
    {
        if (isHiding) StopHiding();
        else StartHiding();
    }

    private void StartHiding()
    {
        isHiding = true;

        originalPosition = playerRoot.position;

        // Desactivar física del Rigidbody
        rb.isKinematic = true;

        // Reducir collider para caber debajo de la mesa
        col.height = 0.25f; // Reducir altura del collider
        col.center = new Vector3(0, 0.15f, 0); // Ajustar el centro del collider

        // Verificar si el collider de la mesa es un BoxCollider
        BoxCollider tableCollider = table.GetComponent<BoxCollider>();
        if (tableCollider != null)
        {
            // Obtener el centro de la mesa y su tamaño
            Vector3 tableCenter = tableCollider.center + table.position; // Centro de la mesa
            Vector3 tableSize = tableCollider.size;

            // Debug para verificar valores de la mesa
            Debug.Log("Centro de la mesa: " + tableCenter);
            Debug.Log("Tamaño de la mesa: " + tableSize);

            // Posicionar el jugador debajo de la mesa
            float newY = tableCenter.y + (tableSize.y / 2f) - headOffset - (col.height / 2f);
            float newX = tableCenter.x; // Centrado lateralmente
            float newZ = tableCenter.z; // Centrado en la profundidad

            // Ajuste de la posición del jugador debajo de la mesa
            newY = Mathf.Max(newY, 0.5f); // Evita que el jugador pase el suelo

            playerRoot.position = new Vector3(newX, newY, newZ);
        }
        else
        {
            // Si no es un BoxCollider, calculamos la posición sin usar center y size
            Vector3 tablePosition = table.position;
            float newY = tablePosition.y - headOffset - (col.height / 2f);
            float newX = tablePosition.x; // Centrado lateralmente
            float newZ = tablePosition.z; // Centrado en la profundidad

            // Ajuste de la posición del jugador debajo de la mesa
            newY = Mathf.Max(newY, 0.5f); // Evita que el jugador pase el suelo

            playerRoot.position = new Vector3(newX, newY, newZ);
        }

        // Ajustar cámara (más cerca del suelo pero no dentro de la mesa)
        look.transform.localPosition = new Vector3(0, 0.65f, 0); // Ajusta la cámara un poco más arriba
        look.transform.localRotation = Quaternion.Euler(cameraRotationEuler);
        look.canLook = true; // Permitir que la cámara se mueva

        // Bloquear movimiento y crouch
        movement.canMove = false;
        crouch.enabled = false;
    }

    private void StopHiding()
    {
        isHiding = false;

        // Restaurar posición y rotación originales
        playerRoot.position = originalPosition;
        playerRoot.rotation = originalRotation;

        // Restaurar física y collider
        rb.isKinematic = originalKinematic;
        col.height = originalHeight;
        col.center = originalCenter;

        // Restaurar cámara
        look.transform.localPosition = originalCameraLocalPos;
        look.canLook = true;

        // Reactivar movimiento y crouch
        movement.canMove = true;
        crouch.enabled = true;
    }
}
