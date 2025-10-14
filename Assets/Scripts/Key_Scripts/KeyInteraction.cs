// KeyInteraction.cs (Actualizado para sonido centralizado)
using UnityEngine;
using TMPro;

public class KeyInteraction : MonoBehaviour
{
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

        if (successfulHit && hit.collider.CompareTag("Key"))
        {
            if (pickupPromptText != null)
            {
                pickupPromptText.gameObject.SetActive(true);
            }

            if (Input.GetKeyDown(interactionKey))
            {
                KeyItem key = hit.collider.GetComponent<KeyItem>();
                if (keyInventory.AddKey(key))
                {
                    if (keyPickupSound != null)
                    {
                        AudioSource.PlayClipAtPoint(keyPickupSound, hit.transform.position, keyPickupVolume);
                    }

                    Destroy(hit.collider.gameObject);
                }
            }
        }
        else
        {
            if (pickupPromptText != null)
            {
                pickupPromptText.gameObject.SetActive(false);
            }
        }
    }
}