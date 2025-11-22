using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Jumpscare : MonoBehaviour
{
    [Header("UI VISUAL")]
    public GameObject scaryFaceObj; 
    public Image blackScreen;       

    [Header("INTENSIDAD")]
    public float shakeAmount = 15f; 
    public float visualDuration = 2.5f; // Cuanto dura la cara en pantalla

    [Header("TIEMPOS DE AUDIO")]
    public float breathingDelay = 1.5f; // Cuánto tarda en empezar a respirar DESPUÉS de que sale la cara
    public float breathingDuration = 4.0f; // Cuánto tiempo se queda respirando después

    [Header("REFERENCIAS DE AUDIO")]
    public AudioSource audioSource; // El del jugador
    public AudioClip screamSound;   // El grito

    private bool isScaring = false;
    private Vector3 originalPos;

    void Start()
    {
        if(scaryFaceObj != null) originalPos = scaryFaceObj.transform.localPosition;
    }

    public void TriggerJumpscare()
    {
        if (isScaring) return;
        StartCoroutine(JumpscareSequence());
    }

    IEnumerator JumpscareSequence()
    {
        isScaring = true;

        // --- FASE 1: EL SUSTO INICIAL (GRITO) ---
        scaryFaceObj.SetActive(true);
        
        // Lanzamos el grito como un efecto de sonido único
        if(audioSource != null && screamSound != null) 
        {
            audioSource.PlayOneShot(screamSound);
        }

        // Zoom rápido
        float timerZoom = 0;
        Vector3 startScale = Vector3.one * 0.5f; 
        Vector3 endScale = Vector3.one * 1.5f; 
        
        while(timerZoom < 0.2f) 
        {
            scaryFaceObj.transform.localScale = Vector3.Lerp(startScale, endScale, timerZoom / 0.2f);
            timerZoom += Time.deltaTime;
            yield return null;
        }
        scaryFaceObj.transform.localScale = endScale;

        // --- FASE 2: VIBRACIÓN (SILENCIO / GRITO TERMINANDO) ---
        // Iniciamos una corrutina paralela para que la respiración empiece más tarde
        StartCoroutine(PlayBreathingDelayed());

        float elapsed = 0;
        // Mantenemos la cara vibrando por el tiempo definido en visualDuration
        while (elapsed < visualDuration)
        {
            float x = Random.Range(-1f, 1f) * shakeAmount;
            float y = Random.Range(-1f, 1f) * shakeAmount;
            scaryFaceObj.transform.localPosition = originalPos + new Vector3(x, y, 0);

            float alpha = Mathf.PingPong(Time.time * 15, 0.8f); // Parpadeo más rápido
            Color c = blackScreen.color;
            c.a = alpha;
            blackScreen.color = c;

            elapsed += Time.deltaTime;
            yield return null;
        }

        // --- FASE 3: LA CARA SE VA, PERO EL MIEDO (AUDIO) SIGUE ---
        HideVisuals(); // Ocultamos la cara
        
        // Esperamos un rato más solo escuchando la respiración antes de habilitar otro susto
        yield return new WaitForSeconds(breathingDuration);

        // Apagamos todo finalmente
        StopAudio();
        isScaring = false;
    }

    // Esta rutina espera un poco y luego arranca la respiración
    IEnumerator PlayBreathingDelayed()
    {
        yield return new WaitForSeconds(breathingDelay);
        
        if(audioSource != null)
        {
            // Aseguramos que no se detenga el grito si aun suena, pero iniciamos el loop
            if(!audioSource.isPlaying)
            {
                audioSource.loop = true;
                audioSource.Play(); 
            }
            else
            {
                // Si el grito sigue sonando, forzamos la respiración o esperamos a que termine
                // En este caso, vamos a transicionar
                audioSource.Stop(); // Cortamos grito si es muy largo
                audioSource.loop = true;
                audioSource.Play();
            }
        }
    }

    void HideVisuals()
    {
        scaryFaceObj.SetActive(false);
        Color c = blackScreen.color;
        c.a = 0f;
        blackScreen.color = c;
    }

    void StopAudio()
    {
        if(audioSource != null) 
        {
            audioSource.Stop();
            audioSource.loop = false;
        }
    }
}