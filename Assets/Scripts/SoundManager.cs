using UnityEngine;
using System.Collections.Generic;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }
    private List<MonsterHearing> allMonstersListening = new List<MonsterHearing>();

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    public void RegisterMonster(MonsterHearing monster)
    {
        if (!allMonstersListening.Contains(monster)) allMonstersListening.Add(monster);
    }

    public void ReportSound(Vector3 soundPosition, float range)
    {
        foreach (var monster in allMonstersListening)
        {
            monster.ProcessSound(soundPosition, range);
        }
    }
}