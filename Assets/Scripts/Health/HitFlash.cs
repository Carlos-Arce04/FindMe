using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HitFlash : MonoBehaviour
{
    public Image flashImage;           // referencia a la imagen roja
    public float flashAlpha = 0.45f;   // opacidad al recibir daño
    public float fadeTime = 0.25f;     // cuánto tarda en desaparecer

    public void Flash()
    {
        StopAllCoroutines();
        StartCoroutine(FlashEffect());
    }

    IEnumerator FlashEffect()
    {
        if (!flashImage) yield break;

        Color c = flashImage.color;
        c.a = flashAlpha;
        flashImage.color = c;

        float t = 0f;
        while (t < fadeTime)
        {
            t += Time.unscaledDeltaTime;
            c.a = Mathf.Lerp(flashAlpha, 0f, t / fadeTime);
            flashImage.color = c;
            yield return null;
        }
        c.a = 0f;
        flashImage.color = c;
    }
}
