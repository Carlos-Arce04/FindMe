using UnityEngine;
using TMPro;

public class GameOverPulse : MonoBehaviour
{
    public float pulseSpeed = 1.5f;   // velocidad del latido
    public float scaleAmount = 1.08f; // cuánto crece
    public float alphaMin = 0.7f;     // mínimo brillo
    public float alphaMax = 1f;       // máximo brillo

    TextMeshProUGUI tmp;
    Vector3 startScale;

    void Awake()
    {
        tmp = GetComponent<TextMeshProUGUI>();
        startScale = transform.localScale;
    }

    void Update()
    {
        float t = (Mathf.Sin(Time.time * pulseSpeed) + 1) / 2f;

        // Escala (latido)
        transform.localScale = startScale * Mathf.Lerp(1f, scaleAmount, t);

        // Brillo (oscila entre alphaMin y alphaMax)
        Color c = tmp.color;
        c.a = Mathf.Lerp(alphaMin, alphaMax, t);
        tmp.color = c;
    }
}
