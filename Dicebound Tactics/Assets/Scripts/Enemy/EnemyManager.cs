using System.Collections;
using UnityEngine;
using System.Linq;
using TacticsToolkit;

public class EnemyManager : Entity
{
    private TurnManager turnManager;

    private void Start()
    {
        turnManager = FindFirstObjectByType<TurnManager>();
    }

    public void BeginAITurn()
    {
        if (!isAlive || CurrentAP <= 0)
        {
            EndAITurn();
            return;
        }

        StartCoroutine(ExecuteAITurn());
    }

    private IEnumerator ExecuteAITurn()
    {
        Debug.Log($"[AI] {name} begins turn with {CurrentAP} AP");

        while (CurrentAP > 0)
        {
            var usableAbilities = abilities
                .Where(a => a.apCost <= CurrentAP)
                .ToList();

            if (usableAbilities.Count == 0)
                break;

            var ability = usableAbilities[Random.Range(0, usableAbilities.Count)];

            var targets = turnManager.playerUnits
                .Where(p => p != null && p.isAlive)
                .ToList();

            if (targets.Count == 0)
                break;

            var target = targets[Random.Range(0, targets.Count)];

            Debug.Log($"[AI] {name} uses {ability.abilityName} on {target.name}");

            ability.Execute(this, target);
            statsContainer.ActionPoints.statValue -= ability.apCost;

            yield return new WaitForSeconds(0.5f);
        }

        Debug.Log($"[AI] {name} ends turn with {CurrentAP} AP");
        EndAITurn();
    }

    private void EndAITurn()
    {
        turnManager.EndTurn();
    }
}
