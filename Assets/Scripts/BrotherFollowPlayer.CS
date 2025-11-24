using UnityEngine;

public class BrotherFollowPlayer : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float moveSpeed = 1.5f;
    [SerializeField] private float stopDistance = 1.5f;

    private Transform target;
    private bool isFollowing = false;

    [Header("Animación")]
    private Animator animator;
    // Cambia "IsWalking" si tu parámetro en el Animator tiene otro nombre
    private static readonly int IsWalkingHash = Animator.StringToHash("IsWalking");

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void StartFollowing(Transform followTarget)
    {
        if (followTarget == null)
        {
            Debug.LogWarning("BrotherFollowPlayer.StartFollowing: followTarget es null");
            return;
        }

        target = followTarget;
        isFollowing = true;
        Debug.Log("BrotherFollowPlayer.StartFollowing → target = " + target.name);
    }

    public void StopFollowing()
    {
        isFollowing = false;
        UpdateAnimator(false);
    }

    private void Update()
    {
        if (!isFollowing || target == null)
        {
            UpdateAnimator(false);
            return;
        }

        // Posiciones en plano XZ (no sube/baja en Y)
        Vector3 currentPos = transform.position;
        Vector3 targetPos = target.position;
        Vector3 flatTarget = new Vector3(targetPos.x, currentPos.y, targetPos.z);

        float distance = Vector3.Distance(currentPos, flatTarget);

        // ¿Debe moverse?
        bool shouldMove = distance > stopDistance;
        UpdateAnimator(shouldMove);

        if (!shouldMove) return;

        // Dirección hacia el jugador
        Vector3 dir = (flatTarget - currentPos).normalized;
        transform.position += dir * moveSpeed * Time.deltaTime;

        // Girar hacia el jugador
        if (dir.sqrMagnitude > 0.0001f)
        {
            Quaternion lookRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, 10f * Time.deltaTime);
        }
    }

    private void UpdateAnimator(bool isWalking)
    {
        if (animator != null)
        {
            animator.SetBool(IsWalkingHash, isWalking);
        }
    }
}
