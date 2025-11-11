using UnityEngine;

public class MonsterAttack : MonoBehaviour
{
    [Header("Daño por contacto")]
    public float damagePerHit = 15f;
    public float attackCooldown = 0.8f; // segundos entre golpes

    private float nextAllowedTime;

    void OnTriggerStay(Collider other)
    {
        // Solo reaccionar si el objeto que entra tiene la etiqueta "Player"
        if (!other.CompareTag("Player")) return;

        // Controla el tiempo entre ataques
        if (Time.time < nextAllowedTime) return;

        // Obtener el componente de salud del jugador
        PlayerHealth hp = other.GetComponent<PlayerHealth>();
        if (hp != null)
        {
            hp.TakeDamage(damagePerHit);
            nextAllowedTime = Time.time + attackCooldown;

           
        }
    }
}
