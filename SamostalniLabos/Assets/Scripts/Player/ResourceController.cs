using TMPro;
using UnityEngine;

namespace Assets.Scripts.Player
{
    public class ResourceController : MonoBehaviour
    {
        [SerializeField] private string _resourceType = null;
        public string ResourceType
        {
            get { return _resourceType; }
            private set { _resourceType = value; }
        }

        [SerializeField] private TextMeshProUGUI _resourceCountText = null;

        private int _resourceCount;

        public int ResourceCount
        {
            get
            {
                return _resourceCount;
            }

            set
            {
                _resourceCount = value;
                _resourceCountText.text = value.ToString();
            }
        }
    }
}