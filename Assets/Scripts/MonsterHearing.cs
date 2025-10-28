using UnityEngine;

[RequireComponent(typeof(MonsterAI))]
public class MonsterHearing : MonoBehaviour
{
    [Range(0.1f, 3f)] public float hearingSensitivity = 1f;
    private MonsterAI monsterAI;

    void Awake() => monsterAI = GetComponent<MonsterAI>();

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

    public void ProcessSound(Vector3 soundPosition, float soundRange)
    {
        Debug.Log("PASO 2: Monstruo procesando sonido.");
        if (monsterAI.currentState == MonsterAI.State.PERSIGUIENDO) return;

        float effectiveRange = soundRange * hearingSensitivity;
        float distanceToSound = Vector3.Distance(transform.position, soundPosition);
        Debug.Log($"Distancia al sonido: {distanceToSound} / Rango efectivo: {effectiveRange}");

        if (distanceToSound <= effectiveRange)
        {
            monsterAI.GoToInvestigateState(soundPosition);
        }
    }
}
