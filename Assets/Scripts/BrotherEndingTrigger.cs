using UnityEngine;
using TMPro;
using System.Collections;

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

    [Header("Tiempos")]
    [SerializeField] private float delayBeforeEndPanel = 2f;

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
        StartCoroutine(EndSequence(other));
    }

    private IEnumerator EndSequence(Collider player)
    {
        // 1) Bocadillo del hermano
        if (brotherSpeechCanvas != null)
            brotherSpeechCanvas.gameObject.SetActive(true);

        if (brotherSpeechText != null)
            brotherSpeechText.text = "Hermano, ¡me encontraste!";

        // Esperar para que el jugador lo lea
        yield return new WaitForSeconds(delayBeforeEndPanel);

        // 2) Mostrar panel de FIN DEL JUEGO en el HUD
        if (endGamePanel != null)
            endGamePanel.SetActive(true);

        if (endGameTitleText != null)
            endGameTitleText.text = "FIN DEL JUEGO";

        // (Opcional) bloquear movimiento
        // var controller = player.GetComponent<FirstPersonController>();
        // if (controller != null) controller.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        GameProgressManager.Instance?.CompleteGame();
    }
}
