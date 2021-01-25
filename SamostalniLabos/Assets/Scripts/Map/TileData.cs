namespace Assets.Scripts.Map
{
	public class TileData
	{
		public (int x, int y) Position { get; private set; }
		public float Weight { get; private set; }
		public TileType TileType { get; private set; }

		public TileData(int x, int y, float weight, TileType tileType)
		{
			Position = (x, y);
			Weight = weight;
			TileType = tileType;
		}
	}
}