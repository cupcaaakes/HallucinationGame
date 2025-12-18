using UnityEngine;

public class DecisionTrigger : MonoBehaviour
{
    public enum DecisionType
    {
        Left,
        Right
    }

    public DecisionType decision;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (decision == DecisionType.Right)
        {
            Debug.Log("DECISION 1: Character went RIGHT");
        }
        else if (decision == DecisionType.Left)
        {
            Debug.Log("DECISION 2: Character went LEFT");
        }
    }
}
