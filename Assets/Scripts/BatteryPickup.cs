// BatteryPickup.cs (Simplificado)
using UnityEngine;

public class BatteryPickup : MonoBehaviour
{
    public int amount = 1;
    public bool destroyOnPickup = true;

    [Header("Sound")]
    public AudioClip pickupSound;
    [Range(0f, 1f)]
    public float pickupVolume = 1f;

    public bool TryPickup(BatteryInventory inv)
    {
        if (inv == null) return false;
        if (!(inv.CanAddBattery() && inv.AddBattery(amount))) return false;

        // La lógica de sonido ya NO está aquí.
        
        if (destroyOnPickup) Destroy(gameObject);
        return true;
    }
}