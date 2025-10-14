using UnityEngine;
using UnityEngine.AI;

public class MonsterFootstepsBySpeed : MonoBehaviour
{
    [Header("Refs")]
    public NavMeshAgent agent;          // si lo dejas vacío, se toma solo en Awake
    public AudioSource footstepSource;  // arrastra el AudioSource del hijo "Sound"

    [Header("Clips")]
    public AudioClip[] stepsConcrete;   // pon 4–8 variaciones cortas (mono)

    [Header("Timming por velocidad")]
    public float speedThreshold = 0.2f; // mínima velocidad para empezar a sonar
    public float walkSpeed = 1.2f;      // velocidad donde el paso es “caminar”
    public float runSpeed  = 3.0f;      // velocidad donde el paso es “correr”
    public float walkInterval = 0.5f;   // tiempo entre pasos caminando
    public float runInterval  = 0.3f;   // tiempo entre pasos corriendo

    [Header("Audio")]
    [Range(0f,1f)] public float volume = 0.9f;
    [Range(0f,0.5f)] public float pitchJitter = 0.07f;

    float nextStepTime;

    void Awake()
    {
        if (!agent) agent = GetComponent<NavMeshAgent>();
        if (!footstepSource) footstepSource = GetComponentInChildren<AudioSource>();
        if (footstepSource) {
            footstepSource.playOnAwake = false;
            footstepSource.loop = false;
            footstepSource.spatialBlend = 1f;
        }
    }

    void Update()
    {
        if (!agent || !footstepSource || stepsConcrete == null || stepsConcrete.Length == 0) return;

        float speed = agent.velocity.magnitude;
        if (speed <= speedThreshold) return;

        if (Time.time >= nextStepTime)
        {
            // reproducir un paso
            footstepSource.pitch = 1f + Random.Range(-pitchJitter, pitchJitter);
            footstepSource.PlayOneShot(stepsConcrete[Random.Range(0, stepsConcrete.Length)], volume);

            // calcular próximo intervalo según la velocidad
            float t = Mathf.InverseLerp(runSpeed, walkSpeed, speed); // 0 = corre, 1 = camina
            float interval = Mathf.Lerp(runInterval, walkInterval, Mathf.Clamp01(t));
            nextStepTime = Time.time + Mathf.Clamp(interval, 0.18f, 0.7f);
        }
    }
}
