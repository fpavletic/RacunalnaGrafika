using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Map
{
	public abstract class StructureController : MonoBehaviour
	{
		public abstract void SetLocalPosition(IEnumerable<Vector3> positions);

		protected static Vector3 AverageLocalPositions(IEnumerable<Vector3> positions)
		{	
			return positions.Aggregate(Vector3.zero, (a, p) => a + p) / positions.Count();
		}
	}
}