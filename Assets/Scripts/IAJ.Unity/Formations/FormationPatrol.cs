using Assets.Scripts.Game;
using Assets.Scripts.Game.NPCs;
using Assets.Scripts.IAJ.Unity.Formations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static GameManager;

public class FormationPatrol
{
    public GameObject anchorPoint;
    public FormationManager FormationManager;

    //Patrol points
    public GameObject patrolPoint1;
    public GameObject patrolPoint2;
    public GameObject currentPP;

    public GameObject player;

    public List<Monster> orcs;

    //Auxiliary flag
    public bool disableFormation = false;

    public FormationPatrol(FormationManager formationManager, GameObject anchor)
    {
        this.anchorPoint = anchor;
        this.FormationManager = formationManager;
        this.orcs = formationManager.monsters;

        //Find player
        this.player = GameObject.FindGameObjectWithTag("Player");

        //Find formation patrol poins
        patrolPoint1 = GameObject.Find("FormationPatrolPoint1");
        patrolPoint2 = GameObject.Find("FormationPatrolPoint2");

        //Find closest formation patrol point and define it as the current patrol point
        if (Vector3.Distance(anchorPoint.transform.position, patrolPoint1.transform.position)
                                   < Vector3.Distance(anchorPoint.transform.position, patrolPoint2.transform.position))

            this.currentPP = patrolPoint1;
        else
            this.currentPP = patrolPoint2;

    }

    public void UpdatePatrol()
    {
        
        //If player is within awake distance
        if(Vector3.Distance(this.anchorPoint.transform.position, this.player.transform.position) < 10f)
        {
            this.disableFormation = true;
        }

        if(!disableFormation)
        {
            //If we are close to the current patrol point, change to the other patrol point
            if (Vector3.Distance(this.anchorPoint.transform.position, this.currentPP.transform.position) < 3f)
            {
                if (this.currentPP.Equals(this.patrolPoint1))
                    this.currentPP = patrolPoint2;
                else
                    this.currentPP = patrolPoint1;
            }

            //Move anchor point
            anchorPoint.GetComponent<NavMeshAgent>().destination = currentPP.transform.position;
            this.FormationManager.AnchorPosition = this.anchorPoint.transform.position;

            //Update slots and move npcs
            this.FormationManager.UpdateSlots();
        }
    }
}
