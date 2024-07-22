using Assets.Scripts.Game;
using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel.ForwardModelActions
{
    public class GetHealthPotion : WalkToTargetAndExecuteAction
    {
        public GetHealthPotion(AutonomousCharacter character, GameObject target) : base("GetHealthPotion",character,target)
        {
        }

        public override bool CanExecute()
        {
            //It doesn't make sense for the character use a HP potion with full health... But is this author design?
            if (!base.CanExecute() || this.Character.baseStats.HP == this.Character.baseStats.MaxHP)
                return false;
            return true;
        }

        public override bool CanExecute(WorldModel worldModel)
        {
            int futureHP = (int)worldModel.GetProperty(Properties.HP);
            int futureMaxHP = (int)worldModel.GetProperty(Properties.MAXHP);

            if (!base.CanExecute(worldModel) || futureHP == futureMaxHP)
                return false;
            return true;
        }

        public override void Execute()
        {
            base.Execute();
            GameManager.Instance.GetHealthPotion(this.Target);
        }

        public override float GetGoalChange(Goal goal)
        {
            var change = base.GetGoalChange(goal);

            if (goal.Name == AutonomousCharacter.SURVIVE_GOAL)
            {
                change -= goal.InsistenceValue;
            }

            return change;
        }

        public override void ApplyActionEffects(WorldModel worldModel)
        {
            base.ApplyActionEffects(worldModel);

            //Changes health to max and the insistence of the survival goal to 0
            int maxHP = (int)worldModel.GetProperty(Properties.MAXHP);
            float surviveValue = worldModel.GetGoalValue(AutonomousCharacter.SURVIVE_GOAL); 
            worldModel.SetProperty(Properties.HP, maxHP);
            worldModel.SetGoalValue(AutonomousCharacter.SURVIVE_GOAL, surviveValue - surviveValue);

            //disables the target object so that it can't be reused again
            worldModel.SetProperty(this.Target.name, false);
        }

        public override float GetHValue(WorldModel worldModel)
        {
            int maxHP = (int)worldModel.GetProperty(Properties.MAXHP);
            int HP = (int)worldModel.GetProperty(Properties.HP);
            int level = (int)worldModel.GetProperty(Properties.LEVEL);

            //Health potions are more important during late game...
            if (HP <= maxHP/2)
            {
                if(level == 1)
                {
                    return 80f;
                }
                else if (level == 2)
                {
                    return 40f;
                }
                else
                {
                    return 10f;
                }
            }
            else
                return 90f;
        }
    }
}
