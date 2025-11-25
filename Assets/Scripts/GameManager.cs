using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEditor;
using UnityEngine.Video; 
using UnityEngine.EventSystems; 

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
    [Tooltip("Panel que muestra las instrucciones o mapeo de teclas.")]
    public GameObject instructionsPanel; 

    [Header("Control del Jugador")]
    public GameObject firstPersonController;

    [Header("Control del Enemigo")]
    public GameObject monster;

    [Header("Scripts para Pausar")]
    public MonoBehaviour[] playerInputScripts;

    // --- GAME OBJECTS DE GENERACIÓN DE ITEMS (MODIFICADO) ---
    [Header("GameObjects de Generación de Items")]
    [Tooltip("Referencia al GameObject KeyManager.")]
    public GameObject keyManager; // <--- MODIFICADO
    [Tooltip("Referencia al GameObject BatterySpawnerArea.")]
    public GameObject batterySpawnerArea; // <--- MODIFICADO
    // --------------------------------------------------------

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
    public MonsterAI monsterAI;
    public MonsterVisionCone monsterVision;
    public MonsterHearing monsterHearing;

    [Header("Ajustes de IA (Sliders en el menú)")]
    public Slider speedSlider;
    public TextMeshProUGUI speedText;
    public Slider visionSlider;
    public TextMeshProUGUI visionText;
    public Slider hearingSlider;
    public TextMeshProUGUI hearingText;
    public Slider reactionSlider;
    public TextMeshProUGUI reactionText;
    // --------------------------------------------------------

    private string currentSceneName;
    private bool isPaused = false;

    void Awake()
    {
        currentSceneName = SceneManager.GetActiveScene().name;

        // --- LÓGICA DE VIDEO ---
        if (videoScreen != null)
        {
            videoPlayer = videoScreen.GetComponent<VideoPlayer>();
            videoRawImage = videoScreen.GetComponent<RawImage>();
        }
        // ---------------------------

        // --- LÓGICA DE IA ---
        if (monster != null)
        {
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
            LoadGameLogic();
        }
        else
        {
            ShowMainMenu();

            // --- TRUCO DE PRE-CARGA DE VIDEO ---
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

        // --- Inicialización de sliders de IA ---
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
        // Lógica de ESCAPE principal (Solo si no estamos en Ajustes o Instrucciones)
        if (firstPersonController.activeSelf && !settingsPanel.activeSelf && (instructionsPanel == null || !instructionsPanel.activeSelf))
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (isPaused) ResumeGame();
                else PauseGame();
            }
        }
        // Cerrar Ajustes con Escape
        else if (settingsPanel.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            HideSettings();
        }
        // Cerrar Instrucciones con Escape
        else if (instructionsPanel != null && instructionsPanel.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            HideInstructions();
        }
    }

    // --- FUNCIONES PÚBLICAS ---

    public void StartGame()
    {
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(false);
        if (instructionsPanel != null) instructionsPanel.SetActive(false); 
        
        // --- LÓGICA DE VIDEO (Se reproduce al iniciar) ---
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
            Debug.LogWarning("No hay VideoPlayer o clip, iniciando juego directo.");
            LoadGameLogic();
        }
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
        ClearUISelection();
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
        ClearUISelection();
        Time.timeScale = 1f;
        isPaused = false;
        AudioListener.pause = false;
        ShowMainMenu();
    }

    public void ShowSettings()
    {
        ClearUISelection();
        mainMenuPanel.SetActive(false);
        pauseMenuPanel.SetActive(false);
        if (instructionsPanel != null) instructionsPanel.SetActive(false); 
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
        ClearUISelection();
        settingsPanel.SetActive(false);
        if (isPaused) PauseGame();
        else ShowMainMenu();
    }

    // --- FUNCIONES DE INSTRUCCIONES ---

    public void ShowInstructions()
    {
        if (instructionsPanel == null) return; 

        ClearUISelection();
        
        mainMenuPanel.SetActive(false);
        pauseMenuPanel.SetActive(false);
        settingsPanel.SetActive(false);
        instructionsPanel.SetActive(true); 
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Lógica de fondo dinámico (como en ShowSettings)
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

    public void HideInstructions()
    {
        if (instructionsPanel == null) return; 

        ClearUISelection();
        instructionsPanel.SetActive(false);
        
        // Vuelve al menú anterior
        if (isPaused) PauseGame();
        else ShowMainMenu();
    }
    
    // --- FUNCIONES INTERNAS ---

    private void OnVideoFinished(VideoPlayer vp)
    {
        vp.loopPointReached -= OnVideoFinished;
        LoadGameLogic();
    }

    private void LoadGameLogic()
    {
        // 1. Ocultar Menús
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(false);
        if (instructionsPanel != null) instructionsPanel.SetActive(false); 
        if (menuBackground != null) menuBackground.SetActive(false);
        
        if (videoScreen != null) videoScreen.SetActive(false);

        // 2. Mostrar HUD y Ocultar Pausa
        gameHUDPanel.SetActive(true);
        pauseMenuPanel.SetActive(false);

        // 3. Activar Juego
        firstPersonController.SetActive(true);
        SetPlayerInput(true);
        if (monster != null) monster.SetActive(true);

        // --- ACTIVAR GAME OBJECTS DE GENERACIÓN DE ITEMS (MODIFICADO) ---
        if (keyManager != null) keyManager.SetActive(true);
        if (batterySpawnerArea != null) batterySpawnerArea.SetActive(true);
        // ----------------------------------------------------------------

        // 4. Estado de Juego
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        isPaused = false;
        Time.timeScale = 1f;
        AudioListener.pause = false;
    }

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
        ClearUISelection();
        mainMenuPanel.SetActive(true);
        gameHUDPanel.SetActive(false);
        pauseMenuPanel.SetActive(false);
        settingsPanel.SetActive(false);
        if (instructionsPanel != null) instructionsPanel.SetActive(false); 

        if (menuBackground != null) menuBackground.SetActive(true);
        
        // --- LÓGICA DE TU VIDEO ---
        if (videoScreen != null)
        {
            videoScreen.SetActive(true);
            if (videoRawImage != null) videoRawImage.enabled = false;
        }
        // --------------------------------------------------------------------------------------

        firstPersonController.SetActive(false);
        SetPlayerInput(false);
        if (monster != null) monster.SetActive(false);

        // --- DESACTIVAR GAME OBJECTS DE GENERACIÓN DE ITEMS (MODIFICADO) ---
        if (keyManager != null) keyManager.SetActive(false);
        if (batterySpawnerArea != null) batterySpawnerArea.SetActive(false);
        // --------------------------------------------------------------------

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

    // --- FUNCIÓN PARA ELIMINAR EL FOCO (COLOR ROJO) DE LA UI ---
    private void ClearUISelection()
    {
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }
    // ---------------------------------------------------------

    // --- AJUSTES AUDIO / BRILLO ---

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