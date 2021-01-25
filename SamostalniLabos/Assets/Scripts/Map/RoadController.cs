using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Map
{
	public class RoadController : StructureController
	{
		public override void SetLocalPosition(IEnumerable<Vector3> positions)
		{
			var positionsArray = positions.ToArray();
			transform.localPosition = AverageLocalPositions(positionsArray);
			transform.forward = (positionsArray[1] - positionsArray[0]).normalized;
			transform.Rotate(Vector3.up, 45);
			transform.localPosition += .2f * Vector3.up;
			//transform.Rotate(Vector3.up, 90f);

		}
	}
}