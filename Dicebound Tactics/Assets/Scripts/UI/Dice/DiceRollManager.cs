using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using TacticsToolkit;

public class DiceRollManager : MonoBehaviour
{
    public GameObject diceUIPrefab; // Assign DiceUI prefab

    private List<DiceRollUI> activeDice = new List<DiceRollUI>();

    public void RollDiceForUnits(List<Entity> units, Action onComplete)
    {
        Debug.Log("Rolling dice for " + units.Count + " units.");
        StartCoroutine(RollRoutine(units, onComplete));
    }

    private IEnumerator RollRoutine(List<Entity> units, Action onComplete)
    {
        activeDice.Clear();
        int finished = 0;

        foreach (Entity unit in units)
        {
            GameObject diceGO = Instantiate(diceUIPrefab, unit.transform.position + Vector3.up * 1.4f, Quaternion.identity);
            DiceRollUI dice = diceGO.GetComponent<DiceRollUI>();
            dice.SetupRoll(unit);
            activeDice.Add(dice);

            Debug.Log("Starting roll for " + unit.name);
            dice.StartRoll((result) =>
            {
              unit.ApplyDiceRoll(result);
              finished++;
              Destroy(diceGO, 2f);
            });
        }

        // Wait for all dice to finish
        while (finished < units.Count)
            yield return null;

        onComplete?.Invoke();
    }
}
