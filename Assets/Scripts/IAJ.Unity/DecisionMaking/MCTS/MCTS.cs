using Assets.Scripts.Game;
using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel.ForwardModelActions;
using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel;
using System;
using System.Collections.Generic;
using UnityEngine;
using Action = Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel.Action;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.MCTS
{
    public class MCTS
    {
        public const float C = 1.4f;
        public bool InProgress { get; set; }
        public int MaxIterations { get; set; }
        public int MaxIterationsProcessedPerFrame { get; set; }
        public int MaxPlayoutDepthReached { get; set; }
        public int MaxSelectionDepthReached { get; private set; }
        public float TotalProcessingTime { get; private set; }
        public MCTSNode BestFirstChild { get; set; }
        public List<Action> BestActionSequence { get; private set; }
        public WorldModel BestActionSequenceWorldState { get; private set; }
        protected int CurrentIterations { get; set; }
        protected int CurrentIterationsInFrame { get; set; }
        protected int CurrentDepth { get; set; }
        protected CurrentStateWorldModel CurrentStateWorldModel { get; set; }
        protected MCTSNode InitialNode { get; set; }
        protected System.Random RandomGenerator { get; set; }
        

        public MCTS(CurrentStateWorldModel currentStateWorldModel)
        {
            //this.InProgress = true;
            this.CurrentStateWorldModel = currentStateWorldModel;
            this.MaxIterations = 100;
            this.MaxIterationsProcessedPerFrame = 500;
            this.RandomGenerator = new System.Random();
        }


        public void InitializeMCTSearch()
        {
            this.MaxPlayoutDepthReached = 0;
            this.MaxSelectionDepthReached = 0;
            this.CurrentIterations = 0;
            this.CurrentIterationsInFrame = 0;
            this.TotalProcessingTime = 0.0f;
            this.CurrentStateWorldModel.Initialize();

            // create root node n0 for state s0
            this.InitialNode = new MCTSNode(this.CurrentStateWorldModel)
            {
                Action = null,
                Parent = null,
                PlayerID = 0
            };
            this.InProgress = true;
            this.BestFirstChild = null;
            this.BestActionSequence = new List<Action>();
        }

        public virtual Action Run()
        {
            MCTSNode selectedNode = null;
            Reward reward;

            //Debugging auxiliary variables
            var startTime = Time.realtimeSinceStartup;
            this.CurrentIterationsInFrame = 0;

            //While within computational budget
            while (this.CurrentIterationsInFrame < this.MaxIterationsProcessedPerFrame)
            {
                //Selection + Expansion
                selectedNode = Selection(this.InitialNode);

                //Playout
                reward = Playout(selectedNode.State);

                //Backpropagation
                Backpropagate(selectedNode, reward);

                //Debugging variable
                this.CurrentIterationsInFrame += 1;
                this.CurrentIterations += 1;
            }

            //Keep count of processing time
            this.TotalProcessingTime += Time.realtimeSinceStartup - startTime;

            this.InProgress = false;

            //Return action of best first child
            return BestFinalAction(this.InitialNode);
        }

        // Selection and Expansion
        protected MCTSNode Selection(MCTSNode initialNode)
        {
            Action nextAction;
            MCTSNode currentNode = initialNode;
            MCTSNode bestChild;

            int maxSelectionDepthReached = 0;

            //While game hasn't reached the end
            while (!currentNode.State.IsTerminal())
            {
                //Get next action...
                nextAction = currentNode.State.GetNextAction();

                //Debugging variables
                maxSelectionDepthReached += 1;
                if (maxSelectionDepthReached > this.MaxSelectionDepthReached)
                    this.MaxSelectionDepthReached = maxSelectionDepthReached;

                //If node is already fully expanded...
                if (nextAction == null)
                {
                    bestChild = BestUCTChild(currentNode);
                    currentNode = bestChild;
                }
                //If node hasn't been fully expanded - Expand
                else
                {
                    //Expand the node
                    MCTSNode childNode = Expand(currentNode, nextAction);
                    return childNode;
                }
            }
            return currentNode;
        }

        protected virtual Reward Playout(WorldModel initialPlayoutState)
        {
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

            //Calculate and return reward for terminal world state
            //(we assume the last player playing is Sir Uthgard)
            Reward reward = new Reward()
            {
                Value = worldState.GetScore(),
                PlayerID = 0
            };

            return reward;
        }

        protected virtual void Backpropagate(MCTSNode node, Reward reward)
        {
            while(node != null)
            {
                //Increment play count, win count (if simulation ended in win)
                node.N += 1;

                //If next player is Sir Uthgard, give reward
                if(node.State.GetNextPlayer() == 0)
                {
                    node.Q += reward.Value;
                }
                //If Sir Uthgard got too close to an enemy, don't give him full reward
                else if (node.State.GetNextPlayer() == 1)
                {
                    node.Q += reward.Value/2;
                }

                //Keep going up the tree
                node = node.Parent;
            }
        }

        protected MCTSNode Expand(MCTSNode parent, Action action)
        {
            //Generate new world state and apply action effects to it
            WorldModel newState = parent.State.GenerateChildWorldModel();
            action.ApplyActionEffects(newState);
            newState.CalculateNextPlayer();

            //Assign new world state and action to child node 
            MCTSNode childNode = new MCTSNode(newState)
            {
                Action = action,
                Parent = parent,
                PlayerID = newState.GetNextPlayer() //Account for multiple players
            };

            //Update child nodes here
            parent.ChildNodes.Add(childNode);

            return childNode;
        }

        protected virtual MCTSNode BestUCTChild(MCTSNode node)
        {
            float mu;
            int nNode;
            int nParent;
            double UCT;

            double bestUCT = 0;
            MCTSNode bestUCTNode = null;

            foreach(MCTSNode childNode in node.ChildNodes)
            {
                //Calculate mu - estimated value for the node (average utility)
                mu = childNode.Q / childNode.N;

                //Number of times the node has been visited
                nNode = childNode.N;

                //Number of times the parent node has been visited
                nParent = node.N;

                //UCT formula (C is the exploration factor)
                UCT = mu + C * Math.Pow((Math.Log(nParent) / nNode), 0.5f);

                //If better than current best UCT, keep the value and the node
                if (UCT > bestUCT)
                {
                    bestUCT = UCT;
                    bestUCTNode = childNode;
                }
            }

            return bestUCTNode;
        }

        //this method is very similar to the bestUCTChild, but it is used to return the final action of the MCTS search, and so we do not care about
        //the exploration factor
        protected MCTSNode BestChild(MCTSNode node)
        {
            MCTSNode bestChild = null;
            float bestmu = 0;
            float mu;

            foreach (MCTSNode childNode in node.ChildNodes)
            {
                //Calculate mu - estimated value for the node (average utility)
                mu = childNode.Q / childNode.N;

                //If better than current best mu, keep the value and the node
                //The equal makes that if theres a tie between all nodes, the bestchild isn't null
                if (mu >= bestmu)
                {
                    bestmu = mu;
                    bestChild = childNode;
                }
            }

            return bestChild;
        }

        
        protected Action BestFinalAction(MCTSNode node)
        {
            var bestChild = this.BestChild(node);
            if (bestChild == null) return null;

            this.BestFirstChild = bestChild;

            //this is done for debugging proposes only
            this.BestActionSequence = new List<Action>();
            this.BestActionSequence.Add(bestChild.Action);
            node = bestChild;

            while(!node.State.IsTerminal())
            {
                bestChild = this.BestChild(node);
                if (bestChild == null) break;
                this.BestActionSequence.Add(bestChild.Action);
                node = bestChild;
                this.BestActionSequenceWorldState = node.State;
            }

            return this.BestFirstChild.Action;
        }

    }
}
