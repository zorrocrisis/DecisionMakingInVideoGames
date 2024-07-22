using UnityEngine;
using System.Collections;
using System;
using Assets.Scripts.IAJ.Unity.Utils;
using UnityEngine.AI;
using Assets.Scripts.IAJ.Unity.DecisionMaking.BehaviorTree;
using Assets.Scripts.IAJ.Unity.DecisionMaking.BehaviorTree.BehaviourTrees;
using Assets.Scripts.IAJ.Unity.Formations;
using System.Collections.Generic;

namespace Assets.Scripts.Game.NPCs
{

    public class Orc : Monster
    {
        public GameObject patrolPoint1;
        public GameObject patrolPoint2;
        public bool heardShout = false;

        public Orc()
        {
            this.enemyStats.Type = "Orc";
            this.enemyStats.XPvalue = 10;
            this.enemyStats.AC = 14;
            this.baseStats.HP = 15;
            this.DmgRoll = () => RandomHelper.RollD10() + 2;
            this.enemyStats.SimpleDamage = 5;
            this.enemyStats.AwakeDistance = 10;
            this.enemyStats.WeaponRange = 3;
        }

        public override void InitializeBehaviourTree()
        {
            FindClosestPatrolPoints();

            //After finding the patrol points, we initialize our patrol movement
            this.BehaviourTree = new PatrolTree(this, this.patrolPoint1, this.patrolPoint2, this.Target);
        }

        private void FindClosestPatrolPoints()
        {
            //Returns array of all patrol poins
            var patrolPoints = GameObject.FindGameObjectsWithTag("PatrolPoint");
            float minDist = float.MaxValue - 1;
            float secondMinDist = float.MaxValue;

            //Finds 2 closest patrol points
            foreach (GameObject pp in patrolPoints)
            {
                var pp2DPosition = new Vector3(pp.transform.position.x, 0, pp.transform.position.z);
                var distance = (pp2DPosition - this.transform.position).magnitude;

                if (distance < minDist)
                {
                    secondMinDist = minDist;
                    minDist = distance;

                    this.patrolPoint2 = this.patrolPoint1;
                    this.patrolPoint1 = pp;
                }
                else if (distance < secondMinDist)
                {
                    secondMinDist = distance;
                    this.patrolPoint2 = pp;
                }
            }
        }

        public void RespondToShout(Vector3 shoutPosition)
        {
            //Shout heard, change behaviour
            heardShout = true;
            this.BehaviourTree = null;
            this.BehaviourTree = new HeardShoutTree(this, shoutPosition);
        }

        public void ResumeFromShout()
        {
            //Investigated shout, return to normal behaviour
            heardShout = false;
            this.BehaviourTree = null;
            this.BehaviourTree = new PatrolTree(this, this.patrolPoint1, this.patrolPoint2, this.Target);
        }

        public void FormationBroken()
        {
            //If the formation is broken, chase the player
            this.BehaviourTree = new ChaseTree(this, this.Target);
        }

        public void Sleep()
        {
            // Interrupt behaviour tree 
            Debug.Log("Sleeping...");
            this.usingBehaviourTree = false;
            this.StartPathfinding(transform.position);
        }

        public void AwakeFromSleeping()
        {
            // Continue behaviour tree
            Debug.Log("Not sleeping anymore...");
            this.usingBehaviourTree = true;
        }

    }
}
