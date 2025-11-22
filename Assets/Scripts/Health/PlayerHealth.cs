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

    public GameObject gameOverUI;   // ✔ correcto
    FadeToBlack fade;               // ✔ correcto

    void Start()
    {
        currentHealth = maxHealth;
        onHealthChanged?.Invoke(currentHealth, maxHealth);

        // ❗ CORRECCIÓN IMPORTANTE
        // El campo gameOverUI AHORA YA NO SE BUSCA POR NOMBRE
        // Se asigna desde el inspector.
        if (gameOverUI != null)
            gameOverUI.SetActive(false);
        else
            Debug.LogError("⚠ No asignaste GameOverUI en el inspector!");

        // ✔ buscar el script FadeToBlack en la escena
        fade = FindObjectOfType<FadeToBlack>();
        if (fade == null)
            Debug.LogError("⚠ No existe FadeToBlack en la escena");
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

        // activar UI de game over
        if (gameOverUI != null)
            gameOverUI.SetActive(true);
        else
            Debug.LogError("⚠ gameOverUI es NULL");

        //  iniciar fade
        if (fade != null)
            yield return StartCoroutine(fade.StartFade());
        else
            Debug.LogError("⚠ fade es NULL");

        //  pausar tiempo
        Time.timeScale = 0f;

        //  usar tiempo real para esperar
        yield return new WaitForSecondsRealtime(2.5f);

        //  restaurar tiempo y recargar
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
