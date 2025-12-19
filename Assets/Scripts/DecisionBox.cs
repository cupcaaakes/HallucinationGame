using UnityEngine;

public class DecisionBox : MonoBehaviour
{
    [SerializeField] private bool isLeft;
    [SerializeField] private Director director;

    void Awake()
    {
        if (!director) director = FindFirstObjectByType<Director>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        director?.SetChoiceHover(isLeft, true);
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        director?.SetChoiceHover(isLeft, false);
    }
}
