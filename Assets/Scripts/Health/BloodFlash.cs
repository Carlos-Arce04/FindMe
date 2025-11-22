using UnityEngine;
using UnityEngine.UI;
using System.Collections;


public class BloodFlash : MonoBehaviour
{
    public Image bloodImage;
    public float maxAlpha = 0.8f; // intensidad del golpe
    public float fadeTime = 0.4f;

    public void ShowBlood()
    {
        StopAllCoroutines();
        StartCoroutine(Flash());
    }

    IEnumerator Flash()
    {
        Color c = bloodImage.color;
        c.a = maxAlpha;
        bloodImage.color = c;

        float t = 0f;
        while (t < fadeTime)
        {
            t += Time.unscaledDeltaTime;
            c.a = Mathf.Lerp(maxAlpha, 0f, t / fadeTime);
            bloodImage.color = c;
            yield return null;
        }

        c.a = 0f;
        bloodImage.color = c;
    }
}
