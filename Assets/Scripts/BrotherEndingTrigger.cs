using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;   // 👈 IMPORTANTE

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

    [Header("Movimiento del hermano")]
    [SerializeField] private BrotherFollowPlayer brotherFollower;

    [Header("Audio")]
    [Tooltip("AudioSource que está en el niño (3D).")]
    [SerializeField] private AudioSource brotherAudioSource;

    [Tooltip("Clip de voz: 'BROTHER, YOU FOUND ME!'")]
    [SerializeField] private AudioClip brotherFoundClip;

    [Tooltip("AudioSource para la voz/efecto del fin del juego (2D, en Canvas o Cámara).")]
    [SerializeField] private AudioSource endGameAudioSource;

    [Tooltip("Clip de voz: 'END OF THE GAME'")]
    [SerializeField] private AudioClip endGameClip;

    [Header("Tiempos")]
    [SerializeField] private float delayBeforeEndPanel = 2f;
    [SerializeField] private float delayBeforeReturnToMenu = 5f;   // 👈 nuevos 5 s

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

        // Hacer que el hermano camine hacia el jugador
        if (brotherFollower != null)
        {
            brotherFollower.StartFollowing(other.transform);
        }

        StartCoroutine(EndSequence(other));
    }

    private IEnumerator EndSequence(Collider player)
    {
        // 1) Bocadillo del hermano
        if (brotherSpeechCanvas != null)
            brotherSpeechCanvas.gameObject.SetActive(true);

        if (brotherSpeechText != null)
            brotherSpeechText.text = "BROTHER, YOU FOUND ME!";

        if (brotherAudioSource != null && brotherFoundClip != null)
            brotherAudioSource.PlayOneShot(brotherFoundClip);

        // Esperamos antes de mostrar el cartel rojo
        yield return new WaitForSecondsRealtime(delayBeforeEndPanel);

        // 2) Cartel "END OF THE GAME"
        if (endGamePanel != null)
            endGamePanel.SetActive(true);

        if (endGameTitleText != null)
            endGameTitleText.text = "END OF THE GAME";

        if (endGameAudioSource != null && endGameClip != null)
            endGameAudioSource.PlayOneShot(endGameClip);

        // Mostrar cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        GameProgressManager.Instance?.CompleteGame();
        GameProgressManager.Instance?.ForceHideObjective();  // por si hay textos de objetivo

        // 3) Esperar 5 s y volver al menú
        yield return new WaitForSecondsRealtime(delayBeforeReturnToMenu);

        // Recargar la escena actual → vuelve al menú principal
        var scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.buildIndex);
    }
}
