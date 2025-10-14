using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Collider))]
public class DoorAudioTrigger : MonoBehaviour
{
    public AudioClip doorAmbience;
    [Range(0f,1f)] public float targetVolume = 0.8f;
    [Min(0f)] public float fadeInTime = 1.0f;
    [Min(0f)] public float fadeOutTime = 1.2f;
    public string playerTag = "Player";

    AudioSource src;
    Coroutine currentFade;

    void Awake()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;

        src = GetComponent<AudioSource>();
        if (src == null) src = gameObject.AddComponent<AudioSource>();

        src.clip = doorAmbience;
        src.loop = true;
        src.playOnAwake = false;
        src.volume = 0f;

        // Audio 3D recomendado
        src.spatialBlend = 1f;
        src.rolloffMode = AudioRolloffMode.Linear;
        src.minDistance = 2f;
        src.maxDistance = 18f;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        if (!src.isPlaying) src.Play();
        StartFade(to: targetVolume, time: fadeInTime);
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        StartFade(to: 0f, time: fadeOutTime, stopWhenDone: true);
    }

    void StartFade(float to, float time, bool stopWhenDone = false)
    {
        if (currentFade != null) StopCoroutine(currentFade);
        currentFade = StartCoroutine(FadeRoutine(to, time, stopWhenDone));
    }

    IEnumerator FadeRoutine(float to, float time, bool stopWhenDone)
    {
        float from = src.volume;
        float t = 0f;
        if (time <= 0f) { src.volume = to; yield break; }

        while (t < 1f)
        {
            t += Time.deltaTime / time;
            src.volume = Mathf.Lerp(from, to, t);
            yield return null;
        }
        src.volume = to;

        if (stopWhenDone && Mathf.Approximately(to, 0f))
            src.Stop();

        currentFade = null;
    }
}
