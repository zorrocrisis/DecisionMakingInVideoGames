using Assets.Scripts.Game;
using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel.ForwardModelActions
{
    public class Pray : Action
    {
        public AutonomousCharacter Character { get; private set; }
        private float restingInterval;
        private int restHPRecovery;

        public Pray(AutonomousCharacter character) : base("Pray")
        {
            this.Character = character;
            this.restingInterval = 5.0f;
            this.restHPRecovery = 2;

            //Important for the change rate of goals...
            this.Duration += this.restingInterval;
        }

        public override bool CanExecute()
        {
            if (this.Character.baseStats.HP == this.Character.baseStats.MaxHP) return false;
            return true;
        }

        public override bool CanExecute(WorldModel worldModel)
        {
            int futureHP = (int)worldModel.GetProperty(Properties.HP);
            int futureMaxHP = (int)worldModel.GetProperty(Properties.MAXHP);

            if (futureHP == futureMaxHP)
                return false;
            else
                return true;
        }

        public override void Execute()
        {
            base.Execute();
            GameManager.Instance.Pray();
        }

        public override float GetGoalChange(Goal goal)
        {
            var change = base.GetGoalChange(goal);

            if (goal.Name == AutonomousCharacter.SURVIVE_GOAL)
            {
                change -= this.restHPRecovery;
            }
            //Put pressure because of the time lost
            else if(goal.Name == AutonomousCharacter.GET_RICH_GOAL)
            {
                //Low change values seems to make Sir Uthgard
                //a very stubborn man of faith (he abuses this action)
                change += goal.InsistenceValue;
            }
            else if(goal.Name == AutonomousCharacter.BE_QUICK_GOAL)
            {
                change += 10;
            }

            return change;
        }

        public override void ApplyActionEffects(WorldModel worldModel)
        {
            base.ApplyActionEffects(worldModel);

            //Changes health values (checking if we dont go over the max value)
            int hp = (int)worldModel.GetProperty(Properties.HP);
            int maxHP = (int)worldModel.GetProperty(Properties.MAXHP);

            if(hp + this.restHPRecovery > maxHP)
                worldModel.SetProperty(Properties.HP, maxHP);
            else
                worldModel.SetProperty(Properties.HP, hp + this.restHPRecovery);

            //Changes goal values
            float surviveValue = worldModel.GetGoalValue(AutonomousCharacter.SURVIVE_GOAL);
            float richValue = worldModel.GetGoalValue(AutonomousCharacter.GET_RICH_GOAL);
            float quickValue = worldModel.GetGoalValue(AutonomousCharacter.BE_QUICK_GOAL);
            worldModel.SetGoalValue(AutonomousCharacter.SURVIVE_GOAL, surviveValue - this.restHPRecovery);
            worldModel.SetGoalValue(AutonomousCharacter.BE_QUICK_GOAL, quickValue + this.restingInterval);
            worldModel.SetGoalValue(AutonomousCharacter.GET_RICH_GOAL, richValue + 10);
        }

        public override float GetHValue(WorldModel worldModel)
        {
            int maxHP = (int)worldModel.GetProperty(Properties.MAXHP);
            int HP = (int)worldModel.GetProperty(Properties.HP);
            int level = (int)worldModel.GetProperty(Properties.LEVEL);

            //Praying is better early game...
            if (HP > maxHP/2)
            {
                if (level == 1)
                {
                    return 5f;
                }
                else if (level == 2)
                {
                    return 93f;
                }
                else
                {
                    return 100f;
                }
            }
            return 98f;
        }
    }
}
