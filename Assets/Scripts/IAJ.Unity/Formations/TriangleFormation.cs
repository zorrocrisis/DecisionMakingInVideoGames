using System.Collections;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.Formations
{
    public class TriangleFormation : FormationPattern
    {
        // This is a very simple line formation, with the anchor being the position of the character at index 0.
        private static readonly float offset = 3.0f;

        public TriangleFormation()
        {

        }

        public override Vector3 GetOrientation(FormationManager formation, Vector3 orientation)
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

            //Different orientations for each slot 
            if(orientation.x == 1)
            {
                var x1 = -1.0f * Mathf.Sin(Mathf.PI / 6.0f) * Mathf.Cos(rotation.y) - 1.0f * Mathf.Cos(Mathf.PI / 6.0f) * Mathf.Sin(rotation.y);
                var z1 = -1.0f * Mathf.Cos(Mathf.PI / 6.0f) * Mathf.Cos(rotation.y) + Mathf.Sin(Mathf.PI / 6.0f) * Mathf.Sin(rotation.y);
                Vector3 v1 = new Vector3(x1, 0, z1);
                return v1;
            }
            else
            {
                var x2 = Mathf.Sin(Mathf.PI / 6.0f) * Mathf.Cos(rotation.y) - 1.0f * Mathf.Cos(Mathf.PI / 6.0f) * Mathf.Sin(rotation.y);
                var z2 = -1.0f * Mathf.Cos(Mathf.PI / 6.0f) * Mathf.Cos(rotation.y) - 1.0f * Mathf.Sin(Mathf.PI / 6.0f) * Mathf.Sin(rotation.y);
                Vector3 v2 = new Vector3(x2, 0, z2);
                return v2;
            }
        }

        public override Vector3 GetSlotLocation(FormationManager formation, int slotNumber) => slotNumber switch
        {
            0 => formation.AnchorPosition,
            1 => formation.AnchorPosition + offset * GetOrientation(formation, new Vector3(slotNumber, 0, 0)),
            2 => formation.AnchorPosition + offset * GetOrientation(formation, new Vector3(slotNumber, 0, 0))
        };

        public override bool SupportSlot(int slotCount)
        {
            return (slotCount <= 3);
        }


    }
}
