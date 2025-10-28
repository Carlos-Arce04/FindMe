using UnityEngine;

public class BatteryPickup : MonoBehaviour
{
    public int amount = 1;
    public bool destroyOnPickup = true;

    [Header("Audio")]
    public AudioClip pickupSound;
    [Range(0f, 1f)] public float pickupVolume = 1f;
    [Range(0f, 1f)] public float spatialBlend3D = 1f;

    [Header("Gameplay Sound")]
    public float hearingRange = 12f;
    public bool scaleRangeByVolume = true;

    public bool TryPickup(BatteryInventory inv)
    {
        if (inv == null) return false;
        if (!(inv.CanAddBattery() && inv.AddBattery(amount))) return false;

        if (pickupSound && pickupVolume > 0f)
        {
            var go = new GameObject("OneShot_BatteryPickup");
            go.transform.position = transform.position;
            var src = go.AddComponent<AudioSource>();
            src.clip = pickupSound;
            src.volume = pickupVolume;
            src.spatialBlend = spatialBlend3D; // 1 = 3D
            src.minDistance = 1f;
            src.maxDistance = Mathf.Max(5f, hearingRange);
            src.Play();
            Destroy(go, pickupSound.length + 0.05f);
        }

        if (SoundManager.Instance != null)
        {
            float range = scaleRangeByVolume ? hearingRange * Mathf.Clamp01(pickupVolume) : hearingRange;
            SoundManager.Instance.ReportSound(transform.position, range);
        }

        if (destroyOnPickup) Destroy(gameObject);
        return true;
    }
}
