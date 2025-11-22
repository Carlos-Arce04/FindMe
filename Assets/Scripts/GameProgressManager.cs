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
    [SerializeField] private GameObject basementBarrier; // Barrera del sótano (si ya la tienes)

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI objectiveText; // Texto a la derecha
    [SerializeField] private Image porcelainDollIcon;       // Icono tipo trofeo

    [Header("Progreso (solo debug)")]
    [SerializeField, Range(0,100)]
    private int currentProgressPercent = 0;

    private int keysCollected = 0;
    private bool floor3Unlocked = false;
    private bool dollCollected = false;
    private bool basementUnlocked = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        // Si quieres que sobreviva entre escenas:
        // DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (porcelainDollIcon != null)
        {
            porcelainDollIcon.gameObject.SetActive(false);
        }

        ShowMessage(GameStrings.OBJ_FIND_KEYS);
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

        AddProgress(30); // % que quieras para este hito
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
        // Aquí luego puedes usar currentProgressPercent para dificultad adaptativa.
    }

    private void ShowMessage(string message)
    {
        if (objectiveText != null)
        {
            objectiveText.text = message;
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
