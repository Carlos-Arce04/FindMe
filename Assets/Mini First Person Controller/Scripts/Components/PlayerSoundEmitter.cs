using UnityEngine;

public class PlayerSoundEmitter : MonoBehaviour
{
    // Función pública para que otros scripts (como el de efectos de sonido) la llamen.
    public void EmitSound(float range)
{
    // mensaje en la consola cada vez que se emita un sonido.
    Debug.Log("PASO 1: EmitSound llamado. Rango: " + range); 

    if (SoundManager.Instance != null)
    {
        SoundManager.Instance.ReportSound(transform.position, range);
    }
}
    
}