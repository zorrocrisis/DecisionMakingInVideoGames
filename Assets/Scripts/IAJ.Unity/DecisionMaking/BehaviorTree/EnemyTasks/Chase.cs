using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.AI;
using Assets.Scripts.Game.NPCs;
using Assets.Scripts.Game;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.BehaviorTree.EnemyTasks
{
    class Chase : Task
    {
        protected NPC Character { get; set; }

        public GameObject Target { get; set; }

        public float range;

        //This auxiliary variable determines at what distance do monsters stop chasing the target (player)
        public float bravery;

        //This classe is extremely similar to MoveTo, the major difference being the bravery variable: if
        //the target is too far away, the task will fail. We need to make sure this target is a player!
        public Chase(Monster character, GameObject target, float _range)
        {
            this.Character = character;
            this.Target = target;
            range = _range;
            bravery = 10f;
        }

        public override Result Run()
        {
            //The target must be a player
            if (Target == null || Target.tag != "Player")
                return Result.Failure;

            //If the player is within range...
            if (Vector3.Distance(Character.transform.position, this.Target.transform.position) <= range)
            {
                return Result.Success;
            }

            //If the player haas escaped...
            else if (Vector3.Distance(Character.transform.position, this.Target.transform.position) >= range * bravery)
            {
                return Result.Failure;
            }

            //If the orc is still after the player
            else
            {
                Character.StartPathfinding(Target.transform.position);
                return Result.Running;
            }
        }

    }
}
