using UnityEngine;

public class KeyPickupTrigger : MonoBehaviour
{
    // Ya no usamos LevelId ni StepKind, así que los quitamos

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // Avisar al nuevo GameProgressManager que se recogió una llave
        GameProgressManager.Instance?.RegisterKeyCollected();

        // Aquí ya no manejamos inventario, solo destruimos la llave
        Destroy(gameObject);
    }
}
