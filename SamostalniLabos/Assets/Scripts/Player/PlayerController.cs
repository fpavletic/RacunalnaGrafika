using Assets.Scripts.Map;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Player { 
	public class PlayerController : MonoBehaviour
	{
		[SerializeField]
		private Color _color = default;
		[SerializeField] 
		private TextMeshProUGUI _name = null;


		public int Id = default;
		public string Name
		{
			get => _name.text;
			set => _name.text = value;
		}
		public ResourceController VictoryPoints = null;

		private Resources _resources;

		private void Start()
		{
			gameObject.GetComponent<Image>().color = _color;
			_resources = new Resources(gameObject.GetComponentsInChildren<ResourceController>());
		}

		//public void MoveBuilder((int x, int y)[])
		//{

		//}

		public void SetResource(TileType type, int resourceCount)
		{
			_resources[type] = resourceCount;
		}

		public void IncrementResource(TileType type, int stepCount)
		{
			_resources[type] += stepCount;
		}
	}
}