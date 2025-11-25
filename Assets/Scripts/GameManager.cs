using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEditor;

public class GameManager : MonoBehaviour
{
    // --- VARIABLE DE REINICIO (Estática) ---
    private static bool isRestarting = false;
    // ---------------------------------------

    [Header("Paneles de UI")]
    public GameObject mainMenuPanel;
    public GameObject gameHUDPanel;
    public GameObject pauseMenuPanel;
    public GameObject settingsPanel;    // Panel de Ajustes

    [Header("Control del Jugador")]
    public GameObject firstPersonController;

    [Header("Control del Enemigo")]
    public GameObject monster; // Arrastra aquí a tu monstruo

    [Header("Scripts para Pausar")]
    // Arrastra aquí los scripts de movimiento, cámara, etc.
    public MonoBehaviour[] playerInputScripts;

    [Header("Configuración de Ajustes - Audio / Pantalla")]
    public Slider volumeSlider;
    public TextMeshProUGUI volumeText;        // Texto que muestra "100%"
    public Slider brightnessSlider;
    public TextMeshProUGUI brightnessText;    // Texto que muestra "100%"
    public Image brightnessFilter;            // Imagen negra para el filtro de brillo
    public GameObject menuBackground;         // Imagen de fondo del menú

    [Header("Referencias de IA / Monstruo")]
    [Tooltip("Referencia al script MonsterAI del enemigo.")]
    public MonsterAI monsterAI;
    [Tooltip("Referencia al script MonsterVisionCone del enemigo (visión).")]
    public MonsterVisionCone monsterVision;
    [Tooltip("Referencia al script MonsterHearing del enemigo (oído).")]
    public MonsterHearing monsterHearing;

    [Header("Ajustes de IA (Sliders en el menú)")]
    [Tooltip("Slider para la velocidad de movimiento del monstruo (multiplicador).")]
    public Slider speedSlider;
    public TextMeshProUGUI speedText;

    [Tooltip("Slider para el rango de visión del monstruo.")]
    public Slider visionSlider;
    public TextMeshProUGUI visionText;

    [Tooltip("Slider para la sensibilidad auditiva del monstruo.")]
    public Slider hearingSlider;
    public TextMeshProUGUI hearingText;

    [Tooltip("Slider para el tiempo de reacción del monstruo.")]
    public Slider reactionSlider;
    public TextMeshProUGUI reactionText;

    private string currentSceneName;
    private bool isPaused = false;

    void Awake()
    {
        // Guarda el nombre de la escena actual para poder reiniciarla
        currentSceneName = SceneManager.GetActiveScene().name;

        // Intentar autoconectar componentes de la IA si no se asignaron
        if (monster != null)
        {
            if (monsterAI == null)
                monsterAI = monster.GetComponent<MonsterAI>();

            if (monsterVision == null)
                monsterVision = monster.GetComponentInChildren<MonsterVisionCone>();

            if (monsterHearing == null)
                monsterHearing = monster.GetComponent<MonsterHearing>();
        }
    }

    void Start()
    {
        AudioListener.pause = false;

        // --- LÓGICA DE REINICIO ---
        if (isRestarting)
        {
            // Si venimos de un reinicio, saltamos el menú y empezamos directo
            StartGame();
            isRestarting = false; // Reseteamos para la próxima vez
        }
        else
        {
            // Si es inicio normal, mostramos el menú
            ShowMainMenu();
        }
        // --------------------------

        // Inicializar sliders de volumen / brillo
        if (volumeSlider != null)
        {
            volumeSlider.onValueChanged.AddListener(SetVolume);
            volumeSlider.value = AudioListener.volume;
            UpdateVolumeText(AudioListener.volume);
        }

        if (brightnessSlider != null)
        {
            brightnessSlider.onValueChanged.AddListener(SetBrightness);
            float defaultBrightness = 0.5f;
            brightnessSlider.value = defaultBrightness;
            SetBrightness(defaultBrightness);
        }

        // --- Inicialización de sliders de IA ---

        // Velocidad de movimiento (multiplicador)
        if (speedSlider != null && monsterAI != null)
        {
            speedSlider.onValueChanged.AddListener(SetMonsterSpeed);
            speedSlider.value = monsterAI.MovementSpeedMultiplier;
            UpdateSpeedText(speedSlider.value);
        }

        // Rango de visión
        if (visionSlider != null && monsterVision != null)
        {
            visionSlider.onValueChanged.AddListener(SetMonsterVisionRange);
            visionSlider.value = monsterVision.VisionRange;
            UpdateVisionText(visionSlider.value);
        }

        // Sensibilidad auditiva
        if (hearingSlider != null && monsterHearing != null)
        {
            hearingSlider.onValueChanged.AddListener(SetMonsterHearingSensitivity);
            hearingSlider.value = monsterHearing.HearingSensitivity;
            UpdateHearingText(hearingSlider.value);
        }

        // Tiempo de reacción
        if (reactionSlider != null && monsterAI != null)
        {
            reactionSlider.onValueChanged.AddListener(SetMonsterReactionTime);
            reactionSlider.value = monsterAI.ReactionTime;
            UpdateReactionText(reactionSlider.value);
        }
    }

    void Update()
    {
        // Si el jugador está activo Y no estamos en el menú de ajustes
        if (firstPersonController.activeSelf && !settingsPanel.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (isPaused)
                    ResumeGame();
                else
                    PauseGame();
            }
        }
        // Si estamos en el menú de ajustes
        else if (settingsPanel.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            // La tecla Escape ahora debe "Volver"
            HideSettings();
        }
    }

    // --- FUNCIONES PÚBLICAS ---

    public void StartGame()
    {
        mainMenuPanel.SetActive(false);
        gameHUDPanel.SetActive(true);
        pauseMenuPanel.SetActive(false);
        settingsPanel.SetActive(false);

        // Al jugar, ocultamos la foto para ver el juego 3D
        if (menuBackground != null) menuBackground.SetActive(false);

        firstPersonController.SetActive(true);
        SetPlayerInput(true);

        // Liberamos al monstruo para que empiece a cazar
        if (monster != null) monster.SetActive(true);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        isPaused = false;
        Time.timeScale = 1f;
        AudioListener.pause = false;
    }

    public void QuitGame()
    {
        Debug.Log("Saliendo del juego...");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void ResumeGame()
    {
        pauseMenuPanel.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;

        AudioListener.pause = false;
        SetPlayerInput(true);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void RestartGame()
    {
        // Activamos la "memoria" antes de recargar la escena
        isRestarting = true;

        Time.timeScale = 1f;
        AudioListener.pause = false;
        isPaused = false;
        SceneManager.LoadScene(currentSceneName);
    }

    public void QuitToMainMenu()
    {
        Time.timeScale = 1f;
        isPaused = false;
        AudioListener.pause = false;
        ShowMainMenu();
    }

    public void ShowSettings()
    {
        mainMenuPanel.SetActive(false);
        pauseMenuPanel.SetActive(false);
        settingsPanel.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // --- LÓGICA PARA EL FONDO ---
        if (isPaused)
        {
            // Si venimos de la pausa (estamos jugando):
            Time.timeScale = 0f;
            AudioListener.pause = true;

            // OCULTAMOS la foto para ver el juego de fondo
            if (menuBackground != null) menuBackground.SetActive(false);
        }
        else
        {
            // Si venimos del menú principal (no estamos jugando):
            // MOSTRAMOS la foto para no ver el fondo vacío
            if (menuBackground != null) menuBackground.SetActive(true);
        }
        // ----------------------------
    }

    public void HideSettings()
    {
        settingsPanel.SetActive(false);

        if (isPaused)
        {
            PauseGame();
        }
        else
        {
            ShowMainMenu();
        }
    }

    // --- FUNCIONES INTERNAS ---

    private void PauseGame()
    {
        pauseMenuPanel.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;

        AudioListener.pause = true;
        SetPlayerInput(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
        gameHUDPanel.SetActive(false);
        pauseMenuPanel.SetActive(false);
        settingsPanel.SetActive(false);

        // Al volver al menú, activamos la foto
        if (menuBackground != null) menuBackground.SetActive(true);

        firstPersonController.SetActive(false);
        SetPlayerInput(false);

        // Mantenemos al monstruo apagado/dormido en el menú
        if (monster != null) monster.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void SetPlayerInput(bool enabled)
    {
        foreach (MonoBehaviour script in playerInputScripts)
        {
            if (script != null)
                script.enabled = enabled;
        }
    }

    // --- AJUSTES AUDIO / BRILLO ---

    public void SetVolume(float volume)
    {
        AudioListener.volume = volume;
        UpdateVolumeText(volume);
    }

    private void UpdateVolumeText(float volume)
    {
        if (volumeText != null)
            volumeText.text = Mathf.Round(volume * 100) + "%";
    }

    public void SetBrightness(float brightness)
    {
        // El slider va de 0 (oscuro) a 1 (normal).
        // maxAlpha define qué tan oscuro será el mínimo. 0.8 = 80% negro.
        float maxAlpha = 0.8f;

        // Invertimos el valor:
        // brightness 1 -> alpha 0 (claro)
        // brightness 0 -> alpha 0.8 (oscuro)
        float targetAlpha = (1.0f - brightness) * maxAlpha;

        if (brightnessFilter != null)
        {
            Color filterColor = brightnessFilter.color;
            filterColor.a = targetAlpha;
            brightnessFilter.color = filterColor;
        }

        UpdateBrightnessText(brightness);
    }

    private void UpdateBrightnessText(float brightness)
    {
        if (brightnessText != null)
            brightnessText.text = Mathf.Round(brightness * 100) + "%";
    }

    // --- AJUSTES DE IA (LLAMADOS POR SLIDERS) ---

    public void SetMonsterSpeed(float multiplier)
    {
        if (monsterAI != null)
            monsterAI.SetMovementSpeedMultiplier(multiplier);

        UpdateSpeedText(multiplier);
    }

    private void UpdateSpeedText(float multiplier)
    {
        if (speedText != null)
            speedText.text = multiplier.ToString("0.0") + "x";
    }

    public void SetMonsterVisionRange(float range)
    {
        if (monsterVision != null)
            monsterVision.SetVisionRange(range);

        UpdateVisionText(range);
    }

    private void UpdateVisionText(float range)
    {
        if (visionText != null)
            visionText.text = Mathf.Round(range).ToString("0") + " u";
    }

    public void SetMonsterHearingSensitivity(float value)
    {
        if (monsterHearing != null)
            monsterHearing.SetHearingSensitivity(value);

        UpdateHearingText(value);
    }

    private void UpdateHearingText(float value)
    {
        if (hearingText != null)
        {
            float pct = value * 100f;
            hearingText.text = Mathf.Round(pct).ToString("0") + "%";
        }
    }

    public void SetMonsterReactionTime(float seconds)
    {
        if (monsterAI != null)
            monsterAI.SetReactionTime(seconds);

        UpdateReactionText(seconds);
    }

    private void UpdateReactionText(float seconds)
    {
        if (reactionText != null)
            reactionText.text = seconds.ToString("0.00") + " s";
    }
}
