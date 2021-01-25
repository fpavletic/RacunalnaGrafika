using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Json
{
	[Serializable]
	public class OnMove
	{
		/// <summary>
		/// THIS ID IS INCORRECT! Use the one from currentPlayerStats
		/// </summary>
		public int playerId;
		public PlayerAction playerAction;
		public PlayerStats currentPlayerStats;
		public PlayerStats otherPlayerStats;
	}

	[Serializable]
	public class PlayerStats
	{
		public int playerId;
		public int victoryPoints;
		public ResourceStack resourceStack;
	}

	[Serializable]
	public class PlayerAction
	{
		public string action;
		public int[] parameters;
	}
}
