using UnityEngine;

public class FloorGate : MonoBehaviour
{
    [Tooltip("Cuando se complete este nivel, este gate se desbloquea (se desactiva). 2=Floor2, 3=Floor3")]
    public int requiredLevelCompleted = 2;

    public Collider gateCollider;   // arrastra el collider que bloquea el paso
    public GameObject visual;       // opcional: malla/puerta para ocultar al desbloquear
    public AudioSource sfx;         // opcional: sonido al desbloquear

    public void Unlock()
    {
        if (gateCollider != null) gateCollider.enabled = false;
        if (visual != null) visual.SetActive(false);
        if (sfx != null) sfx.Play();
        Debug.Log($"[Gate] Unlocked gate for completed level {requiredLevelCompleted}");
    }

    void Reset()
    {
        if (gateCollider == null) gateCollider = GetComponent<Collider>();
    }
}
