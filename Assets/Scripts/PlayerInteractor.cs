using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    [Header("Refs")]
    public Camera cam;
    public BatteryInventory inventory;

    [Header("Interacción")]
    public float maxInteractDistance = 3.0f;
    public LayerMask interactMask;

    BatteryPickup currentTarget;

    void Reset()
    {
        if (cam == null) cam = Camera.main;
        if (inventory == null)
            inventory = GetComponentInParent<BatteryInventory>() ?? GetComponent<BatteryInventory>();
    }

    void Update()
{
    UpdateTarget();

    if (currentTarget != null)
    {
        if (!PickupPromptUI.Instance || !PickupPromptUI.Instance.IsLocked)
            PickupPromptUI.Instance?.Show("Presiona E para recoger batería");

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (inventory != null && inventory.currentBatteries >= inventory.maxBatteries)
            {
                PickupPromptUI.Instance?.ShowSticky(
                    "Inventario lleno (máx. 3)", 3f,
                    "Presiona E para recoger batería"
                );
                return;
            }

            // Guardamos la información del sonido ANTES de que el objeto se destruya
            AudioClip soundToPlay = currentTarget.pickupSound;
            float soundVolume = currentTarget.pickupVolume;
            Vector3 soundPosition = currentTarget.transform.position;

            bool picked = currentTarget.TryPickup(inventory);
            if (picked)
            {
                // --- LÍNEA AÑADIDA ---
                // Si la recogida fue exitosa, reproducimos el sonido guardado
                if (soundToPlay != null)
                {
                    AudioSource.PlayClipAtPoint(soundToPlay, soundPosition, soundVolume);
                }
                // --- FIN DE LA LÍNEA AÑADIDA ---

                PickupPromptUI.Instance?.Hide();
                currentTarget = null;
            }
            else
            {
                PickupPromptUI.Instance?.ShowSticky(
                    "No se pudo recoger", 2f,
                    "Presiona E para recoger batería"
                );
            }
        }
    }
        else
        {
            if (!PickupPromptUI.Instance || !PickupPromptUI.Instance.IsLocked)
                PickupPromptUI.Instance?.Hide();
        }
        
    }   

    void UpdateTarget()
    {
        currentTarget = null;
        if (!cam) return;

        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, maxInteractDistance, interactMask, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider.TryGetComponent(out BatteryPickup p))
                currentTarget = p;
            else
            {
                var p2 = hit.collider.GetComponentInParent<BatteryPickup>();
                if (p2) currentTarget = p2;
            }
        }
    }
}
