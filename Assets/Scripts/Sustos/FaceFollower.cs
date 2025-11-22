using UnityEngine;

public class FaceFollower : MonoBehaviour
{
    [Header("Objetivo")]
    public Transform targetToLook; // Normalmente la cámara del jugador

    [Header("Ajustes")]
    public bool fixBackwards = false; // MARCA ESTO si la cara se pone negra o invisible
    public bool lockYAxis = false;    // Marca esto si SOLO quieres que rote de lado a lado (como un poste)

    void Start()
    {
        // Si se nos olvidó poner el target, buscamos la cámara automáticamente
        if (targetToLook == null && Camera.main != null)
        {
            targetToLook = Camera.main.transform;
        }
    }

    void Update()
    {
        if (targetToLook == null) return;

        // 1. Mirar al jugador
        // (Guardamos la posición a la que queremos mirar)
        Vector3 targetPostition = targetToLook.position;

        // Si queremos bloquear el eje Y (que no mire arriba/abajo), igualamos la altura
        if (lockYAxis)
        {
            targetPostition.y = transform.position.y;
        }

        // La función mágica de Unity que hace todo el cálculo matemático
        transform.LookAt(targetPostition);

        // 2. Corrección de rotación (Por si el Quad sale al revés)
        if (fixBackwards)
        {
            transform.Rotate(0, 180, 0);
        }
    }
}