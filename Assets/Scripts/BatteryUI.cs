using UnityEngine;
using UnityEngine.UI;

public class BatteryUI : MonoBehaviour
{
    [Header("Refs")]
    public FlashlightToggleAndBattery flashlight; 
    public Image batteryImage;                    

    [Header("Colores")]
    public Color colorHigh = new Color(0.2f, 0.8f, 0.2f); // Verde
    public Color colorMid  = new Color(1f, 0.85f, 0.2f);  // Amarillo
    public Color colorLow  = new Color(0.9f, 0.2f, 0.2f); // Rojo

    [Range(0f,1f)] public float midThreshold = 0.5f; // >50% = verde
    [Range(0f,1f)] public float lowThreshold = 0.2f; // <20% = rojo

    void OnEnable()
    {
        if (flashlight != null)
            flashlight.OnBatteryChanged += OnBatteryChanged;
    }

    void OnDisable()
    {
        if (flashlight != null)
            flashlight.OnBatteryChanged -= OnBatteryChanged;
    }

    void Start()
    {
        if (flashlight != null)
            OnBatteryChanged(flashlight.currentCharge, flashlight.maxCharge);
    }

    void OnBatteryChanged(float current, float max)
    {
        float p = max <= 0f ? 0f : current / max;

        if (batteryImage != null)
        {
            // Ajustar relleno
            batteryImage.fillAmount = p;

            // Cambiar color según nivel
            if (p > midThreshold)
                batteryImage.color = colorHigh;
            else if (p > lowThreshold)
                batteryImage.color = colorMid;
            else
                batteryImage.color = colorLow;
        }
    }
}
