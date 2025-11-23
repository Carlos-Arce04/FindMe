using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth;
    public float iFramesAfterHit = 0.35f;
    public UnityEvent<float, float> onHealthChanged;

    float lastHitTime = -999f;
    bool dead;

    // UI opcional de Game Over (puede ser null)
    public GameObject gameOverUI;

    // Fade opcional (puede no existir en la escena)
    FadeToBlack fade;

    void Start()
    {
        currentHealth = maxHealth;
        onHealthChanged?.Invoke(currentHealth, maxHealth);

        // Si hay GameOverUI, la apagamos al inicio
        if (gameOverUI != null)
        {
            gameOverUI.SetActive(false);
        }
        else
        {
            Debug.LogWarning("PlayerHealth: GameOverUI no asignado en el inspector (no se mostrará pantalla de Game Over al morir).");
        }

        // Buscar FadeToBlack en la escena (si existe)
        fade = FindObjectOfType<FadeToBlack>();
        if (fade == null)
        {
            Debug.LogWarning("PlayerHealth: No existe FadeToBlack en la escena (no habrá fade al morir).");
        }
    }

    public void TakeDamage(float amount)
    {
        if (dead) return;
        if (Time.time < lastHitTime + iFramesAfterHit) return;

        lastHitTime = Time.time;
        currentHealth = Mathf.Max(0f, currentHealth - amount);

        BloodFlash blood = FindObjectOfType<BloodFlash>();
        if (blood != null) blood.ShowBlood();

        onHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0f)
        {
            StartCoroutine(HandleDeath());
        }
    }

    IEnumerator HandleDeath()
    {
        dead = true;

        // desactivar monstruo
        MonsterAttack monster = FindObjectOfType<MonsterAttack>();
        if (monster != null)
        {
            monster.enabled = false;
            monster.StopAllCoroutines();
        }

        // desactivar player movement scripts
        var move = GetComponent<FirstPersonMovement>();
        if (move != null) move.enabled = false;

        var jump = GetComponent<Jump>();
        if (jump != null) jump.enabled = false;

        var crouch = GetComponent<Crouch>();
        if (crouch != null) crouch.enabled = false;

        // activar UI de game over solo si existe
        if (gameOverUI != null)
        {
            gameOverUI.SetActive(true);
        }

        // iniciar fade solo si existe
        if (fade != null)
        {
            yield return StartCoroutine(fade.StartFade());
        }

        // pausar tiempo
        Time.timeScale = 0f;

        // usar tiempo real para esperar
        yield return new WaitForSecondsRealtime(2.5f);

        // restaurar tiempo y recargar
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
