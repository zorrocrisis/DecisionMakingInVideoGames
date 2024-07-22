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

    public class Skeleton : Monster
    {
        public Skeleton()
        {
            this.enemyStats.Type = "Skeleton";
            this.enemyStats.XPvalue = 3;
            this.enemyStats.AC = 10;
            this.baseStats.HP = 5;
            this.DmgRoll = () => RandomHelper.RollD6();
            this.enemyStats.SimpleDamage = 2;
            this.enemyStats.AwakeDistance = 10;
            this.enemyStats.WeaponRange = 2;

        }


    }
}
