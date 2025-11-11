using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthBarUI : MonoBehaviour
{
    public PlayerHealth player;          
    public Image barFill;                
    public Text titleText;

    ///colores dependiendo del nivel de salud
    public Color colorHigh = Color.green;                 // >60%
    public Color colorMid  = Color.yellow;                // 40–60%
    public Color colorLow  = new Color(1f, 0.5f, 0f);     // 20–40% (naranja)
    public Color colorCrit = Color.red;                   // <20%

    void Start()
    {
        if (player == null) player = FindObjectOfType<PlayerHealth>();
        if (player != null) player.onHealthChanged.AddListener(UpdateBar);
        if (player != null) UpdateBar(player.maxHealth, player.maxHealth);
    }
    // Actualiza la barra de salud y el texto
    void UpdateBar(float current, float max)
    {
        if (max <= 0f) return;

        float pct = Mathf.Clamp01(current / max);

        if (barFill != null) barFill.fillAmount = pct;

        Color c = (pct > 0.60f) ? colorHigh :
                  (pct > 0.40f) ? colorMid  :
                  (pct > 0.20f) ? colorLow  : colorCrit;

        if (barFill   != null) barFill.color   = c;
        if (titleText != null)
        {
            titleText.text  = $"Health {Mathf.RoundToInt(pct * 100f)}%";
            titleText.color = c;
        }
    }
}
