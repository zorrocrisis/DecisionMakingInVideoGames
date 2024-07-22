using Assets.Scripts.Game;
using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel.ForwardModelActions
{
    public class ShieldOfFaith : Action
    {
        public AutonomousCharacter Character { get; private set; }
        private int maxHPShield;
        private int expectedManaChange;

        public ShieldOfFaith(AutonomousCharacter character) : base("ShieldOfFaith")
        {
            this.Character = character;
            this.maxHPShield = 5;
            this.expectedManaChange = 5;
        }

        public override bool CanExecute()
        {
            //Does the character have enough mana?
            if (this.expectedManaChange > Character.baseStats.Mana || Character.baseStats.ShieldHP == this.maxHPShield)
                return false;

            return true;
        }

        public override bool CanExecute(WorldModel worldModel)
        {
            int futureMana = (int)worldModel.GetProperty(Properties.MANA);
            int futureHPShield = (int)worldModel.GetProperty(Properties.ShieldHP);

            if (!base.CanExecute(worldModel) || this.expectedManaChange > futureMana
                                             || futureHPShield == this.maxHPShield)
                return false;
            return true;
        }

        public override void Execute()
        {
            base.Execute();
            GameManager.Instance.ShieldOfFaith();
        }

        public override float GetGoalChange(Goal goal)
        {
            var change = base.GetGoalChange(goal);

            if (goal.Name == AutonomousCharacter.SURVIVE_GOAL)
            {
                change += this.expectedManaChange;
            }

            return change;
        }

        public override void ApplyActionEffects(WorldModel worldModel)
        {
            base.ApplyActionEffects(worldModel);

            //Changes the insistence of the survival goal and maxes the hp shield (even if recast)
            int surviveValue = (int)worldModel.GetGoalValue(AutonomousCharacter.SURVIVE_GOAL);
            worldModel.SetGoalValue(AutonomousCharacter.SURVIVE_GOAL, surviveValue + this.expectedManaChange);

            worldModel.SetProperty(Properties.ShieldHP, this.maxHPShield);
        }

        public override float GetHValue(WorldModel worldModel)
        {
            int shieldHP = (int)worldModel.GetProperty(Properties.ShieldHP);
            int level = (int)worldModel.GetProperty(Properties.LEVEL);

            //If we already have a full shield, dont use another
            if (shieldHP == this.maxHPShield)
            {
                return 100f;
            }
            //Shield is better at low levels
            else if (shieldHP < this.maxHPShield/2.0f)
            {
                if (level == 1)
                {
                    return 10f;
                }
                else if (level == 2)
                {
                    return 30f;
                }
                else
                {
                    return 50f;
                }
            }
            else
            {
                return 80.0f;
            }
        }
    }
}
