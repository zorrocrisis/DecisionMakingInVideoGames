using Assets.Scripts.Game;
using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel.ForwardModelActions
{
    public class GetManaPotion : WalkToTargetAndExecuteAction
    {
        private int expectedManaChange;

        public GetManaPotion(AutonomousCharacter character, GameObject target) : base("GetManaPotion", character, target)
        {
            this.expectedManaChange = 10;
        }

        public override bool CanExecute()
        {
            if (!base.CanExecute()) return false;
            return true; //There isn't a max mana value
        }

        public override bool CanExecute(WorldModel worldModel)
        {
            if (!base.CanExecute(worldModel)) return false;
            return true;
        }

        public override void Execute()
        {
            base.Execute();
            GameManager.Instance.GetManaPotion(this.Target);
        }

        public override float GetGoalChange(Goal goal)
        {
            var change = base.GetGoalChange(goal);

            //We have to forcefully choose a goal that isn't directly
            //related to the action... A mana potion will help the
            //character survive on the long run but in basic GOB,
            //we need to make that explicit
            if (goal.Name == AutonomousCharacter.SURVIVE_GOAL)
            {
                change -= 2.0f;           
            }

            return change;
        }

        public override void ApplyActionEffects(WorldModel worldModel)
        {
            base.ApplyActionEffects(worldModel);

            //Increases mana and decreases the insistence of the survival goal
            var surviveValue = worldModel.GetGoalValue(AutonomousCharacter.SURVIVE_GOAL);
            worldModel.SetGoalValue(AutonomousCharacter.SURVIVE_GOAL, surviveValue - surviveValue/2);

            int mana = (int)worldModel.GetProperty(Properties.MANA);
            worldModel.SetProperty(Properties.MANA, mana + this.expectedManaChange);

            //disables the target object so that it can't be reused again
            worldModel.SetProperty(this.Target.name, false);
        }

        public override float GetHValue(WorldModel worldModel)
        {
            //Always good to have mana
            return 2.0f;
        }
    }
}