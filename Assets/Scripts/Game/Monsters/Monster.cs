using UnityEngine;
using System.Collections;
using System;
using Assets.Scripts.IAJ.Unity.Utils;
using UnityEngine.AI;
using Assets.Scripts.IAJ.Unity.DecisionMaking.BehaviorTree;
using Assets.Scripts.IAJ.Unity.DecisionMaking.BehaviorTree.BehaviourTrees;
using Assets.Scripts.IAJ.Unity.Formations;
using System.Collections.Generic;
using UnityEngine.UI;

namespace Assets.Scripts.Game.NPCs
{

    public class Monster : NPC
    {

        //Audio effect and visual cue for the shout action
        public AudioSource audioSource;
        public ParticleSystem visualCue;

        [Serializable]
        public struct EnemyStats
        {
            public string Type;
            public int XPvalue;
            public int AC;
            public int SimpleDamage;
            public float AwakeDistance;
            public float WeaponRange;
        }

        public EnemyStats enemyStats;

        public Func<int> DmgRoll;  //how do you like lambda's in c#?

        protected bool usingBehaviourTree;
        protected float decisionRate = 2.0f;
        protected NavMeshAgent agent;
        public GameObject Target { get; set; }
        public Task BehaviourTree;

        private FormationManager formationManager;
        private bool usingFormation;

        void Start()
        {
            //SFX and VFX sources for shout action
            this.audioSource = this.GetComponent<AudioSource>();
            this.visualCue = this.GetComponent<ParticleSystem>();

            agent = this.GetComponent<NavMeshAgent>();
            this.Target = GameObject.FindGameObjectWithTag("Player");
            this.usingBehaviourTree = GameManager.Instance.BehaviourTreeNPCs;

            if (!usingBehaviourTree && !GameManager.Instance.SleepingNPCs)
                Invoke("CheckPlayerPosition", 1.0f);

            if (usingBehaviourTree)
                InitializeBehaviourTree();

        }

        public virtual void InitializeBehaviourTree()
        {
            // Done in the children's class
        }

        void FixedUpdate()
        {
            if (usingBehaviourTree && this.BehaviourTree != null)
                    this.BehaviourTree.Run();
        }

        // Very basic Enemy AI
        void CheckPlayerPosition()
        {
            if (Vector3.Distance(this.transform.position, this.Target.transform.position) < enemyStats.AwakeDistance)
            {

                if (Vector3.Distance(this.transform.position, this.Target.transform.position) <= enemyStats.WeaponRange)
                {
                    AttackPlayer();
                }

                else
                {
                   
                    PursuePlayer();
                    Invoke("CheckPlayerPosition", 0.5f);
                }
            }
            else if (usingFormation)
                this.formationManager.UpdateSlots();
            

            Invoke("CheckPlayerPosition", 2.0f);
        }


        //These are the 3 basic actions the Monsters can make (+ Shout, which was added)
        public void PursuePlayer()
        {
            if(agent != null)
                this.agent.SetDestination(this.Target.transform.position);
        }

        public void AttackPlayer()
        {
            GameManager.Instance.EnemyAttack(this.gameObject);
        }

        public void MoveTo(Vector3 targetPosition)
        {
            if (agent != null)
                this.agent.SetDestination(targetPosition);
        }

        public void Shout()
        {
            audioSource.Play();
            visualCue.Play();
        }

     }
}
