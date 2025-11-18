using UnityEngine;

public class ClueTrigger : MonoBehaviour
{
    public LevelId level = LevelId.Floor2;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        ProgressManager.CompleteStep(level, StepKind.Clue);
        Destroy(gameObject);
    }
}
