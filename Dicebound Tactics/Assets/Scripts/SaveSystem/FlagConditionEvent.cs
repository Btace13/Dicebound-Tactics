using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;

public class FlagConditionEvent : MonoBehaviour
{
    public FlagConditionSet conditions;

    public UnityEvent onConditionsMet;
    public UnityEvent onConditionsNotMet;

    private void Start()
    {
        if (conditions.AreConditionsMet())
            onConditionsMet.Invoke();
        else
            onConditionsNotMet.Invoke();
    }
}
