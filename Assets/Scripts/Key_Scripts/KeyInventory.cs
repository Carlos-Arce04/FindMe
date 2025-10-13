using System.Collections.Generic;
using UnityEngine;

public class KeyInventory : MonoBehaviour
{
    public int maxKeys = 3; // Límite de 3 llaves
    public List<KeyItem> keys = new List<KeyItem>();
    public InventoryKeyUIManager uiManager; // Referencia a la UI

    void Start()
    {
        if (uiManager != null) uiManager.UpdateKeyDisplay(keys);
    }

    public bool AddKey(KeyItem keyToAdd)
    {
        if (keys.Count >= maxKeys)
        {
            Debug.Log("Inventario de llaves lleno.");
            if (uiManager != null) uiManager.ShowFullInventoryMessage();
            return false;
        }
        keys.Add(keyToAdd);
        Debug.Log("Llave " + keyToAdd.keyType + " recogida. Total: " + keys.Count);
        if (uiManager != null) uiManager.UpdateKeyDisplay(keys);
        return true;
    }
}