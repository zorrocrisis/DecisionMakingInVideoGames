using UnityEditor;
using UnityEngine;
using Assets.Scripts.Game;
using Assets.Scripts.Game.NPCs;
using System.Collections.Generic;
using UnityEngine.AI;

namespace Assets.Scripts.IAJ.Unity.Formations
{
    public class FormationManager
    {
        public Dictionary<Monster, int> SlotAssignment = new Dictionary<Monster, int>();

        private FormationPattern pattern;

        // # A Static (i.e., position and orientation) representing the
        // # drift offset for the currently filled slots.

        public Vector3 AnchorPosition;

        public Vector3 Orientation;

        //Auxiliary variable to be accessed by FormationPatrol
        public List<Monster> monsters;

        public GameObject slot1;
        public GameObject slot2;


        public FormationManager(List<Monster> NPCs, FormationPattern pattern, Vector3 position, Vector3 orientation )
        {
            this.pattern = pattern;
            this.AnchorPosition = position;
            this.monsters = NPCs;

            //Auxiliary game objects to help visualize formation
            this.slot1 = GameObject.Find("Slot1");
            this.slot2 = GameObject.Find("Slot2");

            int i = 0;
            foreach (Monster npc in NPCs)
            {
                SlotAssignment[npc] = i;
                i++;
            }

            // the pattern may define specific rules for the orientation...
            this.Orientation = pattern.GetOrientation( this, orientation );
        }

        public void UpdateSlotAssignements()
        {
            int i = 0; 
            foreach(var npc in  SlotAssignment.Keys)
            {
                SlotAssignment[npc] = i;
                i++;
            }
        }

        public bool AddCharacter(Monster character)
        {
            var occupiedSlots = this.SlotAssignment.Count;
            if (this.pattern.SupportSlot(occupiedSlots + 1))
            {
                SlotAssignment.Add(character, occupiedSlots);
                this.UpdateSlotAssignements();
                return true;
            }
            else return false;
        }

        public void RemoveCharacter(Monster character)
        {
            var slot = SlotAssignment[character];
            SlotAssignment.Remove(character);
            UpdateSlots();
        }

        public void UpdateSlots()
        {
            var anchor = this.AnchorPosition;

            foreach (var npc in SlotAssignment.Keys)
            {
                int slotNumber = SlotAssignment[npc];
                var locationPosition = pattern.GetSlotLocation(this, slotNumber);

                // and add drift component.

                locationPosition -= Vector3.one * 0.1f;
                //locationOrientation -= 0.1f;

                //Visualize slot locations
                if (slotNumber == 1)
                    this.slot1.transform.position = locationPosition;
                if (slotNumber == 2)
                    this.slot2.transform.position = locationPosition;

                npc.MoveTo(locationPosition);
                npc.GetComponent<NavMeshAgent>().updateRotation = true;
            }
        }

        public void RemoveAll()
        {
            foreach (var npc in SlotAssignment.Keys)
                this.SlotAssignment.Remove(npc);
        }

    }
}