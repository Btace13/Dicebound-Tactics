using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TacticsToolkit
{
    public class TurnBasedController : MonoBehaviour
    {
        private List<Entity> teamA = new List<Entity>();
        private List<Entity> teamB = new List<Entity>();

        public TurnSorting turnSorting;

        public GameEventGameObject startNewCharacterTurn;
        public GameEventGameObjectList turnOrderSet;
        public GameEventGameObjectList turnPreviewSet;

        public List<TurnOrderPreviewObject> turnOrderPreview;
        public List<TurnOrderPreviewObject> currentTurnOrderPreview;

        public bool ignorePlayers = false;
        public bool ignoreEnemies = false;

        public int previewPoolCount = 10;
        private Entity activeCharacter;

        public enum TurnSorting
        {
            ConstantAttribute,
            CTB,
            SideBased
        };

        private enum TurnPhase { PlayerTeam, EnemyTeam }
        private TurnPhase currentTurnPhase = TurnPhase.PlayerTeam;
        private int turnIndex = 0;

        void Start()
        {
            if (!ignorePlayers)
                teamA = GameObject.FindGameObjectsWithTag("Player").Select(x => x.GetComponent<Entity>()).ToList();

            if (!ignoreEnemies)
                teamB = GameObject.FindGameObjectsWithTag("Enemy").Select(x => x.GetComponent<Entity>()).ToList();

            turnOrderPreview = new List<TurnOrderPreviewObject>();

            foreach (var item in teamA)
                item.teamID = 1;

            foreach (var item in teamB)
                item.teamID = 2;

            if (teamA.Count > 0 || teamB.Count > 0)
                SortTeamOrder(true);
        }

        private List<Entity> GetCurrentTeam()
        {
            return currentTurnPhase == TurnPhase.PlayerTeam ? teamA.Where(x => x.isAlive).ToList() : teamB.Where(x => x.isAlive).ToList();
        }

        private void SortTeamOrder(bool updateListSize = false)
        {
            var combinedList = new List<Entity>();

            switch (turnSorting)
            {
                case TurnSorting.ConstantAttribute:
                    if (updateListSize)
                    {
                        if (teamA.Count > 0 || teamB.Count > 0)
                        {
                            combinedList.AddRange(teamA.Where(x => x.isAlive));
                            combinedList.AddRange(teamB.Where(x => x.isAlive));
                            turnOrderPreview = combinedList.OrderBy(x => x.statsContainer.Speed.statValue)
                                                           .Select(x => new TurnOrderPreviewObject(x, x.initiativeValue))
                                                           .ToList();
                            activeCharacter = turnOrderPreview[0].character;

                            int characterCount = 0;
                            while (turnOrderPreview.Count < previewPoolCount)
                            {
                                foreach (var item in combinedList)
                                {
                                    turnOrderPreview.Add(new TurnOrderPreviewObject(item, item.initiativeValue * characterCount));
                                }
                                characterCount++;
                            }
                        }
                    }
                    else
                    {
                        TurnOrderPreviewObject item = turnOrderPreview[0];
                        turnOrderPreview.RemoveAt(0);
                        turnOrderPreview.Add(item);
                        activeCharacter = turnOrderPreview[0].character;
                    }
                    break;

                case TurnSorting.CTB:
                    if (teamA.Count > 0 || teamB.Count > 0)
                    {
                        combinedList.AddRange(teamA.Where(x => x.isAlive));
                        combinedList.AddRange(teamB.Where(x => x.isAlive));
                        combinedList = combinedList.OrderBy(x => x.initiativeValue).ToList();

                        turnOrderPreview = combinedList.Select(x =>
                            new TurnOrderPreviewObject(x, x.initiativeValue + (Constants.BaseCost / x.GetStat(Stats.Speed).statValue))
                        ).ToList();

                        int characterCount = 2;
                        while (turnOrderPreview.Count < previewPoolCount)
                        {
                            foreach (var item in combinedList)
                            {
                                turnOrderPreview.Add(new TurnOrderPreviewObject(item,
                                    item.initiativeValue + ((Constants.BaseCost / item.GetStat(Stats.Speed).statValue) * characterCount)));
                            }
                            characterCount++;
                        }

                        turnOrderPreview = turnOrderPreview.OrderBy(x => x.PreviewInitiativeValue).ToList();
                        activeCharacter = turnOrderPreview[0].character;
                    }
                    break;

                case TurnSorting.SideBased:
                    // Side-based mode doesn't require initiative-based sorting
                    turnOrderPreview.Clear();
                    break;
            }

            currentTurnOrderPreview = turnOrderPreview;
            turnOrderSet.Raise(turnOrderPreview.Select(x => x.character.gameObject).ToList());
        }

        public void StartLevel()
        {
            if (!HasAliveCharacters()) return;

            if (turnSorting == TurnSorting.SideBased)
            {
                currentTurnPhase = TurnPhase.PlayerTeam;
                turnIndex = 0;
                StartNextTurn();
            }
            else
            {
                SortTeamOrder(true);
                activeCharacter.StartTurn();
                startNewCharacterTurn.Raise(activeCharacter.gameObject);
            }
        }

        public void EndTurn()
        {
            if (turnSorting == TurnSorting.SideBased)
            {
                FinaliseEndCharactersTurn();

                var currentTeam = GetCurrentTeam();
                turnIndex++;

                if (turnIndex >= currentTeam.Count)
                {
                    // Switch sides
                    currentTurnPhase = currentTurnPhase == TurnPhase.PlayerTeam ? TurnPhase.EnemyTeam : TurnPhase.PlayerTeam;
                    turnIndex = 0;

                    if (!HasAliveCharacters()) return;
                }

                StartNextTurn();
            }
            else
            {
                if (turnOrderPreview.Count > 0)
                {
                    FinaliseEndCharactersTurn();
                    SortTeamOrder();

                    foreach (var entity in turnOrderPreview)
                        entity.character.isActive = false;

                    if (HasAliveCharacters())
                    {
                        if (activeCharacter.isAlive)
                        {
                            activeCharacter.isActive = true;
                            activeCharacter.ApplyEffects();

                            if (activeCharacter.isAlive)
                            {
                                activeCharacter.StartTurn();
                                startNewCharacterTurn.Raise(activeCharacter.gameObject);
                            }
                            else EndTurn();

                            foreach (var ability in activeCharacter.abilitiesForUse)
                                ability.turnsSinceUsed++;
                        }
                        else EndTurn();
                    }
                }
            }
        }

        private void StartNextTurn()
        {
            var team = GetCurrentTeam();
            if (team.Count == 0)
            {
                // Skip to other team
                currentTurnPhase = currentTurnPhase == TurnPhase.PlayerTeam ? TurnPhase.EnemyTeam : TurnPhase.PlayerTeam;
                turnIndex = 0;
                StartNextTurn();
                return;
            }

            activeCharacter = team[turnIndex];
            activeCharacter.isActive = true;
            activeCharacter.ApplyEffects();

            if (activeCharacter.isAlive)
            {
                activeCharacter.StartTurn();
                startNewCharacterTurn.Raise(activeCharacter.gameObject);
            }
            else
            {
                EndTurn(); // Dead unit, skip
            }
        }

        private bool HasAliveCharacters()
        {
            return (teamA.Any(x => x.isAlive) || teamB.Any(x => x.isAlive));
        }

        private void FinaliseEndCharactersTurn()
        {
            if (activeCharacter.activeTile && activeCharacter.activeTile.tileData)
            {
                var tileEffect = activeCharacter.activeTile.tileData.effect;
                if (tileEffect != null)
                    activeCharacter.AttachEffect(tileEffect);
            }

            activeCharacter.UpdateInitiative(Constants.BaseCost);
        }

        IEnumerator DelayedSetActiveCharacter(Entity firstCharacter)
        {
            yield return new WaitForFixedUpdate();
            startNewCharacterTurn.Raise(firstCharacter.gameObject);
        }

        public void SpawnNewCharacter(GameObject character)
        {
            teamA.Add(character.GetComponent<Entity>());
            SortTeamOrder(true);
        }

        public void UpdatePreviewForAction(int actionCost)
        {
            var updatedTurnOrderPreview = turnOrderPreview.Select(x => new TurnOrderPreviewObject(x.character, x.PreviewInitiativeValue)).ToList();
            var activeCharacters = updatedTurnOrderPreview.Where(x => x.character.name == activeCharacter.name).ToList();

            for (int i = 1; i < activeCharacters.Count; i++)
            {
                activeCharacters[i].PreviewInitiativeValue += Mathf.RoundToInt(actionCost / activeCharacters[i].character.GetStat(Stats.Speed).statValue);
            }

            var updatedOrder = updatedTurnOrderPreview.OrderBy(x => x.PreviewInitiativeValue).ToList();
            currentTurnOrderPreview = updatedOrder;
            turnPreviewSet.Raise(updatedOrder.Select(x => x.character.gameObject).ToList());
        }

        public void UndoPreview()
        {
            turnOrderSet.Raise(turnOrderPreview.Select(x => x.character.gameObject).ToList());
        }

        public void ActionCompleted()
        {
            turnOrderPreview = currentTurnOrderPreview;
            turnOrderSet.Raise(currentTurnOrderPreview.Select(x => x.character.gameObject).ToList());
        }

        public void SwitchCharacter()
        {
            if (turnSorting != TurnSorting.SideBased) return;

            var team = GetCurrentTeam();
            if (team.Count <= 1) return; // Nothing to switch to

            // Deactivate current character
            if (activeCharacter != null)
                activeCharacter.isActive = false;

            // Advance index
            turnIndex = (turnIndex + 1) % team.Count;

            // Set new character
            activeCharacter = team[turnIndex];
            activeCharacter.isActive = true;

            // Start new character's control phase
            activeCharacter.StartTurn();
            startNewCharacterTurn.Raise(activeCharacter.gameObject);
        }

    }

    public class TurnOrderPreviewObject
    {
        public Entity character;
        public int PreviewInitiativeValue;

        public TurnOrderPreviewObject(Entity character, int previewInitiativeValue)
        {
            this.character = character;
            PreviewInitiativeValue = previewInitiativeValue;
        }
    }
}
