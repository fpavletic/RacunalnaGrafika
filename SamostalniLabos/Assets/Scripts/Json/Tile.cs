using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Json
{
	[Serializable]
	public class Tile
	{
		public string resourceType;
		public int resourceWeight;
		public Coordinates coordinates;
		public int x;
		public int y;
	}
}
