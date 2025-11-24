using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameProgressManager : MonoBehaviour
{
    public static GameProgressManager Instance { get; private set; }

    [Header("Objetivos")]
    [SerializeField] private int keysNeeded = 3;

    [Header("Barreras / zonas bloqueadas")]
    [SerializeField] private GameObject floor3Barrier;   // Cinta amarilla piso 3
    [SerializeField] private GameObject basementBarrier; // Barrera del sótano

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI objectiveText; // Texto a la derecha
    [SerializeField] private Image porcelainDollIcon;       // Icono tipo trofeo

    [Header("Objetivos - visualización")]
    [Tooltip("Segundos que el texto de objetivo permanece visible.")]
    [SerializeField] private float objectiveDisplayTime = 2f;

    [Header("Progreso (solo debug)")]
    [SerializeField, Range(0, 100)]
    private int currentProgressPercent = 0;

    private int keysCollected = 0;
    private bool floor3Unlocked = false;
    private bool dollCollected = false;
    private bool basementUnlocked = false;

    // Control de texto de objetivos
    private string lastObjectiveMessage = "";
    private float objectiveTimer = 0f;
    private bool objectiveVisible = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        // DontDestroyOnLoad(gameObject);  // si lo quieres entre escenas
    }

    private void Start()
    {
        if (porcelainDollIcon != null)
        {
            porcelainDollIcon.gameObject.SetActive(false);
        }

        if (objectiveText != null)
        {
            objectiveText.gameObject.SetActive(false);
        }

        // Primer objetivo al iniciar
        ShowMessage(GameStrings.OBJ_FIND_KEYS);
    }

    private void Update()
    {
        if (objectiveVisible && objectiveText != null)
        {
            objectiveTimer -= Time.unscaledDeltaTime;

            if (objectiveTimer <= 0f)
            {
                objectiveText.gameObject.SetActive(false);
                objectiveVisible = false;
            }
        }

        if (Input.GetKeyDown(KeyCode.I))
        {
            if (!string.IsNullOrEmpty(lastObjectiveMessage))
            {
                ShowMessage(lastObjectiveMessage);
            }
        }
    }

    // === LLAVES ==========================================================

    public void RegisterKeyCollected()
    {
        keysCollected++;

        if (!floor3Unlocked && keysCollected >= keysNeeded)
        {
            UnlockFloor3();
        }
    }

    private void UnlockFloor3()
    {
        floor3Unlocked = true;

        if (floor3Barrier != null)
        {
            floor3Barrier.SetActive(false);  // Se “quita” la cinta amarilla
        }

        AddProgress(30);
        ShowMessage(GameStrings.OBJ_GO_TO_FLOOR3);
    }

    // === MUÑECO DE PORCELANA =============================================

    public void RegisterPorcelainDollCollected(Sprite dollSprite = null)
    {
        if (dollCollected) return;

        dollCollected = true;

        if (porcelainDollIcon != null)
        {
            if (dollSprite != null)
            {
                porcelainDollIcon.sprite = dollSprite;
            }

            porcelainDollIcon.gameObject.SetActive(true); // Muestra el “trofeo”
        }

        UnlockBasement();
    }

    private void UnlockBasement()
    {
        basementUnlocked = true;

        if (basementBarrier != null)
        {
            basementBarrier.SetActive(false);
        }

        AddProgress(30);
        ShowMessage(GameStrings.OBJ_BASEMENT_UNLOCKED);
    }

    // === UTILIDADES ======================================================

    private void AddProgress(int amount)
    {
        currentProgressPercent = Mathf.Clamp(currentProgressPercent + amount, 0, 100);
    }

    /// <summary>
    /// Muestra un mensaje de objetivo durante objectiveDisplayTime segundos.
    /// </summary>
    private void ShowMessage(string message)
    {
        lastObjectiveMessage = message;

        if (objectiveText == null) return;

        objectiveText.text = message;
        objectiveText.gameObject.SetActive(true);

        objectiveTimer = objectiveDisplayTime;
        objectiveVisible = true;
    }

    /// <summary>
    /// Oculta el texto de objetivo inmediatamente.
    /// </summary>
    public void ForceHideObjective()
    {
        objectiveVisible = false;
        objectiveTimer = 0f;

        if (objectiveText != null)
        {
            objectiveText.gameObject.SetActive(false);
        }
    }

    public int GetProgressPercent()
    {
        return currentProgressPercent;
    }

    public void CompleteGame()
    {
        currentProgressPercent = 100;
    }
}
