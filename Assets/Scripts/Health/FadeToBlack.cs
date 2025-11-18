using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FadeToBlack : MonoBehaviour
{
    [Header("Fade Settings")]
    public Image fadeImage;         // ← Asigna aquí la Image del panel negro
    public float fadeTime = 1.5f;   // Duración del fade

    void Awake()
    {
        // Asegurar que el panel empieza completamente transparente
        if (fadeImage != null)
        {
            Color c = fadeImage.color;
            c.a = 0f;
            fadeImage.color = c;
        }
    }

    /// <summary>
    /// Inicia fade a negro
    /// </summary>
    public IEnumerator StartFade()
    {
        float t = 0f;
        Color c = fadeImage.color;

        while (t < fadeTime)
        {
            t += Time.unscaledDeltaTime;  // Afecta aunque el juego esté pausado

            c.a = Mathf.Lerp(0f, 1f, t / fadeTime);
            fadeImage.color = c;

            yield return null;
        }

        // Asegurar que termina completamente negro
        c.a = 1f;
        fadeImage.color = c;
    }
}
