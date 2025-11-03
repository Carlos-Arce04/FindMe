using UnityEngine;

[RequireComponent(typeof(MonsterAI))]
public class MonsterHearing : MonoBehaviour
{
    private MonsterAI monsterAI;

    private void Awake()
    {
        monsterAI = GetComponent<MonsterAI>();
    }

    private void Start()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.RegisterMonster(this);
        }
    }

    // El SoundManager llama a esta función.
    public void ProcessSound(Vector3 soundPosition, float soundRange)
{
    Debug.Log("PASO 2: Monstruo procesando sonido."); // <--- AÑADE ESTA LÍNEA
    if (monsterAI.currentState == MonsterAI.State.PERSIGUIENDO) return;

    float distanceToSound = Vector3.Distance(transform.position, soundPosition);
    Debug.Log("Distancia al sonido: " + distanceToSound + " / Rango del sonido: " + soundRange); // <--- AÑADE ESTA LÍNEA

    if (distanceToSound <= soundRange)
    {
        monsterAI.GoToInvestigateState(soundPosition);
    }
}
}