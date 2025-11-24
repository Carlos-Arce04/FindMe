using UnityEngine;

public class GameStateUIManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject mainMenuCanvas;     // Menú principal
    [SerializeField] private GameObject hudCanvas;          // Canvas del juego (Progreso, sangre, etc.)
    [SerializeField] private GameObject progresoDelJuego;   // Objeto donde está GameProgressManager

    [Header("Player")]
    [SerializeField] private GameObject playerRoot;         // FPS Controller / Player

    private void Start()
    {
        // Estado inicial: solo menú.
        mainMenuCanvas.SetActive(true);
        hudCanvas.SetActive(false);
        if (progresoDelJuego != null) progresoDelJuego.SetActive(false);
        if (playerRoot != null) playerRoot.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void OnStartGameButton()
    {
        mainMenuCanvas.SetActive(false);
        hudCanvas.SetActive(true);
        if (progresoDelJuego != null) progresoDelJuego.SetActive(true);
        if (playerRoot != null) playerRoot.SetActive(true);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
