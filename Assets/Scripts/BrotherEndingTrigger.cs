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

        // 🔊 Reproducir voz del hermano
        if (brotherAudioSource != null && brotherFoundClip != null)
        {
            brotherAudioSource.PlayOneShot(brotherFoundClip);
        }

        // Esperar para que se lea y se escuche bien
        yield return new WaitForSeconds(delayBeforeEndPanel);

        // 2) Mostrar panel de FIN DEL JUEGO
        if (endGamePanel != null)
            endGamePanel.SetActive(true);

        if (endGameTitleText != null)
            endGameTitleText.text = "END OF THE GAME";

        // 🔊 Reproducir voz / efecto de fin del juego
        if (endGameAudioSource != null && endGameClip != null)
        {
            endGameAudioSource.PlayOneShot(endGameClip);
        }

        // Opcional: bloquear movimiento del jugador aquí
        // var controller = player.GetComponent<FirstPersonController>();
        // if (controller != null) controller.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        GameProgressManager.Instance?.CompleteGame();
    }
}
