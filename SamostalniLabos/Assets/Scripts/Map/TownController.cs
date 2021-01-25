using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Map
{
    public class TownController : StructureController
    {
        public override void SetLocalPosition(IEnumerable<Vector3> positions)
        {
            transform.localPosition = AverageLocalPositions(positions) + Vector3.up * .2f;
        }
    }
}