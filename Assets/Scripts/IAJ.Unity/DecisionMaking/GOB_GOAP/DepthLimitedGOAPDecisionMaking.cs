using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel.ForwardModelActions;
using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel;
using Assets.Scripts.Game;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.GOB
{
    public class DepthLimitedGOAPDecisionMaking
    {
        public const int MAX_DEPTH = 3;
        public int ActionCombinationsProcessedPerFrame { get; set; }
        public float TotalProcessingTime { get; set; }
        public int TotalActionCombinationsProcessed { get; set; }
        public bool InProgress { get; set; }

        public CurrentStateWorldModel InitialWorldModel { get; set; }
        private List<Goal> Goals { get; set; }
        private WorldModel[] Models { get; set; }
        private Action[] ActionPerLevel { get; set; }
        public Action[] BestActionSequence { get; private set; }
        public Action BestAction { get; private set; }
        public float BestDiscontentmentValue { get; private set; }
        private int CurrentDepth {  get; set; }

        public DepthLimitedGOAPDecisionMaking(CurrentStateWorldModel currentStateWorldModel, List<Action> actions, List<Goal> goals)
        {
            this.ActionCombinationsProcessedPerFrame = 200;
            this.Goals = goals;
            this.InitialWorldModel = currentStateWorldModel;
        }

        public void InitializeDecisionMakingProcess()
        {
            this.InProgress = true;
            this.TotalProcessingTime = 0.0f;
            this.TotalActionCombinationsProcessed = 0;
            this.CurrentDepth = 0;
            this.Models = new WorldModel[MAX_DEPTH + 1];
            this.Models[0] = this.InitialWorldModel;
            this.ActionPerLevel = new Action[MAX_DEPTH];
            this.BestActionSequence = new Action[MAX_DEPTH];
            this.BestAction = null;
            this.BestDiscontentmentValue = float.MaxValue;
            this.InitialWorldModel.Initialize();
        }

        public Action ChooseAction()
        {
            var startTime = Time.realtimeSinceStartup;
            float currentValue;
            float bestValue = float.MaxValue;
            Action nextAction = null;

            //While there are world states to "explore" and the action combinations per frame limit hasnt been reached
            while (this.CurrentDepth >= 0 && this.TotalActionCombinationsProcessed < this.ActionCombinationsProcessedPerFrame)
            {
                //If we've reached the max depth...
                if (this.CurrentDepth >= MAX_DEPTH)
                {
                    //Calculate the discontentment for the current action combination
                    currentValue = this.Models[this.CurrentDepth].CalculateDiscontentment(Goals);
                    
                    //Store the best action and best action sequence (lowest discontentment) so far
                    if(currentValue < bestValue)
                    {
                        bestValue = currentValue;
                        this.BestAction = this.ActionPerLevel[0];
                        this.BestActionSequence = this.ActionPerLevel;
                        this.BestDiscontentmentValue = bestValue;
                    }

                    this.CurrentDepth -= 1;

                    //Keep count of action combinations processed (every time we reach the max depth + 1)
                    this.TotalActionCombinationsProcessed += 1;

                    continue;
                }

                //Get next executable action and apply its effect on the following world state
                nextAction = Models[this.CurrentDepth].GetNextAction();
                if (nextAction != null)
                {
                    this.Models[this.CurrentDepth + 1] = this.Models[this.CurrentDepth].GenerateChildWorldModel();
                    nextAction.ApplyActionEffects(this.Models[this.CurrentDepth + 1]);
                    this.ActionPerLevel[this.CurrentDepth] = nextAction;
                    this.CurrentDepth += 1;
                }
                else
                    this.CurrentDepth -= 1;
            }
            
            //Keep count of processing time
            this.TotalProcessingTime += Time.realtimeSinceStartup - startTime;

            this.InProgress = false;

            //Change insistence of goals considering best action
            foreach (var goal in this.Goals)
            {
                // Calculate the new value after the best action
                var newValue = goal.InsistenceValue + BestAction.GetGoalChange(goal);

                // The change rate is how much the goals changes per time
                newValue += BestAction.GetDuration() * goal.ChangeRate;

                goal.InsistenceValue = newValue;
            }

            return this.BestAction;
        }
    }
}
