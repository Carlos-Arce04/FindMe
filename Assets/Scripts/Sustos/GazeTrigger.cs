using UnityEngine;

public class GazeTrigger : MonoBehaviour
{
    [Header("Configuración")]
    public Jumpscare jumpscareManager; // Referencia al script del susto
    public float detectionDistance = 20f;   // A qué distancia te ve
    
    [Header("No tocar (Automático)")]
    public Transform playerCamera; 

    void Start()
    {
        // Busca automáticamente la cámara principal
        if (Camera.main != null)
        {
            playerCamera = Camera.main.transform;
        }
        else
        {
            Debug.LogError("¡No encuentro la cámara principal! Asegúrate que tu cámara tenga el tag MainCamera");
        }
    }

    void Update()
    {
        if (playerCamera == null) return;

        // Lanzamos un rayo desde la cámara hacia adelante
        Ray ray = new Ray(playerCamera.position, playerCamera.forward);
        RaycastHit hit;

        // Dibujamos el rayo en la escena (solo visible en el editor) para que veas a dónde apuntas
        Debug.DrawRay(playerCamera.position, playerCamera.forward * detectionDistance, Color.red);

        // Si el rayo choca con algo
        if (Physics.Raycast(ray, out hit, detectionDistance))
        {
            // Verificamos si con lo que chocó es ESTE objeto (el monstruo)
            if (hit.transform == this.transform)
            {
                ActivateJumpscare();
            }
        }
    }

    void ActivateJumpscare()
    {
        // 1. Activamos el susto en el Canvas
        jumpscareManager.TriggerJumpscare();

        // 2. Destruimos o desactivamos esta cara flotante 3D
        // (Para que no se vea la cara 3D detrás de la cara del susto en la pantalla)
        gameObject.SetActive(false); 
        
        // Opcional: Destruirlo si nunca más va a aparecer
        // Destroy(gameObject, 5f); 
    }
}