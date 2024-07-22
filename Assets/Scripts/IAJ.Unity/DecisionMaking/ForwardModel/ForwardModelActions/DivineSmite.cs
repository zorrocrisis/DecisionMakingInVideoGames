using Assets.Scripts.Game;
using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel;
using Assets.Scripts.IAJ.Unity.Utils;
using System;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel.ForwardModelActions
{
    public class DivineSmite : WalkToTargetAndExecuteAction
    {
        private float expectedXPChange;
        private int expectedManaChange;
        private int xpChange;

        public DivineSmite(AutonomousCharacter character, GameObject target) : base("DivineSmite", character, target)
        {

            this.expectedManaChange = 2;
            this.xpChange = 3;
            this.expectedXPChange = 2.7f;
        }

        public override float GetGoalChange(Goal goal)
        {
            var change = base.GetGoalChange(goal);

            if (goal.Name == AutonomousCharacter.SURVIVE_GOAL)
            {
                change += this.expectedManaChange;
            }
            else if (goal.Name == AutonomousCharacter.GAIN_LEVEL_GOAL)
            {
                change -= this.expectedXPChange;
            }
            else if(goal.Name == AutonomousCharacter.BE_QUICK_GOAL)
            {
                //Its quicker to smite than to sword attack
                change -= goal.InsistenceValue;
            }

            return change;
        }

        public override bool CanExecute()
        {
            //Does the character have enough mana?
            if (!base.CanExecute() || this.expectedManaChange > this.Character.baseStats.Mana)
                return false;

            return true;
        }

        public override bool CanExecute(WorldModel worldModel)
        {
            int futureMana = (int)worldModel.GetProperty(Properties.MANA);

            if (!base.CanExecute(worldModel) || this.expectedManaChange > futureMana)
                return false;
            return true;
        }

        public override void Execute()
        {
            base.Execute();
            GameManager.Instance.DivineSmite(this.Target);
        }

        public override void ApplyActionEffects(WorldModel worldModel)
        {
            base.ApplyActionEffects(worldModel);

            int xp = (int)worldModel.GetProperty(Properties.XP);
            int mana = (int)worldModel.GetProperty(Properties.MANA);

            //Character doesn't suffer any damage by doing this attack
            //We don't need to calculate the damage to the monster either, since it immediately destroys the skeletons

            //Enemy is destroyed, disables the target object
            worldModel.SetProperty(this.Target.name, false);

            //Gain xp and lose mana
            worldModel.SetProperty(Properties.XP, xp + this.xpChange);

            //Since there is no max mana, we don't need to check if that value is exceeded
            worldModel.SetProperty(Properties.MANA, mana - this.expectedManaChange);

            //Change goal values
            var surviveValue = worldModel.GetGoalValue(AutonomousCharacter.SURVIVE_GOAL);
            var bequickValue = worldModel.GetGoalValue(AutonomousCharacter.BE_QUICK_GOAL);
            var levelValue = worldModel.GetGoalValue(AutonomousCharacter.GAIN_LEVEL_GOAL);
            worldModel.SetGoalValue(AutonomousCharacter.SURVIVE_GOAL, surviveValue + this.expectedManaChange);
            worldModel.SetGoalValue(AutonomousCharacter.SURVIVE_GOAL, bequickValue - bequickValue);
            worldModel.SetGoalValue(AutonomousCharacter.GAIN_LEVEL_GOAL, levelValue - this.expectedXPChange);

        }

        public override float GetHValue(WorldModel worldModel)
        {
            int level = (int)worldModel.GetProperty(Properties.LEVEL);

            //Quick xp gain and little to no risk... Always good, especially at lower levels
            if (level == 1)
            {
                return 4.0f;
            }
            else
                return 22.0f;
        }

    }
}
