using Assets.Scripts.Game;
using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel.ForwardModelActions;
using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel;
using System;
using System.Collections.Generic;
using UnityEngine;
using Action = Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel.Action;
using System.Linq;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.MCTS
{
    public class MCTSLimitedPlayout : MCTS
    {
        //Child of MCTS - we only need to override the playout method
        public int depthLimit;

        public MCTSLimitedPlayout(CurrentStateWorldModel currentStateWorldModel) : base(currentStateWorldModel)
        {
            this.depthLimit = 5;
        }

        protected override Reward Playout(WorldModel initialPlayoutState)
        {
            Action[] executableActions;
            WorldModel worldState = initialPlayoutState.GenerateChildWorldModel();
            int playoutDepth = 0;

            //Normal playout but we also have a depth limit
            while (!worldState.IsTerminal() || playoutDepth != this.depthLimit + 1)
            {
                //Update executable actions
                executableActions = worldState.GetExecutableActions();

                //Check if there is at least 1 executable action
                if (executableActions.Length == 0)
                    break;

                //Choose random action
                var randomActionIndex = RandomGenerator.Next(0, executableActions.Length - 1);
                Action randomAction = executableActions[randomActionIndex];

                //And apply its effect on the world state
                randomAction.ApplyActionEffects(worldState);

                //Needed for multiple players
                worldState.CalculateNextPlayer();

                //Debugging variable
                playoutDepth += 1;
            }

            //Register max playout depth
            if (playoutDepth > this.MaxPlayoutDepthReached)
                this.MaxPlayoutDepthReached = playoutDepth;

            //The reward value (quality of state) is going to be calculated through a heuristic
            float quality = worldState.GetHeuristicQualityOfState();

            //Return reward for reached world state
            //(since the world could be not terminal, we need to get the next player)
            Reward reward = new Reward()
            {
                Value = quality,
                PlayerID = worldState.GetNextPlayer()
            };

            return reward;
        }
    }
}
