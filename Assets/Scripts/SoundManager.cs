using UnityEngine;
using System.Collections.Generic;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }
    private readonly List<MonsterHearing> allMonstersListening = new List<MonsterHearing>();

    [Header("Debug")]
    public bool drawGizmos = true;
    public Color gizmoColor = new Color(1f, 0.9f, 0.2f, 0.25f);
    private readonly Queue<(Vector3 pos, float range, float t)> gizmoBursts = new();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void RegisterMonster(MonsterHearing monster)
    {
        if (monster != null && !allMonstersListening.Contains(monster))
            allMonstersListening.Add(monster);
    }

    public void UnregisterMonster(MonsterHearing monster)
    {
        if (monster != null) allMonstersListening.Remove(monster);
    }

    public void ReportSound(Vector3 soundPosition, float range)
    {
        if (drawGizmos) gizmoBursts.Enqueue((soundPosition, range, Time.time));

        for (int i = allMonstersListening.Count - 1; i >= 0; i--)
        {
            var m = allMonstersListening[i];
            if (m == null) { allMonstersListening.RemoveAt(i); continue; }
            m.ProcessSound(soundPosition, range);
        }
    }

    void OnDrawGizmos()
    {
        if (!drawGizmos || gizmoBursts.Count == 0) return;

        const float life = 0.8f;
        int c = gizmoBursts.Count;
        for (int i = 0; i < c; i++)
        {
            var (pos, range, t) = gizmoBursts.Dequeue();
            if (Application.isPlaying && Time.time - t > life) continue;
            Gizmos.color = gizmoColor;
            Gizmos.DrawSphere(pos, 0.05f);
            Gizmos.DrawWireSphere(pos, range);
            gizmoBursts.Enqueue((pos, range, t));
        }
    }
}
