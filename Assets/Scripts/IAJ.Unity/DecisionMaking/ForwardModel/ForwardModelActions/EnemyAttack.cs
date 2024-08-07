﻿
using Assets.Scripts.Game;
using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel;
using Assets.Scripts.IAJ.Unity.Utils;
using System;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel.ForwardModelActions
{
    public class EnemyAttack : SwordAttack
    {

        public EnemyAttack(AutonomousCharacter character, GameObject target) : base(character, target)
        {
            this.Name = "EnemyAttack(" + target.name + ")";
        }
       
        public override void Execute()
        {
            base.Execute();
            GameManager.Instance.EnemyAttack(this.Target);
        }
    }
}
