using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Json
{
	[Serializable]
	public class Map
	{
		public Tile[] tiles;
		public int width;
		public int height;
	}
}
