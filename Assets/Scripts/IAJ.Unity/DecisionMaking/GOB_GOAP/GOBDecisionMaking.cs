using System.Collections.Generic;
using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel.ForwardModelActions;
using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel;
using UnityEngine;
namespace Assets.Scripts.IAJ.Unity.DecisionMaking.GOB
{
    public class GOBDecisionMaking
    {
        public bool InProgress { get; set; }
        private List<Goal> goals { get; set; }
        private List<Action> actions { get; set; }

        public Action secondBestAction;

        // Utility based GOB
        public GOBDecisionMaking(List<Action> _actions, List<Goal> goals)
        {
            this.actions = _actions;
            this.goals = goals;
            secondBestAction = new Action("yo");
        }


        public static float CalculateDiscontentment(Action action, List<Goal> goals)
        {
            // Keep a running total
            var discontentment = 0.0f;
            var duration = action.GetDuration();

            foreach (var goal in goals)
            {
                // Calculate the new value after the action
                var newValue = goal.InsistenceValue + action.GetGoalChange(goal);

                // The change rate is how much the goals changes per time
                newValue += duration * goal.ChangeRate;

                //Calculate discontentment
                discontentment += goal.GetDiscontentment(newValue);
            }

            return discontentment;
        }

        public Action ChooseAction()
        {
            // Find the action leading to the lowest discontentment
            InProgress = true;
            Action bestAction = null;
            var bestValue = float.PositiveInfinity;

            //Goes through actions
            foreach (Action action in actions)
            {
                //Checks if they are executable
                if(action.CanExecute())
                {
                    //Calculate discontentment having each goal in mind
                    var value = CalculateDiscontentment(action, this.goals);

                    //If this is the best value yet, keep it
                    if(value < bestValue)
                    {
                        bestValue = value;
                        bestAction = action;
                    }
                }
            }
            
            //Change insistence of goals considering best action
            foreach(var goal in goals)
            {
                // Calculate the new value after the best action
                var newValue = goal.InsistenceValue + bestAction.GetGoalChange(goal);

                // The change rate is how much the goals changes per time
                newValue += bestAction.GetDuration() * goal.ChangeRate;

                goal.InsistenceValue = newValue;
            }

            InProgress = false;
            return bestAction;
        }
    }
}
