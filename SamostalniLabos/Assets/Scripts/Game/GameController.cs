using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Json;
using Assets.Scripts.Map;
using Assets.Scripts.Player;
using UnityEngine;

namespace Assets.Scripts.Game {
    public class GameController : MonoBehaviour
    {
	    private const int RequiredVictoryPoints = 22;

        [SerializeField] private MapController _map = null;
        [SerializeField] private PlayerController[] _playerControllers = null;

        private GameState _gameState = GameState.WaitingForMap;
        private CanvasGroup _victoryCanvasGroup = null;
		private string _winner = default;

        private readonly Queue<Action> _actionQueue = new Queue<Action>();
        
        void Start()
        {
			ConnectionController.Instance.AddListener(ConnectionHandler);
	    }

		// Update is called once per frame
		void Update()
		{
			if (_actionQueue.Count != 0)
	        {
		        _actionQueue.Dequeue()();
	        }

			if (_gameState == GameState.Finished)
			{
				_victoryCanvasGroup.alpha += .2f * Time.deltaTime;
			}
        }

		public void ExitGame()
		{
			Application.Quit();
		}

		private void ConnectionHandler(string message)
		{
			switch (_gameState)
			{
				case GameState.WaitingForMap:
					_gameState = MapBuilder(message);
					break;
				case GameState.WaitingForMoves:
					_gameState = MoveHandler(message);
					break;
				case GameState.Finished:
					//TODO: Declare victor
					break;
			}
		}
		private GameState MoveHandler(string message)
		{
			var moves = JsonUtility.FromJson<OnMove>(message);
			var parameters = moves.playerAction.parameters;
			var playerId = moves.currentPlayerStats.playerId == _playerControllers[0].Id ? 0 : 1;

			_actionQueue.Enqueue(() =>
			{
				_playerControllers[playerId].SetResource(TileType.ClayPit, moves.currentPlayerStats.resourceStack.clayStockpile);
				_playerControllers[playerId].SetResource(TileType.Mountain, moves.currentPlayerStats.resourceStack.ironStockpile);
				_playerControllers[playerId].SetResource(TileType.Pasture, moves.currentPlayerStats.resourceStack.sheepStockpile);
				_playerControllers[playerId].SetResource(TileType.WheatField, moves.currentPlayerStats.resourceStack.wheatStockpile);
				_playerControllers[playerId].SetResource(TileType.Forest, moves.currentPlayerStats.resourceStack.woodStockpile);

				_playerControllers[playerId].VictoryPoints.ResourceCount = moves.currentPlayerStats.victoryPoints;

				switch (moves.playerAction.action.ToLower())
				{
					case "initial":
						_map.BuildVillage(Enumerable.Range(0, 3)
							.Select(i => (parameters[2*i +1] -1, parameters[2*i] -1)).ToArray(), playerId);
						_map.BuildRoad(Enumerable.Range(0, 2)
							.Select(i => (parameters[6 + 2*i + 1] -1, parameters[6 + 2*i] -1)).ToArray(), playerId);
						break;
					case "buildroad":
						_map.BuildRoad(Enumerable.Range(0, 2)
							.Select(i => (parameters[2*i + 1] - 1, parameters[2*i] - 1)).ToArray(), playerId);
						break;
					case "buildtown":
						_map.BuildVillage(Enumerable.Range(0, 3)
							.Select(i => (parameters[2*i + 1] - 1, parameters[2*i] - 1)).ToArray(), playerId);
						break;
					case "upgradetown":
						Debug.Log(parameters.Aggregate("", (s, i) => s + i));
						_map.BuildTown(Enumerable.Range(0, 3)
							.Select(i => (parameters[2*i + 1] - 1, parameters[2*i] - 1)).ToArray(), playerId);
						break;
					case "move":
						_map.MoveBuilder(Enumerable.Range(0, 3)
							.Select(i => (parameters[2 * i + 1] - 1, parameters[2 * i] - 1)).ToArray(), playerId);
						break;
					case "empty":
						break;
				}
			});

			return _playerControllers[playerId].VictoryPoints.ResourceCount >= RequiredVictoryPoints ? GameState.Finished : GameState.WaitingForMoves;
		}

		private GameState MapBuilder(string message)
		{
			var game = JsonUtility.FromJson<OnSubscription>(message.Replace("[[", "[").Replace("],[", ",").Replace("]]", "]"));
			var tiles = game.map.tiles
				.Where(t => t.resourceType != null)
				.Where(t => t.resourceType.ToLower() != "water")
				.Select(t =>
					new TileData(t.y - 1, t.x - 1, t.resourceWeight, TileTypeUtils.FromResource(t.resourceType)))
				.ToArray();
			
			_actionQueue.Enqueue(() =>
			{
				Debug.Log("Creating map");
				_map.CreateMap(tiles, game.map.height - 2);
				_playerControllers[0].Id = game.player1Id; //Red
				_playerControllers[0].Name = game.player1Name;
				_playerControllers[1].Id = game.player2Id; //Blue
				_playerControllers[1].Name = game.player2Name;
			});
			return GameState.WaitingForMoves;
		}
    }
}