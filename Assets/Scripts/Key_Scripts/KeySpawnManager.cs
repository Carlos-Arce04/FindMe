using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class KeySpawnManager : MonoBehaviour
{
    [System.Serializable]
    public class KeyPrefab { public KeyType keyType; public GameObject prefab; }
    
    public List<KeyPrefab> keyPrefabs;
    public List<Transform> spawnPoints;
    public int minKeysPerColor = 2;
    
    void Start()
    {
        SpawnKeys();
    }

    void SpawnKeys()
    {
        if (keyPrefabs.Count == 0 || spawnPoints.Count == 0) return;
        
        List<Transform> availablePoints = new List<Transform>(spawnPoints).OrderBy(x => Random.value).ToList();
        int pointIndex = 0;

        foreach (var keyInfo in keyPrefabs)
        {
            for (int i = 0; i < minKeysPerColor; i++)
            {
                if (pointIndex >= availablePoints.Count) return;
                
                Transform spawnPoint = availablePoints[pointIndex];
                Instantiate(keyInfo.prefab, spawnPoint.position, keyInfo.prefab.transform.rotation); // Usa la rotación del prefab
                pointIndex++;
            }
        }
    }
}