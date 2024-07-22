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
    class Shout : Task
    {
        protected Monster Character { get; set; }
        public GameObject Target { get; set; }

        public float range;

        public Shout(Monster character, GameObject target, float _range)
        {
            this.Character = character;
            this.Target = target;
            range = _range;
        }

        public override Result Run()
        {

           //Warn other orcs (they will move to where the shout came from)
           var orcs = GameObject.FindGameObjectsWithTag("Orc");
           foreach (GameObject orc in orcs)
           {
                //If it isnt the orc who shouted, respond to shout
                if (!orc.GetComponent<Monster>().Equals(this.Character))
                {
                    orc.GetComponent<Orc>().RespondToShout(Character.transform.position);
                }
           }

            //Play VFX and SFX
            this.Character.Shout();

            return Result.Success;
        }
    }
}
