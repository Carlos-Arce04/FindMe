using UnityEngine;
using TMPro;

public class PickupPromptUI : MonoBehaviour
{
    public static PickupPromptUI Instance;

    [Header("Refs")]
    public TMP_Text message;   

    bool locked = false;
    float unlockAt = 0f;
    string restoreText = null;

    void Awake()
    {
        Instance = this;
        if (message) message.gameObject.SetActive(false);
    }

    void Update()
    {
        if (locked && Time.time >= unlockAt)
        {
            locked = false;
            if (!message) return;

            if (string.IsNullOrEmpty(restoreText))
            {
                message.gameObject.SetActive(false);
            }
            else
            {
                message.text = restoreText;
                message.gameObject.SetActive(true);
            }
            restoreText = null;
        }
    }

    public void Show(string msg)
    {
        if (locked || !message) return;
        message.text = msg;
        message.gameObject.SetActive(true);
    }

    public void Hide()
    {
        if (locked || !message) return;
        message.gameObject.SetActive(false);
    }

    public void ShowSticky(string msg, float seconds, string restoreTo = null)
    {
        if (!message) return;
        locked = true;
        unlockAt = Time.time + Mathf.Max(0.01f, seconds);
        restoreText = restoreTo;
        message.text = msg;
        message.gameObject.SetActive(true);
    }

    public bool IsLocked => locked;
}
