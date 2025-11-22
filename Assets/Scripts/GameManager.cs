using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEditor;
using UnityEngine.Video; // Tu adición para el video

public class GameManager : MonoBehaviour
{
    // --- VARIABLE DE REINICIO (Estática) ---
    private static bool isRestarting = false;
    // ---------------------------------------

    [Header("Paneles de UI")]
    public GameObject mainMenuPanel;
    public GameObject gameHUDPanel;
    public GameObject pauseMenuPanel;
    public GameObject settingsPanel;

    [Header("Control del Jugador")]
    public GameObject firstPersonController;

    [Header("Control del Enemigo")]
    public GameObject monster;

    [Header("Scripts para Pausar")]
    public MonoBehaviour[] playerInputScripts;

    [Header("Configuración de Ajustes - Audio / Pantalla")]
    public Slider volumeSlider;
    public TextMeshProUGUI volumeText;
    public Slider brightnessSlider;
    public TextMeshProUGUI brightnessText;
    public Image brightnessFilter;
    public GameObject menuBackground;

    // --- TUS VARIABLES DE VIDEO ---
    [Header("Video Setup")]
    public GameObject videoScreen;
    private VideoPlayer videoPlayer;
    private RawImage videoRawImage;
    // ------------------------------

    // --- VARIABLES DE LA IA DEL MONSTRUO (Tu compañero) ---
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
    // --------------------------------------------------------

    private string currentSceneName;
    private bool isPaused = false;

    void Awake()
    {
        currentSceneName = SceneManager.GetActiveScene().name;

        // --- LÓGICA DE TU VIDEO ---
        if (videoScreen != null)
        {
            videoPlayer = videoScreen.GetComponent<VideoPlayer>();
            videoRawImage = videoScreen.GetComponent<RawImage>();
        }
        // ---------------------------

        // --- LÓGICA DE IA DE TU COMPAÑERO ---
        if (monster != null)
        {
            // Nota: Aquí asumimos que los scripts 'MonsterAI', 'MonsterVisionCone', y 'MonsterHearing' existen.
            if (monsterAI == null)
                monsterAI = monster.GetComponent<MonsterAI>();

            if (monsterVision == null)
                monsterVision = monster.GetComponentInChildren<MonsterVisionCone>();

            if (monsterHearing == null)
                monsterHearing = monster.GetComponent<MonsterHearing>();
        }
        // ------------------------------------
    }

    void Start()
    {
        AudioListener.pause = false;

        if (isRestarting)
        {
            isRestarting = false;
            // Usamos tu función para empezar el juego después del reinicio
            LoadGameLogic(); 
        }
        else
        {
            ShowMainMenu();

            // --- TRUCO DE PRE-CARGA DE VIDEO (Tu adición) ---
            if (videoPlayer != null)
            {
                videoScreen.SetActive(true); 
                if (videoRawImage != null) videoRawImage.enabled = false;
                videoPlayer.Prepare();
            }
            // ------------------------------------------------
        }

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

        // --- Inicialización de sliders de IA (Tu compañero) ---
        if (speedSlider != null && monsterAI != null)
        {
            speedSlider.onValueChanged.AddListener(SetMonsterSpeed);
            speedSlider.value = monsterAI.MovementSpeedMultiplier;
            UpdateSpeedText(speedSlider.value);
        }

        if (visionSlider != null && monsterVision != null)
        {
            visionSlider.onValueChanged.AddListener(SetMonsterVisionRange);
            visionSlider.value = monsterVision.VisionRange;
            UpdateVisionText(visionSlider.value);
        }

        if (hearingSlider != null && monsterHearing != null)
        {
            hearingSlider.onValueChanged.AddListener(SetMonsterHearingSensitivity);
            hearingSlider.value = monsterHearing.HearingSensitivity;
            UpdateHearingText(hearingSlider.value);
        }

        if (reactionSlider != null && monsterAI != null)
        {
            reactionSlider.onValueChanged.AddListener(SetMonsterReactionTime);
            reactionSlider.value = monsterAI.ReactionTime;
            UpdateReactionText(reactionSlider.value);
        }
        // -------------------------------------------------------
    }

    void Update()
    {
        // Lógica de ESCAPE (igual en ambas versiones)
        if (firstPersonController.activeSelf && !settingsPanel.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (isPaused) ResumeGame();
                else PauseGame();
            }
        }
        else if (settingsPanel.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            HideSettings();
        }
    }

    // --- FUNCIONES PÚBLICAS ---

    public void StartGame()
    {
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(false);

        // --- LÓGICA DE TU VIDEO (Se reproduce al iniciar) ---
        if (videoPlayer != null && videoScreen != null && videoPlayer.clip != null)
        {
            if (menuBackground != null) menuBackground.SetActive(false);
            
            videoScreen.SetActive(true);
            if (videoRawImage != null) videoRawImage.enabled = true;
            
            videoPlayer.loopPointReached += OnVideoFinished;
            videoPlayer.Play();
        }
        else
        {
            // Si no hay video, se salta a la lógica de inicio de juego
            Debug.LogWarning("No hay VideoPlayer o clip, iniciando juego directo.");
            LoadGameLogic();
        }
        // ---------------------------------------------------
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

        if (isPaused)
        {
            Time.timeScale = 0f;
            AudioListener.pause = true;
            if (menuBackground != null) menuBackground.SetActive(false);
        }
        else
        {
            if (menuBackground != null) menuBackground.SetActive(true);
        }
    }

    public void HideSettings()
    {
        settingsPanel.SetActive(false);
        if (isPaused) PauseGame();
        else ShowMainMenu();
    }

    // --- FUNCIONES INTERNAS DE VIDEO (Tus adiciones) ---
    private void OnVideoFinished(VideoPlayer vp)
    {
        vp.loopPointReached -= OnVideoFinished;
        LoadGameLogic();
    }

    private void LoadGameLogic()
    {
        // Esta función es necesaria para iniciar el juego después de que el video termine.

        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(false);
        if (menuBackground != null) menuBackground.SetActive(false);
        
        // Apagamos completamente el video al terminar el juego
        if (videoScreen != null) videoScreen.SetActive(false); 

        gameHUDPanel.SetActive(true);
        pauseMenuPanel.SetActive(false);

        firstPersonController.SetActive(true);
        SetPlayerInput(true);
        if (monster != null) monster.SetActive(true);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        isPaused = false;
        Time.timeScale = 1f;
        AudioListener.pause = false;
    }
    // ----------------------------------------------------

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

        if (menuBackground != null) menuBackground.SetActive(true);
        
        // --- LÓGICA DE TU VIDEO (asegura que el video esté oculto pero activo para pre-carga) ---
        if (videoScreen != null)
        {
            videoScreen.SetActive(true); 
            if (videoRawImage != null) videoRawImage.enabled = false; 
        }
        // --------------------------------------------------------------------------------------

        firstPersonController.SetActive(false);
        SetPlayerInput(false);
        if (monster != null) monster.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void SetPlayerInput(bool enabled)
    {
        foreach (MonoBehaviour script in playerInputScripts)
        {
            if (script != null) script.enabled = enabled;
        }
    }

    // --- AJUSTES AUDIO / BRILLO (Funciones de ambas versiones) ---

    public void SetVolume(float volume)
    {
        AudioListener.volume = volume;
        UpdateVolumeText(volume);
    }

    private void UpdateVolumeText(float volume)
    {
        if (volumeText != null) volumeText.text = Mathf.Round(volume * 100) + "%";
    }

    public void SetBrightness(float brightness)
    {
        float maxAlpha = 0.8f;
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
        if (brightnessText != null) brightnessText.text = Mathf.Round(brightness * 100) + "%";
    }

    // --- AJUSTES DE IA (Tu compañero) ---

    public void SetMonsterSpeed(float multiplier)
    {
        if (monsterAI != null)
            // Asumo que tu compañero implementó este método en MonsterAI
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
            // Asumo que tu compañero implementó este método en MonsterVisionCone
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
            // Asumo que tu compañero implementó este método en MonsterHearing
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
            // Asumo que tu compañero implementó este método en MonsterAI
            monsterAI.SetReactionTime(seconds); 

        UpdateReactionText(seconds);
    }

    private void UpdateReactionText(float seconds)
    {
        if (reactionText != null)
            reactionText.text = seconds.ToString("0.00") + " s";
    }
}