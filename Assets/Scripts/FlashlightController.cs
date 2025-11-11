using UnityEngine;

public class FlashlightController : MonoBehaviour
{
    [Header("Referencias")]
    public Light flashlight;              // el mismo light de siempre
    public AudioSource switchSound;
    public AudioSource defensiveSound;

    [Header("Modo normal")]
    public float normalIntensity = 2.1f;
    public float normalRange = 10f;
    public Color normalColor = Color.white;

    [Header("Modo defensivo")]
    public KeyCode defensiveKey = KeyCode.Q;
    public float defensiveIntensity = 6f;
    public float defensiveRange = 15f;
    public Color defensiveColor = Color.red;

    private bool isDefensive = false;

    // esto lo leen los monstruos
    public static bool playerIsUsingDefensiveLight = false;

    void Start()
    {
        if (flashlight != null)
        {
            flashlight.intensity = normalIntensity;
            flashlight.range = normalRange;
            flashlight.color = normalColor;
        }
    }

    void Update()
    {
        // 1) si la linterna está apagada y yo creo que estoy en modo defensivo, me salgo
        if (isDefensive && (flashlight == null || !flashlight.enabled))
        {
            ExitDefensiveMode();
        }

        // 2) toggle con Q
        if (Input.GetKeyDown(defensiveKey))
        {
            // si está apagada, no dejar activar
            if (flashlight == null || !flashlight.enabled)
                return;

            if (isDefensive)
                ExitDefensiveMode();
            else
                EnterDefensiveMode();
        }
    }

    void EnterDefensiveMode()
    {
        isDefensive = true;

        if (flashlight != null)
        {
            flashlight.intensity = defensiveIntensity;
            flashlight.range = defensiveRange;
            flashlight.color = defensiveColor;
        }

        playerIsUsingDefensiveLight = true;

        if (defensiveSound != null) defensiveSound.Play();
        else if (switchSound != null) switchSound.Play();
    }

    void ExitDefensiveMode()
    {
        isDefensive = false;

        if (flashlight != null)
        {
            flashlight.intensity = normalIntensity;
            flashlight.range = normalRange;
            flashlight.color = normalColor;
        }

        playerIsUsingDefensiveLight = false;

        // apagar música / sonido
        if (defensiveSound != null && defensiveSound.isPlaying)
            defensiveSound.Stop();

        if (switchSound != null)
            switchSound.Play();
    }
}
