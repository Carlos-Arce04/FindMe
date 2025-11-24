using UnityEngine;

[RequireComponent(typeof(MonsterAI))]
public class MonsterHearing : MonoBehaviour
{
    [Header("Sensibilidad auditiva (modificable en menú)")]
    [Range(0.1f, 3f)]
    public float hearingSensitivity = 1f; // 1 = normal, <1 menos sensible, >1 más sensible

    private MonsterAI monsterAI;

    /// <summary>
    /// Para que el menú pueda leer el valor actual.
    /// </summary>
    public float HearingSensitivity => hearingSensitivity;

    /// <summary>
    /// Para que el menú pueda cambiar la sensibilidad auditiva.
    /// </summary>
    public void SetHearingSensitivity(float value)
    {
        // Evitamos valores negativos
        hearingSensitivity = Mathf.Max(0f, value);
    }

    void Awake()
    {
        monsterAI = GetComponent<MonsterAI>();
    }

    void OnEnable()
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.RegisterMonster(this);
    }

    void OnDisable()
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.UnregisterMonster(this);
    }

    /// <summary>
    /// Llamado por el SoundManager cuando se emite un sonido en el mundo.
    /// soundPosition = posición del sonido.
    /// soundRange = rango base del sonido (antes de aplicar sensibilidad).
    /// </summary>
    public void ProcessSound(Vector3 soundPosition, float soundRange)
    {
        Debug.Log("PASO 2: Monstruo procesando sonido.");

        if (monsterAI == null)
        {
            Debug.LogWarning("MonsterHearing: MonsterAI no encontrado.");
            return;
        }

        // Si ya está persiguiendo, ignoramos sonidos para no sobre-escribir su objetivo
        if (monsterAI.currentState == MonsterAI.State.PERSIGUIENDO)
            return;

        // Aplica la sensibilidad auditiva para calcular el rango efectivo
        float effectiveRange = soundRange * hearingSensitivity;
        float distanceToSound = Vector3.Distance(transform.position, soundPosition);
        Debug.Log($"Distancia al sonido: {distanceToSound} / Rango efectivo: {effectiveRange}");

        // Solo reacciona si el sonido está dentro del rango efectivo
        if (distanceToSound <= effectiveRange)
        {
            // Usamos OnHearNoise para respetar el tiempo de reacción del MonsterAI
            monsterAI.OnHearNoise(soundPosition);
        }
    }
}
