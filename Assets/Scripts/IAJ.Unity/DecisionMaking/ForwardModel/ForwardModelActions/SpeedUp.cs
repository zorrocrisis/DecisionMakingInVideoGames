using Assets.Scripts.IAJ.Unity.DecisionMaking.GOB;
using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel;
using Assets.Scripts.Game;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel.ForwardModelActions
{
    public class SpeedUp : Action
    {
        public AutonomousCharacter Character { get; private set; }
        private int expectedManaChange;

        public SpeedUp(AutonomousCharacter character) : base("SpeedUp")
        {
            this.Character = character;
            this.expectedManaChange = 5;
        }

        public override float GetGoalChange(Goal goal)
        {
            var change = base.GetGoalChange(goal);

            if (goal.Name == AutonomousCharacter.BE_QUICK_GOAL)
            {
                change = -goal.InsistenceValue;
            }

            return change;
        }


        public override bool CanExecute()
        {
            //Can only execute after level 2, if there is enough mana and if the character isnt already sped up
            var level = this.Character.baseStats.Level;

            if (level < 2 || this.expectedManaChange > this.Character.baseStats.Mana || this.Character.SpedUp)
                return false;
            else
                return true;
        }


        public override bool CanExecute(WorldModel worldModel)
        {
            int futureLevel = (int)worldModel.GetProperty(Properties.LEVEL);
            int futureMana = (int)worldModel.GetProperty(Properties.MANA);

            if (futureLevel < 2 || this.expectedManaChange > futureMana)
                return false;
            else
                return true;
        }

        public override void Execute()
        {
            GameManager.Instance.SpeedUp();
        }

        public override void ApplyActionEffects(WorldModel worldModel)
        {
            //Change mana
            int mana = (int)worldModel.GetProperty(Properties.MANA);
            worldModel.SetProperty(Properties.MANA, mana - this.expectedManaChange);

            //Change goal values
            worldModel.SetGoalValue(AutonomousCharacter.BE_QUICK_GOAL, 0);
        }

        public override float GetHValue(WorldModel worldModel)
        {
            //Speeding up might be a good ideia almost everytime
            return 1.0f;
        }
    }
}

