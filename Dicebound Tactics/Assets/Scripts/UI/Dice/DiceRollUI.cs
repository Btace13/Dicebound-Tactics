using System.Collections;
using UnityEngine;
using TMPro;
using System;
using TacticsToolkit;

public class DiceRollUI : MonoBehaviour
{
  public TextMeshProUGUI diceText;
  public float rollDuration = 1.5f;

  private int finalRoll;
  private Action<int> onRollComplete;
  private Entity entity;
  private int maxRollValue = 6;
  
  public void SetupRoll(Entity entity)
  {
      this.entity = entity;
      maxRollValue = entity.equippedDice.sides.Count;
  }

  public void StartRoll(Action<int> callback = null)
  {
    onRollComplete = callback;
    StartCoroutine(RollDiceCoroutine());
  }

  private IEnumerator RollDiceCoroutine()
  {
    float elapsed = 0f;
    float speed = 0.05f;

    while (elapsed < rollDuration)
    {
        int currentRoll = UnityEngine.Random.Range(1, maxRollValue + 1);
        diceText.text = currentRoll.ToString();

        yield return new WaitForSeconds(speed);
        elapsed += speed;
        speed += 0.01f;
    }

    finalRoll = UnityEngine.Random.Range(1, maxRollValue + 1);
    diceText.text = finalRoll.ToString();
    // Debug.Log("Final roll for " + entity.name + ": " + finalRoll);
    onRollComplete?.Invoke(finalRoll);
  }
}
