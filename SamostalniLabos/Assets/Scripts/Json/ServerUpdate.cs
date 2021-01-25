using System;

namespace Assets.Scripts.Json
{
	[Serializable]
	public class ServerUpdate
	{
		public int playerId;
		public string action;
		public int victoryPoints;
		public ResourceStack resourceStack1;
		public ResourceStack resourceStack2;
	}
}
