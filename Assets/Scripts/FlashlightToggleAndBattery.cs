using UnityEngine;
using System;

public class FlashlightToggleAndBattery : MonoBehaviour
{
    [Header("Refs")]
    public Light flashlight;
    public AudioSource audioSource;

    [Header("Batería")]
    public float maxCharge = 100f;
    public float currentCharge = 100f;
    public float drainPerSecond = 0.7f;
    public float defensiveDrainMultiplier = 1.2f;

    [Header("Flicker (batería baja)")]
    [Range(0.01f, 0.3f)] public float lowBatteryThreshold = 0.10f;
    public bool enableFlicker = true;
    public float flickerNoiseFreq = 18f;
    [Range(0f, 1f)] public float flickerNoiseDepth = 0.35f;
    public float microBlinkChancePerSec = 0.7f;
    public Vector2 microBlinkDuration = new Vector2(0.03f, 0.08f);

    // NUEVO: Variable para bloquear el control del jugador
    [HideInInspector] public bool isLocked = false; 

    float _microBlinkTimer = 0f;

    public event Action<float, float> OnBatteryChanged;

    public bool IsFlashlightOn => flashlight != null && flashlight.enabled;
    public float ChargePercent01 => maxCharge <= 0f ? 0f : currentCharge / maxCharge;
    public bool IsFull => currentCharge >= maxCharge - 0.001f;

    void Awake()
    {
        currentCharge = Mathf.Clamp(currentCharge, 0, maxCharge);
        NotifyUI();
    }

    void Update()
    {
        // MODIFICADO: Ahora verificamos si NO está bloqueada (!isLocked) antes de permitir encender/apagar
        if (Input.GetMouseButtonDown(1) && !isLocked)
        {
            TryToggle();
        }

        if (flashlight != null && flashlight.enabled && currentCharge > 0f)
        {
            float drain = drainPerSecond;

            if (FlashlightController.playerIsUsingDefensiveLight)
                drain *= defensiveDrainMultiplier;

            currentCharge -= drain * Time.deltaTime;

            if (currentCharge <= 0f)
            {
                currentCharge = 0f;
                flashlight.enabled = false;

                if (FlashlightController.playerIsUsingDefensiveLight)
                    FlashlightController.playerIsUsingDefensiveLight = false;
            }

            NotifyUI();
        }

        // El parpadeo por batería baja solo ocurre si NO estamos bloqueados por un evento
        if (!isLocked) 
        {
            ApplyFlicker();
        }
    }

    public void AddCharge(float amount)
    {
        if (amount <= 0f) return;
        currentCharge = Mathf.Clamp(currentCharge + amount, 0f, maxCharge);
        NotifyUI();
    }

    void TryToggle()
    {
        if (flashlight == null) return;
        if (!flashlight.enabled && currentCharge <= 0f) return;

        flashlight.enabled = !flashlight.enabled;

        if (audioSource != null)
            audioSource.Play();

        if (!flashlight.enabled && FlashlightController.playerIsUsingDefensiveLight)
            FlashlightController.playerIsUsingDefensiveLight = false;
    }

    void ApplyFlicker()
    {
        if (flashlight == null) return;
        if (!flashlight.enabled) return;

        float t = ChargePercent01;
        bool low = enableFlicker && t <= lowBatteryThreshold && currentCharge > 0f;

        if (!low)
        {
            flashlight.intensity = 1.5f; // Asumiendo intensidad base
            return;
        }

        float n = Mathf.PerlinNoise(Time.time * flickerNoiseFreq, 0f);
        float depth = Mathf.Clamp01(flickerNoiseDepth);
        float flickerFactor = Mathf.Lerp(1f - depth, 1f, n);
        float intensityWithNoise = 1.5f * flickerFactor;

        if (_microBlinkTimer > 0f)
        {
            _microBlinkTimer -= Time.deltaTime;
            flashlight.intensity = 0f;
        }
        else
        {
            flashlight.intensity = intensityWithNoise;
            float p = microBlinkChancePerSec * Time.deltaTime;
            if (UnityEngine.Random.value < p)
            {
                _microBlinkTimer = UnityEngine.Random.Range(microBlinkDuration.x, microBlinkDuration.y);
            }
        }
    }

    void NotifyUI() => OnBatteryChanged?.Invoke(currentCharge, maxCharge);
}