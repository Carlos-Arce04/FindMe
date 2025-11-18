using UnityEngine;

public class DoorOpenTrigger : MonoBehaviour
{
    public LevelId level = LevelId.Floor2;

    // Si usas interacción propia, expón esto como método público y llámalo al abrir.
    public void RegisterDoorOpened()
    {
        ProgressManager.CompleteStep(level, StepKind.Door);
    }

    // Versión por trigger (si prefieres):
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        ProgressManager.CompleteStep(level, StepKind.Door);
        // opcional: Destroy(gameObject);
    }
}
