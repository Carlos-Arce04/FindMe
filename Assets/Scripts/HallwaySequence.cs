using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HallwaySequence : MonoBehaviour
{
    [Header("Control de Ambiente")]
    public GameObject ambienceContainer; 
    private AudioSource[] allAmbientSources;
    private float[] originalVolumes;

    [Header("Control del Jugador")]
    [Tooltip("Arrastra aquí el script de la linterna del jugador")]
    public FlashlightToggleAndBattery playerFlashlight; 

    [Header("Audio de la Secuencia")]
    public AudioSource slowFootstepsSource;
    public AudioSource whisperSource; 
    
    [Tooltip("Arrastra aquí el AudioSource de la PUERTA (NUEVO)")]
    public AudioSource doorSlamSource; // <-- NUEVO: EL PORTAZO

    public AudioSource fastFootstepsSource;

    [Tooltip("Arrastra aquí el AudioSource del grito del monstruo")]
    public AudioSource monsterAttackSource; 
    
    [Header("El Monstruo")]
    public GameObject monster;
    public Transform monsterRunTarget; 
    public float monsterRunSpeed = 12f; 

    private Animator monsterAnimator;
    private bool sequenceHasRun = false;
    private Coroutine slowStepsRoutine;
    private Coroutine fastStepsRoutine;

    void Start()
    {
        // Preparación Ambiente
        if (ambienceContainer != null)
        {
            allAmbientSources = ambienceContainer.GetComponentsInChildren<AudioSource>();
            originalVolumes = new float[allAmbientSources.Length];
            for (int i = 0; i < allAmbientSources.Length; i++) originalVolumes[i] = allAmbientSources[i].volume;
        }

        if (monster != null)
        {
            monsterAnimator = monster.GetComponent<Animator>();
            monster.SetActive(false); 
        }
    }

    public void StartSequence()
    {
        if (sequenceHasRun) return; 
        sequenceHasRun = true;
        StartCoroutine(TheSequence());
    }

    private IEnumerator TheSequence()
    {
        // --- PASO 0: INICIO ---
        if (allAmbientSources != null) foreach (AudioSource source in allAmbientSources) source.volume = 0f;

        if (playerFlashlight != null) playerFlashlight.isLocked = true;

        // --- FASE 1: TENSIÓN (6 SEGUNDOS) ---
        StartCoroutine(FlickerFlashlight(6.0f, 0.1f, 0.4f)); 
        
        if (slowFootstepsSource != null) 
            slowStepsRoutine = StartCoroutine(PlayStepLoop(slowFootstepsSource, 1.4f));

        yield return new WaitForSeconds(6.0f); 


        // --- FASE 2: APAGÓN TOTAL ---
        if (slowStepsRoutine != null) StopCoroutine(slowStepsRoutine);
        
        if (playerFlashlight != null && playerFlashlight.flashlight != null)
            playerFlashlight.flashlight.enabled = false;
        
        yield return new WaitForSeconds(2.0f); 

        // --- FASE 3: SUSURRO ---
        if (whisperSource != null) whisperSource.Play();
        
        yield return new WaitForSeconds(3.0f); 

        // --- FASE 3.5: EL PORTAZO (NUEVO) ---
        if (doorSlamSource != null) 
            doorSlamSource.Play(); // ¡PLAM!
        
        // Esperamos un segundo para que el jugador salte del susto antes de que salga el monstruo
        yield return new WaitForSeconds(1.0f);


        // --- FASE 4: ATAQUE ---
        if (monster != null)
        {
            monster.SetActive(true); 
            if(monster.GetComponent<Collider>() != null) monster.GetComponent<Collider>().enabled = false;
            if(monster.GetComponent<Rigidbody>() != null) monster.GetComponent<Rigidbody>().isKinematic = true;
            // El Animator corre automáticamente
        }
        
        if (fastFootstepsSource != null)
            fastStepsRoutine = StartCoroutine(PlayStepLoop(fastFootstepsSource, 0.3f));

        if (monsterAttackSource != null) monsterAttackSource.Play();

        yield return new WaitForSeconds(1.0f); 

        // ¡SUSTO! Encendemos linterna
        if (playerFlashlight != null && playerFlashlight.flashlight != null)
            playerFlashlight.flashlight.enabled = true;

        yield return StartCoroutine(MoveMonster()); 


        // --- FASE 5: FIN Y DESBLOQUEO ---
        if (fastStepsRoutine != null) StopCoroutine(fastStepsRoutine);

        if (playerFlashlight != null) playerFlashlight.isLocked = false;
        
        if (monster != null) monster.SetActive(false); 

        if (allAmbientSources != null)
        {
            for (int i = 0; i < allAmbientSources.Length; i++) allAmbientSources[i].volume = originalVolumes[i];
        }
    }

    // --- Funciones Auxiliares ---
    private IEnumerator FlickerFlashlight(float duration, float minTime, float maxTime)
    {
        float timer = 0f;
        bool isOn = true;
        while (timer < duration)
        {
            isOn = !isOn;
            if (playerFlashlight != null && playerFlashlight.flashlight != null)
                playerFlashlight.flashlight.enabled = isOn;
            
            float waitTime = Random.Range(minTime, maxTime);
            timer += waitTime;
            yield return new WaitForSeconds(waitTime);
        }
    }

    private IEnumerator PlayStepLoop(AudioSource source, float interval)
    {
        while (true) { source.PlayOneShot(source.clip); yield return new WaitForSeconds(interval); }
    }

    private IEnumerator MoveMonster()
    {
        if (monster == null || monsterRunTarget == null) yield break;
        monster.transform.LookAt(monsterRunTarget.position);
        Vector3 startPos = monster.transform.position;
        Vector3 endPos = monsterRunTarget.position;
        float journey = 0f;
        float distance = Vector3.Distance(startPos, endPos);
        if (distance <= 0) yield break; 
        while (journey < distance)
        {
            journey += Time.deltaTime * monsterRunSpeed;
            float percent = journey / distance;
            monster.transform.position = Vector3.Lerp(startPos, endPos, percent);
            yield return null; 
        }
    }
}