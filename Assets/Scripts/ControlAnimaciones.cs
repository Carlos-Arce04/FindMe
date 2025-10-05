using UnityEngine;

public class ControlAnimaciones : MonoBehaviour
{
    private Animator animator;  // Referencia al Animator del prefab

    // Variables para controlar los movimientos
    private bool isWalking = false;

    void Start()
    {
        // Obtener el componente Animator
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // Si presionas la tecla W, activar la animación de caminar
        if (Input.GetKeyDown(KeyCode.W)) // Tecla W para caminar
        {
            isWalking = true;
            animator.SetTrigger("StartWalk");  // Activa el trigger para la animación Creep|Crouch_Action (caminar)
            animator.speed = 1f;  // Reanuda la animación a velocidad normal
        }
        else if (Input.GetKeyUp(KeyCode.W)) // Cuando sueltas la tecla W, volver a la animación de reposo
        {
            isWalking = false;
            animator.SetTrigger("StartIdle");  // Activa el trigger para la animación de reposo (quieto)
            animator.speed = 1f;  // Asegurarse de que la animación vuelva a la velocidad normal
        }
    }

    // Función para controlar la animación
    void ControlAnimation()
    {
        if (!isWalking)
        {
            // Si no estamos caminando, se activa la animación de reposo
            animator.Play("Creep|Idle2_Action");
        }
    }
}
