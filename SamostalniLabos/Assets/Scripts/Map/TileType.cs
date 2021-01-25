namespace Assets.Scripts.Map
{
	public enum TileType
	{
		Pasture,
		Forest,
		WheatField,
		ClayPit,
		Mountain,
		Desert,
		None
	}

	public static class TileTypeUtils
	{
		public static TileType FromResource(string resource)
		{
			switch (resource.ToLower())
			{
				case "sheep": return TileType.Pasture;
				case "wood": return TileType.Forest;
				case "wheat": return TileType.WheatField;
				case "clay": return TileType.ClayPit;
				case "iron": return TileType.Mountain;
				case "dust": return TileType.Desert;
				default: return TileType.None;
			}
		}
	}

}