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
    class PatrolTree : Sequence
    {
        public PatrolTree(Monster character, GameObject target1, GameObject target2, GameObject target)
        {
            var chase_patrol1 = new List<Task>()
            {
                new ChaseTree(character, target),
                new MoveTo(character, target1, character.enemyStats.WeaponRange)
            };

            var chase_patrol2 = new List<Task>()
            {
                new ChaseTree(character, target),
                new MoveTo(character, target2, character.enemyStats.WeaponRange)
            };

            this.children = new List<Task>()
            {
                new Selector (chase_patrol1),
                new Selector (chase_patrol2)
            };
        }
    }
}