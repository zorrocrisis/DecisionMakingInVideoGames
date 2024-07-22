using Assets.Scripts.IAJ.Unity.DecisionMaking.BehaviorTree.EnemyTasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Assets.Scripts.Game;
using Assets.Scripts.Game.NPCs;


namespace Assets.Scripts.IAJ.Unity.DecisionMaking.BehaviorTree.BehaviourTrees
{
    class HeardShoutTree : Sequence
    {
        public HeardShoutTree(Monster character, Vector3 target)
        {
            this.children = new List<Task>()
            {
                new MoveToShout(character, target, character.enemyStats.WeaponRange)
            };
        }
    }
}