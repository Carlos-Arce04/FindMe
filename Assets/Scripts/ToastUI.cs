using UnityEngine;
using TMPro;

public class ToastUI : MonoBehaviour
{
    public CanvasGroup group;       // CanvasGroup del panel
    public TextMeshProUGUI label;   // Texto a mostrar
    public float fadeSpeed = 8f;

    float _timer = 0f;

    void Update()
    {
        if (_timer > 0f)
        {
            _timer -= Time.deltaTime;
            group.alpha = Mathf.MoveTowards(group.alpha, 1f, fadeSpeed * Time.deltaTime);
        }
        else
        {
            group.alpha = Mathf.MoveTowards(group.alpha, 0f, fadeSpeed * Time.deltaTime);
        }
    }

    public void Show(string text, float seconds)
    {
        if (label != null) label.text = text;
        _timer = Mathf.Max(seconds, 0.1f);
    }
}
