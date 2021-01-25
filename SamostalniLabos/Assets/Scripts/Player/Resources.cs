using System;
using System.Collections.Generic;
using Assets.Scripts.Map;
using UnityEngine;

namespace Assets.Scripts.Player
{
	public class Resources
	{
		private readonly IDictionary<TileType, ResourceController> _resourceTypeToController = new Dictionary<TileType, ResourceController>();

		public Resources(IEnumerable<ResourceController> resourceControllers)
		{
			foreach(var resourceController in resourceControllers)
			{
				if (!Enum.TryParse<TileType>(resourceController.ResourceType, out var resourceType))
				{
					//throw new ArgumentException($"{resourceController.ResourceType} is not a resource type!");
					Debug.LogWarning($"{resourceController.ResourceType} is not a resource type!");
				}

				_resourceTypeToController[resourceType] = resourceController;
			}
		}

		public int this[TileType type]
		{
			get => _resourceTypeToController[type].ResourceCount;
			set => _resourceTypeToController[type].ResourceCount = value;
		}

		public void Increment(TileType type, int stepCount)
		{
			var controller = _resourceTypeToController[type];
			if ( controller == null)
			{
				throw new ArgumentException("Provided resource type is invalid!");
			}

		}
	}
}