using Assets.Scripts.Game;
using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel.ForwardModelActions;
using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel;
using System;
using System.Collections.Generic;
using UnityEngine;
using Action = Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel.Action;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.MCTS
{
    public class MCTSMultiplePlayouts : MCTS
    {
        //Child of MCTS - we only need to override the playout method

        public uint numberPlayouts;
        public float[] multipleRewards;

        public MCTSMultiplePlayouts(CurrentStateWorldModel currentStateWorldModel, uint nPlayouts) : base (currentStateWorldModel)
        {
            this.numberPlayouts = nPlayouts;

            //Array of floats where we'll keep the reward values from the multiple playouts
            multipleRewards = new float[this.numberPlayouts];
        }

        protected override Reward Playout(WorldModel initialPlayoutState)
        {
            //Do multiple playouts
            for(uint i = 0; i < this.numberPlayouts; i++)
            {
                //Normal playout..
                Action[] executableActions;
                WorldModel worldState = initialPlayoutState.GenerateChildWorldModel();
                int playoutDepth = 0;

                while (!worldState.IsTerminal())
                {
                    //Update executable actions
                    executableActions = worldState.GetExecutableActions();

                    if (executableActions == null)
                        break;

                    //Choose random executable action
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

                //Register the reward value for each playout
                multipleRewards[i] = worldState.GetScore();
            }

            //Finally, find average of all reward values and return average reward
            float sumOfRewardValues = 0;

            foreach (float rewardValue in multipleRewards)
            {
                sumOfRewardValues += rewardValue;
            }

            Reward averageReward = new Reward()
            {
                Value = sumOfRewardValues / this.numberPlayouts,
                PlayerID = 0
            };

            return averageReward;
        }

    }
}
