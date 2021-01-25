using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Player;
using UnityEngine;

namespace Assets.Scripts.Map
{
	public class MapController : MonoBehaviour
	{
		private const float DEFAULT_SIZE = 7f;
		private const float DEFAULT_TRIANGLE_SIDE = .5f;
		private const float DEFAULT_TRIANGLE_HEIGHT = 0.43301270189f;

		[SerializeField] private GameObject[] _tilePrefabs = null;

		[SerializeField] private GameObject _roadPrefabRed = null;
		[SerializeField] private GameObject _roadPrefabBlue = null;
		[SerializeField] private GameObject _villagePrefabRed = null;
		[SerializeField] private GameObject _villagePrefabBlue = null;
		[SerializeField] private GameObject _townPrefabRed = null;
		[SerializeField] private GameObject _townPrefabBlue = null;
		[SerializeField] private BuilderController _builderRed = null;
		[SerializeField] private BuilderController _builderBlue = null;


		private IDictionary<(int x, int y), TileController> _positionToTile = new Dictionary<(int x, int y), TileController>();
		private IDictionary<TileType, GameObject> _tileTypeToPrefab = new Dictionary<TileType, GameObject>();

		private int _mapSize = 7;

		private void Awake()
		{
			foreach (var prefab in _tilePrefabs)
			{
				if (Enum.TryParse<TileType>(prefab.name, out var type))
				{
					_tileTypeToPrefab[type] = prefab;
				}
				else
				{
					Debug.LogError($"Unable to parse tile prefab: {prefab.name}");
				}
			}
		}

		public void CreateMap(TileData[] tileData, int mapSize)
		{
			_mapSize = mapSize;

			foreach (var tile in _positionToTile.Values)
			{
				Destroy(tile.gameObject);
			}
			_positionToTile.Clear();

			for (int i = 0; i < tileData.Length; i++)
			{
				var tile = Instantiate(_tileTypeToPrefab[tileData[i].TileType], transform).GetComponent<TileController>();
				tile.UpdateWeight(tileData[i].Weight);
				tile.UpdateScale(CalculateScale(_mapSize));
				tile.UpdatePosition(CalculateOffset(tileData[i], _mapSize));
				_positionToTile[tileData[i].Position] = tile;
			}
		}

		public void BuildRoad((int x, int y)[] tileData, int playerId)
		{
			Build(tileData, playerId == 0 ? _roadPrefabRed : _roadPrefabBlue);
		}

		public void BuildVillage((int x, int y)[] tileData, int playerId)
		{
			Build(tileData, playerId == 0 ? _villagePrefabRed : _villagePrefabBlue);
		}

		public void BuildTown((int x, int y)[] tileData, int playerId)
		{
			Build(tileData, playerId == 0 ? _townPrefabRed : _townPrefabBlue);
		}

		public void MoveBuilder((int x, int y)[] tileData, int playerId)
		{
			(playerId == 0 ? _builderRed : _builderBlue).Move(tileData,
				tileData.Select(CalculateOffset));
		}

		private StructureController Build((int x, int y)[] tileData, GameObject prefab)
		{
			var structure = Instantiate(prefab, transform).GetComponent<StructureController>();
			structure.SetLocalPosition(tileData.Select(CalculateOffset));
			return structure;
		}

		public Vector3 CalculateOffset((int x, int y) tileData)
		{
			var (x, y) = tileData;
			if (x > _mapSize / 2)
			{
				y += x - _mapSize / 2;
			}

			return new Vector3((y - _mapSize / 2 + (x - _mapSize / 2)) * DEFAULT_TRIANGLE_HEIGHT, 0f, -(x - _mapSize / 2 - (y - _mapSize / 2)) * 1.5f * DEFAULT_TRIANGLE_SIDE) * DEFAULT_SIZE / _mapSize;
		}

		private Vector3 CalculateOffset(TileData tileData, int size)
		{
			var (x, y) = tileData.Position;
			if ( x > size / 2)
			{
				y += x - size / 2;
			}

			return new Vector3((y - size / 2 + (x - size / 2)) * DEFAULT_TRIANGLE_HEIGHT, 0f, -(x - size / 2 - (y - size / 2)) * 1.5f * DEFAULT_TRIANGLE_SIDE ) * DEFAULT_SIZE / size;
		}

		private float CalculateScale(int size)
		{
			return DEFAULT_SIZE / size;
		}
	}
}