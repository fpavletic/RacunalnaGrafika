using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Json
{
	[Serializable]
	public class OnSubscription
	{
		public int player1Id;
		public int player2Id;
		public string player1Name;
		public string player2Name;
		public Map map;
	}
}
