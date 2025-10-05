using UnityEngine;

public class BatteryPickup : MonoBehaviour
{
    public int amount = 1;
    public AudioSource pickupSfx;
    public bool destroyOnPickup = true;

    public bool TryPickup(BatteryInventory inv)
    {
        if (inv == null) return false;
        if (!(inv.CanAddBattery() && inv.AddBattery(amount))) return false;

        if (pickupSfx)
        {
            pickupSfx.Play();
            pickupSfx.transform.SetParent(null);
            Destroy(pickupSfx.gameObject, pickupSfx.clip.length);
        }
        if (destroyOnPickup) Destroy(gameObject);
        return true;
    }
}
