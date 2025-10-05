using UnityEngine;

public class BatteryInventory : MonoBehaviour
{
    [Header("Inventario")]
    public int maxBatteries = 3;       
    public int currentBatteries = 0;   

    [Header("Consumo")]
    public KeyCode useKey = KeyCode.R; 
    public float rechargeAmount = 50f;

    [Header("Refs")]
    public FlashlightToggleAndBattery flashlight; 
    public AudioSource useBatterySfx;             

    void Update()
    {
        if (Input.GetKeyDown(useKey))
        {
            TryUseBattery();
        }
    }

    public bool CanAddBattery() => currentBatteries < maxBatteries;

    public bool AddBattery(int amount = 1)
    {
        if (amount <= 0) return false;
        if (currentBatteries >= maxBatteries) return false;

        currentBatteries = Mathf.Clamp(currentBatteries + amount, 0, maxBatteries);
        return true;
    }

    public bool TryUseBattery()
    {
        if (currentBatteries <= 0) return false;        
        if (flashlight == null) return false;         
        if (flashlight.IsFull) return false;         

        currentBatteries--;                            
        flashlight.AddCharge(rechargeAmount);          
        if (useBatterySfx != null) useBatterySfx.Play();
        return true;
    }
}
