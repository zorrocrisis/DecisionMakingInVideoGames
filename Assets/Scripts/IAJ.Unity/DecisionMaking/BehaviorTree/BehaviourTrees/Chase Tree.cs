using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Assets.Scripts.Game;
using Assets.Scripts.Game.NPCs;
using Assets.Scripts.IAJ.Unity.DecisionMaking.BehaviorTree.EnemyTasks;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.BehaviorTree.BehaviourTrees
{
    class ChaseTree: Sequence
    {
        //Similar to the BasicTree but specialized in chasing the player (giving up if they are too far)
        public ChaseTree(Monster character, GameObject target)
        {
            this.children = new List<Task>()
            {
                new IsCharacterNearTarget(character, target, character.enemyStats.AwakeDistance),
                new Shout(character, target, character.enemyStats.AwakeDistance),
                new Chase(character, target, character.enemyStats.WeaponRange),
                new LightAttack(character),
            };
        }
    }
}