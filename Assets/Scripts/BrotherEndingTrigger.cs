using UnityEngine;
using TMPro;

public class BrotherEndingTrigger : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private string playerTag = "Player";

    [Header("Bocadillo del hermano (World Space)")]
    [SerializeField] private Canvas brotherSpeechCanvas;
    [SerializeField] private TextMeshProUGUI brotherSpeechText;

    [Header("UI de fin de juego (HUD)")]
    [SerializeField] private GameObject endGamePanel;
    [SerializeField] private TextMeshProUGUI endGameTitleText;

    private bool hasTriggered = false;

    private void Start()
    {
        if (brotherSpeechCanvas != null)
            brotherSpeechCanvas.gameObject.SetActive(false);

        if (endGamePanel != null)
            endGamePanel.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered) return;
        if (!other.CompareTag(playerTag)) return;

        hasTriggered = true;

        // Mensaje del hermano en su bocadillo
        if (brotherSpeechCanvas != null)
            brotherSpeechCanvas.gameObject.SetActive(true);

        if (brotherSpeechText != null)
            brotherSpeechText.text = "Hermano, ¡me encontraste!";

        // Pantalla de fin de juego en grande
        if (endGamePanel != null)
            endGamePanel.SetActive(true);

        if (endGameTitleText != null)
            endGameTitleText.text = "FIN DEL JUEGO";

        // Opcional: aquí podrías desactivar el movimiento del jugador
        // var controller = other.GetComponent<FirstPersonController>();
        // if (controller != null) controller.enabled = false;

        // Mostrar el cursor para que el jugador pueda salir del juego/menú
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Opcional: marcar progreso al 100%
        GameProgressManager.Instance?.CompleteGame();
    }
}
