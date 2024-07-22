using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace Assets.Scripts.IAJ.Unity.Formations
{
    public class LineFormation : FormationPattern 
    {
        // This is a very simple line formation, with the anchor being the position of the character at index 0.
        private static readonly float offset = 3.0f;

        public LineFormation()
        {
        }

        public override Vector3 GetOrientation(FormationManager formation, Vector3 orientation )
        {
            //old old
            //Quaternion rotation = formation.SlotAssignment[0].transform.rotation;
            //Vector2 orientation = new Vector2(rotation.x, rotation.z);
            //return orientation;

            //old
            //Quaternion rotation = formation.SlotAssignment.Keys.First().transform.rotation;

            //In this formation, the orientation is defined by the first character's transform rotation (in degrees)...
            Vector3 rotation = formation.SlotAssignment.Keys.First().transform.rotation.eulerAngles;

            //Change degrees into radians
            rotation = rotation * Mathf.Deg2Rad;

            //Get final orientation (same direction, reverse sense) used to calculate slot locations
            Vector3 finalOrientation = new Vector3(Mathf.Sin(rotation.y) * -1.0f, 0, Mathf.Cos(rotation.y) * -1.0f);

            return finalOrientation;
        }

        public override Vector3 GetSlotLocation(FormationManager formation, int slotNumber) => slotNumber switch
        {
            0 => formation.AnchorPosition,
            _ => formation.AnchorPosition + offset * slotNumber * this.GetOrientation(formation, new Vector3(0,0,0))
        };

        public override  bool SupportSlot(int slotCount)
        {
            return (slotCount <= 3); 
        }
    }
}