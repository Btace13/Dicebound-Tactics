using UnityEngine;
using NUnit.Framework;

/// <summary>
/// Unit tests for CombatUIManager state machine
/// </summary>
public class CombatUIManagerTests
{
    private GameObject testGO;
    private CombatUIManager uiManager;

    [SetUp]
    public void Setup()
    {
        testGO = new GameObject("TestCombatUIManager");
        uiManager = testGO.AddComponent<CombatUIManager>();
    }

    [TearDown]
    public void Teardown()
    {
        if (testGO != null)
        {
            Object.DestroyImmediate(testGO);
        }
    }

    [Test]
    public void InitialState_ShouldBeHidden()
    {
        Assert.AreEqual(CombatUIManager.UIState.Hidden, uiManager.GetCurrentState());
    }

    [Test]
    public void ValidTransition_ShouldSucceed()
    {
        // Test valid transition from Hidden to BattleStart
        bool result = uiManager.TryTransition(CombatUIManager.UITransition.ShowCombatUI);
        
        Assert.IsTrue(result);
        Assert.AreEqual(CombatUIManager.UIState.BattleStart, uiManager.GetCurrentState());
    }

    [Test]
    public void InvalidTransition_ShouldFail()
    {
        // Test invalid transition from Hidden to PlayerTurn (should go through BattleStart first)
        bool result = uiManager.TryTransition(CombatUIManager.UITransition.StartPlayerTurn);
        
        Assert.IsFalse(result);
        Assert.AreEqual(CombatUIManager.UIState.Hidden, uiManager.GetCurrentState());
    }

    [Test]
    public void StateTransitionChain_ShouldWork()
    {
        // Test a complete state transition chain
        Assert.IsTrue(uiManager.TryTransition(CombatUIManager.UITransition.ShowCombatUI));
        Assert.AreEqual(CombatUIManager.UIState.BattleStart, uiManager.GetCurrentState());

        Assert.IsTrue(uiManager.TryTransition(CombatUIManager.UITransition.StartPlayerTurn));
        Assert.AreEqual(CombatUIManager.UIState.PlayerTurn, uiManager.GetCurrentState());

        Assert.IsTrue(uiManager.TryTransition(CombatUIManager.UITransition.OpenAbilityPanel));
        Assert.AreEqual(CombatUIManager.UIState.AbilitySelection, uiManager.GetCurrentState());

        Assert.IsTrue(uiManager.TryTransition(CombatUIManager.UITransition.GoBack));
        Assert.AreEqual(CombatUIManager.UIState.PlayerTurn, uiManager.GetCurrentState());
    }

    [Test]
    public void TargetSelectionFlow_ShouldWork()
    {
        // Setup to PlayerTurn state
        uiManager.TryTransition(CombatUIManager.UITransition.ShowCombatUI);
        uiManager.TryTransition(CombatUIManager.UITransition.StartPlayerTurn);

        // Test target selection flow
        Assert.IsTrue(uiManager.TryTransition(CombatUIManager.UITransition.StartTargetSelection));
        Assert.AreEqual(CombatUIManager.UIState.TargetSelection, uiManager.GetCurrentState());

        Assert.IsTrue(uiManager.TryTransition(CombatUIManager.UITransition.EndTargetSelection));
        Assert.AreEqual(CombatUIManager.UIState.PlayerTurn, uiManager.GetCurrentState());
    }

    [Test]
    public void EnemyTurnFlow_ShouldWork()
    {
        // Setup to PlayerTurn state
        uiManager.TryTransition(CombatUIManager.UITransition.ShowCombatUI);
        uiManager.TryTransition(CombatUIManager.UITransition.StartPlayerTurn);

        // Test enemy turn flow
        Assert.IsTrue(uiManager.TryTransition(CombatUIManager.UITransition.StartEnemyTurn));
        Assert.AreEqual(CombatUIManager.UIState.EnemyTurn, uiManager.GetCurrentState());

        Assert.IsTrue(uiManager.TryTransition(CombatUIManager.UITransition.StartPlayerTurn));
        Assert.AreEqual(CombatUIManager.UIState.PlayerTurn, uiManager.GetCurrentState());
    }

    [Test]
    public void BattleEndFlow_ShouldWork()
    {
        // Setup to PlayerTurn state
        uiManager.TryTransition(CombatUIManager.UITransition.ShowCombatUI);
        uiManager.TryTransition(CombatUIManager.UITransition.StartPlayerTurn);

        // Test battle end flow
        Assert.IsTrue(uiManager.TryTransition(CombatUIManager.UITransition.EndBattle));
        Assert.AreEqual(CombatUIManager.UIState.BattleEnd, uiManager.GetCurrentState());

        Assert.IsTrue(uiManager.TryTransition(CombatUIManager.UITransition.HideCombatUI));
        Assert.AreEqual(CombatUIManager.UIState.Hidden, uiManager.GetCurrentState());
    }
}
