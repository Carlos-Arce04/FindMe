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
    public int maxBatteriesInScene = 6;
    public float spawnInterval = 15f;
    public int maxTriesPerSpawn = 15;
    public float minDistanceBetweenBatteries = 0.6f;

    [Header("Opciones")]
    public bool randomYRotation = true;
    public float heightOffset = 0.02f;

    float timer;
    readonly List<GameObject> spawned = new();

    void Update()
    {
        spawned.RemoveAll(g => g == null);

        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer = 0f;
            TrySpawn();
        }
    }

    void TrySpawn()
    {
        if (!batteryPickupPrefab) return;
        if (spawned.Count >= maxBatteriesInScene) return;

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
                    if (g && (g.transform.position - hit.point).sqrMagnitude < (minDistanceBetweenBatteries * minDistanceBetweenBatteries))
                    { occupied = true; break; }
                }
                if (occupied) continue;

                Quaternion rot = randomYRotation ? Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f)
                                                 : Quaternion.identity;

                Vector3 pos = hit.point + Vector3.up * heightOffset;

                var go = Instantiate(batteryPickupPrefab, pos, rot);
                spawned.Add(go);
                break;
            }
        }
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
