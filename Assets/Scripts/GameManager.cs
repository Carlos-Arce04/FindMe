using UnityEngine;
using UnityEngine.SceneManagement; 
using UnityEngine.UI; // ¡Importante! Para Slider y Image
using TMPro; // ¡Importante! Para TextMeshProUGUI
using UnityEditor;

public class GameManager : MonoBehaviour
{
    // --- ¡NUEVA VARIABLE PARA EL REINICIO! ---
    // static hace que esta variable NO se borre al recargar la escena
    private static bool isRestarting = false; 
    // -----------------------------------------

    [Header("Paneles de UI")]
    public GameObject mainMenuPanel;    
    public GameObject gameHUDPanel;     
    public GameObject pauseMenuPanel;   
    public GameObject settingsPanel;    // Panel de Ajustes

    [Header("Control del Jugador")]
    public GameObject firstPersonController; 

    [Header("Scripts para Pausar")]
    // Arrastra aquí los scripts de movimiento, cámara, etc.
    public MonoBehaviour[] playerInputScripts;

    [Header("Configuración de Ajustes")]
    public Slider volumeSlider;         
    public TextMeshProUGUI volumeText;  // El texto que SÓLO muestra "100%"
    public Slider brightnessSlider;     
    public TextMeshProUGUI brightnessText; // El texto que SÓLO muestra "100%"
    
    // Arrastra la imagen negra que cubre la pantalla (Filtro de Brillo)
    public Image brightnessFilter; 

    // Arrastra aquí tu objeto "BackgroundImage" (la foto de terror del menú)
    public GameObject menuBackground; 
    
    private string currentSceneName; 
    private bool isPaused = false;

    void Awake()
    {
        // Guarda el nombre de la escena actual para poder reiniciarla
        currentSceneName = SceneManager.GetActiveScene().name;
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

        // Inicializar los sliders de ajustes
        if (volumeSlider != null)
        {
            volumeSlider.onValueChanged.AddListener(SetVolume);
            volumeSlider.value = AudioListener.volume; 
            UpdateVolumeText(AudioListener.volume);
        }

        if (brightnessSlider != null)
        {
            brightnessSlider.onValueChanged.AddListener(SetBrightness);
            // Establecer un brillo por defecto (ej. 50%)
            float defaultBrightness = 0.5f;
            brightnessSlider.value = defaultBrightness; 
            // Llamar a SetBrightness para aplicar el alfa inicial del filtro
            SetBrightness(defaultBrightness); 
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
                {
                    ResumeGame();
                }
                else
                {
                    PauseGame();
                }
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

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        isPaused = false;
        Time.timeScale = 1f;
        AudioListener.pause = false; 
    }

    public void QuitGame()
    {
        Debug.Log("Saliendo del juego...");

        // Esto significa: "Si estoy corriendo dentro del Editor de Unity"
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        
        // Esto significa: "En cualquier otro caso" (como en un .exe)
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
        // --- ¡CAMBIO AQUÍ! ---
        // Activamos la "memoria" antes de recargar la escena
        isRestarting = true; 
        // ---------------------

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

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void SetPlayerInput(bool enabled)
    {
        foreach (MonoBehaviour script in playerInputScripts)
        {
            if (script != null)
            {
                script.enabled = enabled;
            }
        }
    }

    // --- AJUSTES ---

    public void SetVolume(float volume)
    {
        AudioListener.volume = volume;
        UpdateVolumeText(volume);
    }

    private void UpdateVolumeText(float volume)
    {
        if (volumeText != null)
        {
            volumeText.text = Mathf.Round(volume * 100) + "%";
        }
    }

    public void SetBrightness(float brightness)
    {
        // El slider va de 0 (oscuro) a 1 (normal).
        // maxAlpha define qué tan oscuro será el mínimo. 0.8 = 80% negro.
        float maxAlpha = 0.8f; 
        
        // Invertimos el valor:
        // Si brightness es 1 (max), targetAlpha = (1-1) * 0.8 = 0.0 (transparente)
        // Si brightness es 0 (min), targetAlpha = (1-0) * 0.8 = 0.8 (oscuro)
        float targetAlpha = (1.0f - brightness) * maxAlpha;

        if (brightnessFilter != null)
        {
            // Obtenemos el color actual del filtro (que debe ser negro)
            Color filterColor = brightnessFilter.color;
            
            // Cambiamos solo el valor 'a' (alpha/transparencia)
            filterColor.a = targetAlpha;
            
            // Asignamos el nuevo color con la transparencia actualizada
            brightnessFilter.color = filterColor;
        }

        // Actualizamos el texto del porcentaje
        UpdateBrightnessText(brightness);
    }

    private void UpdateBrightnessText(float brightness)
    {
        if (brightnessText != null)
        {
            // Solo muestra el porcentaje
            brightnessText.text = Mathf.Round(brightness * 100) + "%";
        }
    }
}