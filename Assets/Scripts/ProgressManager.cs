using UnityEngine;
using System.Collections.Generic;

public enum LevelId { Floor2 = 2, Floor3 = 3, Basement = 4 }
public enum StepKind { Key, Door, Clue }

public class ProgressManager : MonoBehaviour
{
    public static ProgressManager Instance;

    [Header("Setup")]
    public LevelId currentLevel = LevelId.Floor2;
    public int stepsPerLevel = 3;   // Key, Door, Clue
    public ToastUI toast;           // arrastra tu ToastUI aquí (en GameSystems)

    [Header("Narrativa (opcional)")]
    [TextArea] public string l2_keyText = "La llave está tibia…";
    [TextArea] public string l2_doorText = "La bisagra cede. Olor a polvo.";
    [TextArea] public string l2_clueText = "‘Sube. No hagas ruido.’";
    [TextArea] public string l3_keyText = "Llave rayada. Prisa.";
    [TextArea] public string l3_doorText = "Puerta vibró. Bloqueada desde dentro.";
    [TextArea] public string l3_clueText = "‘Baja. Él no mira abajo.’";
    [TextArea] public string l4_keyText = "Metal frío. Un zumbido lejano.";
    [TextArea] public string l4_doorText = "El candado cae. El aire muerde.";
    [TextArea] public string endingText = "Lo encuentras. ‘Te oí llegar…’ FIN";

    // Progreso interno: combina nivel+tipo para evitar duplicados
    private HashSet<string> _completed = new HashSet<string>();
    private int _stepsDoneThisLevel = 0;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        // Opcional: DontDestroyOnLoad(gameObject);
    }

    public static void CompleteStep(LevelId level, StepKind step)
    {
        if (Instance == null) { Debug.LogWarning("[Progress] No Instance"); return; }
        Instance.InternalCompleteStep(level, step);
    }

    void InternalCompleteStep(LevelId level, StepKind step)
    {
        // Solo cuenta si corresponde al nivel actual
        if (level != currentLevel) return;

        string key = $"{(int)level}:{step}";
        if (_completed.Contains(key)) return; // ya contado

        _completed.Add(key);
        _stepsDoneThisLevel++;

        // Mensaje corto contextual
        ShowStepToast(level, step);

        Debug.Log($"[Progress] Step {step} done in L{(int)level} ({_stepsDoneThisLevel}/{stepsPerLevel})");

        if (_stepsDoneThisLevel >= stepsPerLevel)
        {
            // Nivel completado → desbloquear siguiente
            UnlockNextLevel();
        }
    }

    void ShowStepToast(LevelId level, StepKind step)
    {
        if (toast == null) return;

        string msg = null;
        switch (level)
        {
            case LevelId.Floor2:
                if      (step == StepKind.Key)  msg = l2_keyText;
                else if (step == StepKind.Door) msg = l2_doorText;
                else if (step == StepKind.Clue) msg = l2_clueText;
                break;
            case LevelId.Floor3:
                if      (step == StepKind.Key)  msg = l3_keyText;
                else if (step == StepKind.Door) msg = l3_doorText;
                else if (step == StepKind.Clue) msg = l3_clueText;
                break;
            case LevelId.Basement:
                if      (step == StepKind.Key)  msg = l4_keyText;
                else if (step == StepKind.Door) msg = l4_doorText;
                else if (step == StepKind.Clue) msg = endingText; // al encontrar al hermano
                break;
        }
        if (!string.IsNullOrWhiteSpace(msg)) toast.Show(msg, 3f);
    }

    void UnlockNextLevel()
    {
        if (currentLevel == LevelId.Basement)
        {
            // Ending ya mostrado en Clue, podrías cargar escena menú, etc.
            Debug.Log("[Progress] Game End");
            return;
        }

        // Desbloquear gates cuyo requiredLevelCompleted == currentLevel
        var gates = FindObjectsOfType<FloorGate>(true);
        foreach (var g in gates)
        {
            if ((LevelId)g.requiredLevelCompleted == currentLevel)
                g.Unlock();
        }

        // Toast de piso desbloqueado
        if (toast != null)
        {
            if (currentLevel == LevelId.Floor2) toast.Show("Tercer piso desbloqueado", 3f);
            if (currentLevel == LevelId.Floor3) toast.Show("Sótano desbloqueado", 3f);
        }

        // Preparar siguiente nivel
        currentLevel = NextLevel(currentLevel);
        _stepsDoneThisLevel = 0; // reset por nivel
        Debug.Log($"[Progress] Next Level: L{(int)currentLevel}");
    }

    LevelId NextLevel(LevelId lvl)
    {
        if (lvl == LevelId.Floor2) return LevelId.Floor3;
        if (lvl == LevelId.Floor3) return LevelId.Basement;
        return LevelId.Basement;
    }
}
