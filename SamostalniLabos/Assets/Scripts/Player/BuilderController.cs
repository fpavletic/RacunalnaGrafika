using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Player
{
	public class BuilderController : MonoBehaviour
	{
		private (int x, int y)[] _tileData = null;

		public (int x, int y)[] TileData => _tileData;

		public void Move((int x, int y)[] tileData, IEnumerable<Vector3> vectorTileData)
		{
			_tileData = tileData;
			transform.localPosition = vectorTileData.Aggregate(Vector3.zero, (v1, v2) => v1 + v2) / vectorTileData.Count();
		}

	}
}
