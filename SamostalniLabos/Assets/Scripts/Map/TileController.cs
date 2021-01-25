using TMPro;
using UnityEngine;

namespace Assets.Scripts.Map
{
	public class TileController : MonoBehaviour
	{
		private TextMeshPro _weightText;
		void Awake()
		{
			_weightText = gameObject.GetComponentInChildren<TextMeshPro>();
		}

		public void UpdateWeight(float weight)
		{
			_weightText.transform.parent.gameObject.SetActive(weight != 0);
			_weightText.text = weight.ToString("0.##");
		}

		public void UpdatePosition(Vector3 coordinates)
		{
			transform.localPosition = coordinates;
		}
		public void UpdateScale(float scale)
		{
			transform.localScale = new Vector3(scale, 2, scale);
		}

	}
}