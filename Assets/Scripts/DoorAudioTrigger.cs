using UnityEngine;
using System.Collections;

public class DoorAudioTrigger : MonoBehaviour
{
    public AudioClip doorAmbience;  // El audio que deseas reproducir
    private AudioSource audioSource;
    private bool isPlayerInRange = false;
    private bool isPlaying = false;
    private float targetVolume = 1f;  // Volumen final
    private float fadeSpeed = 0.1f;   // Velocidad del fade

    void Start()
    {
        // Asegurarnos de que el AudioSource esté presente
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            Debug.LogError("No AudioSource found on " + gameObject.name);
            return; // Detener el script si no hay AudioSource
        }

        audioSource.clip = doorAmbience;
        audioSource.loop = true;
        audioSource.playOnAwake = false;  // No reproducir al inicio
        audioSource.volume = 0f;  // Inicialmente el volumen es 0
    }

    void Update()
    {
        if (audioSource == null) return;

        // Reproducir el audio solo si el jugador está dentro del rango
        if (isPlayerInRange && !audioSource.isPlaying)
        {
            audioSource.Play();
            StartCoroutine(FadeInAudio());  // Iniciar el fade in
        }
        else if (!isPlayerInRange && audioSource.isPlaying)
        {
            StartCoroutine(FadeOutAudio());  // Iniciar el fade out cuando el jugador sale del rango
        }
    }

    // Detectar cuando el jugador entra en el rango
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            Debug.Log("Jugador ha entrado en el rango.");
        }
    }

    // Detectar cuando el jugador sale del rango
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            Debug.Log("Jugador ha salido del rango.");
        }
    }

    // Coroutine para el fade in del audio (cuando el jugador entra en rango)
    private IEnumerator FadeInAudio()
    {
        float currentVolume = 0f;
        while (currentVolume < targetVolume)
        {
            currentVolume += fadeSpeed * Time.deltaTime;
            audioSource.volume = currentVolume;
            yield return null;  // Esperar un frame antes de continuar
        }
        audioSource.volume = targetVolume;  // Asegurarse de que alcance el volumen final
    }

    // Coroutine para el fade out del audio (cuando el jugador sale del rango)
    private IEnumerator FadeOutAudio()
    {
        float currentVolume = audioSource.volume;
        while (currentVolume > 0f)
        {
            currentVolume -= fadeSpeed * Time.deltaTime;
            audioSource.volume = currentVolume;
            yield return null;  // Esperar un frame antes de continuar
        }
        audioSource.Stop();  // Detener el sonido después del fade out
        audioSource.volume = 0f;  // Asegurarse de que el volumen llegue a 0
    }
}
