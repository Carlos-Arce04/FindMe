using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryKeyUIManager : MonoBehaviour
{
    public TextMeshProUGUI fullInventoryText;
    public float messageDisplayTime = 2f;
    public List<Image> keySlots;

    [System.Serializable]
    public class KeySprite { public KeyType keyType; public Sprite icon; }
    public List<KeySprite> keySprites;

    void Start()
    {
        if (fullInventoryText != null) fullInventoryText.gameObject.SetActive(false);
        UpdateKeyDisplay(new List<KeyItem>());
    }

    public void UpdateKeyDisplay(List<KeyItem> playerKeys)
    {
        for (int i = 0; i < keySlots.Count; i++)
        {
            if (i < playerKeys.Count)
            {
                KeyItem key = playerKeys[i];
                Sprite keyIcon = GetSpriteForKey(key.keyType);
                keySlots[i].sprite = keyIcon;
                keySlots[i].color = Color.white;
            }
            else
            {
                keySlots[i].sprite = null;
                keySlots[i].color = new Color(1, 1, 1, 0);
            }
        }
    }

    private Sprite GetSpriteForKey(KeyType type)
    {
        foreach (var ks in keySprites) { if (ks.keyType == type) return ks.icon; }
        return null;
    }

    public void ShowFullInventoryMessage()
    {
        StartCoroutine(ShowMessageCoroutine());
    }

    private IEnumerator ShowMessageCoroutine()
    {
        if (fullInventoryText != null)
        {
            fullInventoryText.gameObject.SetActive(true);
            yield return new WaitForSeconds(messageDisplayTime);
            fullInventoryText.gameObject.SetActive(false);
        }
    }
}