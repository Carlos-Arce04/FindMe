using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Collider))]
public class ProximityAudioZone : MonoBehaviour
{
    [Header("Player detection")]
    [Tooltip("Tag del objeto Player que activa la zona")]
    public string playerTag = "Player";

    [Header("Volúmenes y fades")]
    [Range(0f, 1f)] public float targetVolume = 0.8f;
    [Min(0f)] public float fadeInTime = 1.0f;
    [Min(0f)] public float fadeOutTime = 1.5f;
    [Tooltip("Si es true, el volumen se escala según la distancia al centro del trigger.")]
    public bool scaleWithDistance = false;

    [Tooltip("Cuando scaleWithDistance es true, este es el radio interior (en metros) donde suena al volumen máximo.")]
    public float innerRadius = 1.0f;

    [Tooltip("Cuando scaleWithDistance es true y el collider es grande, usa este radio exterior (desde el centro) como referencia para volumen 0.")]
    public float outerRadius = 8.0f;

    AudioSource src;
    Collider triggerCol;
    Coroutine fadeRoutine;
    int insideCount = 0; 

    void Awake()
    {
        src = GetComponent<AudioSource>();
        triggerCol = GetComponent<Collider>();

        triggerCol.isTrigger = true;

        src.loop = true;          
        src.playOnAwake = false;  
        src.spatialBlend = 1f;    
        src.volume = 0f;          
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        insideCount++;
        if (insideCount == 1)
        {
            if (!src.isPlaying) src.Play();
            StartFade(targetVolume, fadeInTime);
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (!scaleWithDistance) return;
        if (!other.CompareTag(playerTag)) return;

        float desired = targetVolume * DistanceFactor(other.transform.position);
        src.volume = Mathf.MoveTowards(src.volume, desired, Time.deltaTime * (targetVolume / Mathf.Max(0.05f, fadeInTime)));
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        insideCount = Mathf.Max(insideCount - 1, 0);
        if (insideCount == 0)
        {
            StartFade(0f, fadeOutTime, stopWhenDone: true);
        }
    }

    void StartFade(float to, float time, bool stopWhenDone = false)
    {
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(FadeRoutine(to, time, stopWhenDone));
    }

    IEnumerator FadeRoutine(float to, float time, bool stopWhenDone)
    {
        float from = src.volume;
        if (Mathf.Approximately(time, 0f))
        {
            src.volume = to;
        }
        else
        {
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / time;
                src.volume = Mathf.Lerp(from, to, t);
                yield return null;
            }
            src.volume = to;
        }

        if (stopWhenDone && Mathf.Approximately(src.volume, 0f))
            src.Stop();

        fadeRoutine = null;
    }

    float DistanceFactor(Vector3 playerPos)
    {
        Vector3 center = triggerCol.bounds.center;
        float d = Vector3.Distance(playerPos, center);
        float t = Mathf.InverseLerp(outerRadius, innerRadius, d);
        return Mathf.Clamp01(t);
    }
    void OnDrawGizmosSelected()
    {
        if (!scaleWithDistance) return;
        Gizmos.color = new Color(0f, 0.8f, 1f, 0.25f);
        Gizmos.DrawWireSphere(GetComponent<Collider>() ? GetComponent<Collider>().bounds.center : transform.position, outerRadius);
        Gizmos.color = new Color(0f, 1f, 0.3f, 0.35f);
        Gizmos.DrawWireSphere(GetComponent<Collider>() ? GetComponent<Collider>().bounds.center : transform.position, innerRadius);
    }
}
