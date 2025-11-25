using UnityEngine;
using System.Collections.Generic;

public class BatterySpawnerArea : MonoBehaviour
{
    [Header("Prefab")]
    public GameObject batteryPickupPrefab;

    [Header("Área local (centrada aquí)")]
    public Vector3 size = new Vector3(20f, 0f, 20f);
    public float raycastHeight = 10f;
    public LayerMask groundMask;

    [Header("Control")]
    [Tooltip("Máximo de baterías que se pueden colocar en el área")]
    public int maxBatteriesInScene = 6;

    [Tooltip("Máximos intentos por cada batería a colocar")]
    public int maxTriesPerSpawn = 15;

    [Tooltip("Distancia mínima entre baterías")]
    public float minDistanceBetweenBatteries = 0.6f;

    [Header("Opciones")]
    public bool randomYRotation = true;
    public float heightOffset = 0.02f;

    readonly List<GameObject> spawned = new();

    void Start()
    {
        int targetCount = Mathf.Clamp(
            Random.Range(1, maxBatteriesInScene + 1),
            1,
            maxBatteriesInScene
        );

        for (int i = 0; i < targetCount; i++)
        {
            bool success = TrySpawnOne();
            if (!success)
            {
                break;
            }
        }
    }

    bool TrySpawnOne()
    {
        if (!batteryPickupPrefab) return false;

        spawned.RemoveAll(g => g == null);

        if (spawned.Count >= maxBatteriesInScene) return false;

        for (int i = 0; i < maxTriesPerSpawn; i++)
        {
            Vector3 local = new Vector3(
                UnityEngine.Random.Range(-size.x * 0.5f, size.x * 0.5f),
                0f,
                UnityEngine.Random.Range(-size.z * 0.5f, size.z * 0.5f)
            );

            Vector3 worldTop = transform.TransformPoint(local + Vector3.up * raycastHeight);

            if (Physics.Raycast(worldTop, Vector3.down, out RaycastHit hit, raycastHeight * 2f, groundMask, QueryTriggerInteraction.Ignore))
            {
                bool occupied = false;
                foreach (var g in spawned)
                {
                    if (!g) continue;

                    if ((g.transform.position - hit.point).sqrMagnitude <
                        (minDistanceBetweenBatteries * minDistanceBetweenBatteries))
                    {
                        occupied = true;
                        break;
                    }
                }
                if (occupied) continue;

                Quaternion rot = randomYRotation
                    ? Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f)
                    : Quaternion.identity;

                Vector3 pos = hit.point + Vector3.up * heightOffset;

                var go = Object.Instantiate(batteryPickupPrefab, pos, rot);
                spawned.Add(go);

                return true;
            }
        }

        return false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 0.4f, 0.25f);
        Matrix4x4 m = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
        Gizmos.matrix = m;
        Gizmos.DrawCube(Vector3.zero, new Vector3(size.x, 0.05f, size.z));
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(size.x, 0.05f, size.z));
    }
}
