// KeyInteraction.cs (Llaves + muñeco + sonido + progreso)
using UnityEngine;
using TMPro;

public class KeyInteraction : MonoBehaviour
{
    [Header("Interacción")]
    public float interactionDistance = 3f;
    public KeyCode interactionKey = KeyCode.E;

    [Tooltip("Recomendable grosor de 0.5.")]
    public float interactionRadius = 0.5f;

    [Header("UI Prompts")]
    public TextMeshProUGUI pickupPromptText;

    [Header("Sound Effects")]
    [Tooltip("El sonido que se reproducirá al recoger CUALQUIER llave.")]
    public AudioClip keyPickupSound;

    [Tooltip("El volumen del sonido de recogida.")]
    [Range(0f, 1f)]
    public float keyPickupVolume = 0.8f;

    private Camera playerCamera;
    private KeyInventory keyInventory;

    void Start()
    {
        playerCamera = Camera.main;
        keyInventory = GetComponent<KeyInventory>();

        if (pickupPromptText != null)
        {
            pickupPromptText.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        RaycastHit hit;

        bool successfulHit = Physics.SphereCast(
            playerCamera.transform.position,
            interactionRadius,
            playerCamera.transform.forward,
            out hit,
            interactionDistance
        );

        if (!successfulHit)
        {
            if (pickupPromptText != null)
                pickupPromptText.gameObject.SetActive(false);

            return;
        }

        bool isKey  = hit.collider.CompareTag("Key");
        bool isDoll = hit.collider.CompareTag("Doll");

        // Mostrar / ocultar el prompt según lo que se está mirando
        if (pickupPromptText != null)
            pickupPromptText.gameObject.SetActive(isKey || isDoll);

        // Si no se presionó la tecla de interacción, salimos
        if (!Input.GetKeyDown(interactionKey))
            return;

        // ================= RECOGER LLAVE =================
        if (isKey)
        {
            KeyItem key = hit.collider.GetComponent<KeyItem>();

            if (key != null && keyInventory != null && keyInventory.AddKey(key))
            {
                // Avisar al sistema de progreso
                GameProgressManager.Instance?.RegisterKeyCollected();

                // Sonido de recogida de llave
                if (keyPickupSound != null)
                {
                    AudioSource.PlayClipAtPoint(
                        keyPickupSound,
                        hit.transform.position,
                        keyPickupVolume
                    );
                }

                Destroy(hit.collider.gameObject);
            }
        }
        // ================= RECOGER MUÑECO =================
        else if (isDoll)
        {
            // Avisar al sistema de progreso que tomamos el muñeco
            GameProgressManager.Instance?.RegisterPorcelainDollCollected();

            // Si luego quieres inventario visual, lo puedes agregar aquí
            Destroy(hit.collider.gameObject);
        }
    }
}
