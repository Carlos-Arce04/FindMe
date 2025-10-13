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
        
        // Physics.SphereCast, el radio se define en unity
        
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