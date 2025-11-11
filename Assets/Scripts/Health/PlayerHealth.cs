using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

public class PlayerHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth;
    public float iFramesAfterHit = 0.35f; // tiempo mínimo entre golpes
    public UnityEvent<float, float> onHealthChanged; // evento para futura barra de vida

    float lastHitTime = -999f;
    bool dead;

    void Awake()
    {
        currentHealth = maxHealth;
        onHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void TakeDamage(float amount)
    {
        if (dead) return;
        if (Time.time < lastHitTime + iFramesAfterHit) return; // evita daño repetido

        lastHitTime = Time.time;
        currentHealth = Mathf.Max(0f, currentHealth - amount);

        // efecto visual de daño
        HitFlash flash = FindObjectOfType<HitFlash>();
        if (flash != null) flash.Flash();

        onHealthChanged?.Invoke(currentHealth, maxHealth);

        // si la vida llega a cero, reinicia la escena
        if (currentHealth <= 0f)
        {
            dead = true;
            Scene scene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(scene.buildIndex);
        }
    }
}
