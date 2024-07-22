using Assets.Scripts.Game;
using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel.ForwardModelActions;
using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel;
using System;
using System.Collections.Generic;
using UnityEngine;
using Action = Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel.Action;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.MCTS
{
    public class MCTSBiasedPlayout : MCTS
    {
        //Child of MCTS - we only need to override the playout method
        public float heuristicValue;

        public MCTSBiasedPlayout(CurrentStateWorldModel currentStateWorldModel) : base(currentStateWorldModel)
        {
        }

        protected override Reward Playout(WorldModel initialPlayoutState)
        {
            Action[] executableActions;
            WorldModel worldState = initialPlayoutState.GenerateChildWorldModel();
            int playoutDepth = 0;

            //Normal playout
            while (!worldState.IsTerminal())
            {
                //Update executable actions
                executableActions = worldState.GetExecutableActions();

                if (executableActions == null)
                    break;

                //Choose action based on heuristic
                Action biasedAction = ChooseBiasedAction(executableActions, worldState);

                //And apply its effect on the world state
                biasedAction.ApplyActionEffects(worldState);

                //Needed for multiple players
                worldState.CalculateNextPlayer();

                //Debugging variable
                playoutDepth += 1;
            }

            //Register max playout depth
            if (playoutDepth > this.MaxPlayoutDepthReached)
                this.MaxPlayoutDepthReached = playoutDepth;

            //Calculate and return reward for terminal world state
            //(we assume the last player playing is Sir Uthgard)
            Reward reward = new Reward()
            {
                Value = worldState.GetScore(),
                PlayerID = 0
            };

            return reward;
        }


        public Action ChooseBiasedAction(Action [] executableActions, WorldModel worldState)
        {
            this.heuristicValue = 100f;
            Action biasedAction = executableActions[0];
            float value = 0; 

            //For each executable action...
            foreach(Action action in executableActions)
            {
                //Calculate the heuristic value and normalize it...
                value = action.GetHValue(worldState)/100f;
                
                //We pick the lowest value so far and store the action (greedy approach)
                if(value < this.heuristicValue)
                {
                    this.heuristicValue = value;
                    biasedAction = action;
                }
            }

            return biasedAction;
        }

    }
}
