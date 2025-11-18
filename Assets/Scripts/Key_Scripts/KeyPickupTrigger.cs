using UnityEngine;

public class KeyPickupTrigger : MonoBehaviour
{
    public LevelId level = LevelId.Floor2;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        ProgressManager.CompleteStep(level, StepKind.Key);
        // destruye la llave / lógica de inventario propio
        Destroy(gameObject);
    }
}
