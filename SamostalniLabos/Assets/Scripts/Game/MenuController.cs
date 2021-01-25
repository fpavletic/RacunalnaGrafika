using System.Collections;
using System.Net.WebSockets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Game
{

	public class MenuController : MonoBehaviour
	{
		private const string Connecting = "Connecting to the server, please wait.";
		private const string ConnectionFailedRetrying = "Connection failed!\n Retrying in {0} seconds...";
		private const int ConnectionFailedRetryTimer = 5;

#pragma warning disable 649
		[SerializeField] private CanvasGroup _menuCanvasGroup = null;
		[SerializeField] private TextMeshProUGUI _serverIp = null;
		[SerializeField] private TMP_InputField _serverInputField = null;
		[SerializeField] private TMP_InputField _gameInputField = null;
		[SerializeField] private Button _connectButton = null;

		[SerializeField] private CanvasGroup _connectingCanvasGroup = null;
		[SerializeField] private TextMeshProUGUI _connectionStatusText = null;
		[SerializeField] private Button _cancelButton = null;

		[SerializeField] private CanvasGroup _connectedCanvasGroup = null;
#pragma warning restore 649

		private int _mapReceived = 0;
		private int _retryTimer = -1;
		private Coroutine _connectCoroutine = null;
		
		public void Connect()
		{
			MenuInteractable(false);
			_connectingCanvasGroup.alpha = 1f;
			ConnectingInteractable(true);

			_connectCoroutine = StartCoroutine(ConnectCoroutine());
		}

		public void Cancel()
		{
			if (_connectCoroutine != null)
			{
				StopCoroutine(_connectCoroutine);
			}

			ConnectingInteractable(false);
			_connectingCanvasGroup.alpha = 0f;
			MenuInteractable(true);
		}

		private void MenuInteractable(bool interactable)
		{
			_menuCanvasGroup.blocksRaycasts = interactable;

			_connectButton.interactable = interactable;
			_serverInputField.interactable = interactable;
			_gameInputField.interactable = interactable;
			_menuCanvasGroup.interactable = interactable;
		}

		private void ConnectingInteractable(bool interactable)
		{
			_connectingCanvasGroup.blocksRaycasts = interactable;

			_connectingCanvasGroup.interactable = interactable;
			_cancelButton.interactable = interactable;
		}

		private IEnumerator ConnectCoroutine()
		{
			ConnectionController.Instance.Connect(_serverIp.text);
			_connectionStatusText.text = Connecting;

			_connectingCanvasGroup.alpha = 1;
			while (ConnectionController.Instance.State == WebSocketState.Connecting)
			{
				yield return 0;
				if (ConnectionController.Instance.State != WebSocketState.Closed) continue;
				_retryTimer = ConnectionFailedRetryTimer;

				while (_retryTimer > 0)
				{
					_connectionStatusText.text = string.Format(ConnectionFailedRetrying, _retryTimer);
					yield return new WaitForSeconds(1f);
					_retryTimer -= 1;
				}

				ConnectionController.Instance.Connect(_serverIp.text);
				_connectionStatusText.text = Connecting;
			}

			ConnectionController.Instance.AddListener((m) => _mapReceived--);

			ConnectingInteractable(false);
			while (_connectingCanvasGroup.alpha > 0)
			{
				_connectedCanvasGroup.alpha += 2f * Time.deltaTime;
				_connectingCanvasGroup.alpha -= 2f * Time.deltaTime;
				yield return 0;
			}
		}

		public void Update()
		{
			if (_mapReceived == -1)
			{
				StartCoroutine(HideMenuCoroutine());
			}
		}

		public IEnumerator HideMenuCoroutine()
		{
			while (_connectedCanvasGroup.alpha > 0)
			{
				_connectedCanvasGroup.alpha -= .2f * Time.deltaTime;
				_menuCanvasGroup.alpha -= .2f * Time.deltaTime;
				yield return 0;
			}

		}
	}
}
