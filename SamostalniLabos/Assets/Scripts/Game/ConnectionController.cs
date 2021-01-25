using System;
using System.Collections.Generic;
using System.IO;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.Scripts.Game
{
	public class ConnectionController
	{
		private const int ReceiveBufferSize = 8192;

		private static ConnectionController _instance;
		public static ConnectionController Instance
		{
			get
			{
				return _instance = _instance ?? new ConnectionController();
			}
			private set => _instance = value;
		}

		public WebSocketState State => _webSocket.State;

		private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
		private ClientWebSocket _webSocket;
		private ISet<Action<string>> _listeners = new HashSet<Action<string>>();

		public void AddListener(Action<string> listener) => _listeners.Add(listener);

		public void RemoveListener(Action<string> listener) => _listeners.Remove(listener);

		public async void Connect(string ipAndPort)
		{
			if (_webSocket != null && _webSocket.State != WebSocketState.None && _webSocket.State != WebSocketState.Closed)
			{
				await _webSocket.CloseAsync(WebSocketCloseStatus.Empty, "Closing", _cancellationTokenSource.Token);
			}
			_webSocket = new ClientWebSocket();
			
			var serverUri = new Uri($"ws://{ipAndPort.Substring(0, ipAndPort.Length -1)}/streaming/websocket/");
			await _webSocket.ConnectAsync(serverUri, _cancellationTokenSource.Token);
			await Task.Factory.StartNew(ReceiveLoop, _cancellationTokenSource.Token, TaskCreationOptions.LongRunning,
				TaskScheduler.Default);
		}

		private async void ReceiveLoop()
		{
			var loopToken = _cancellationTokenSource.Token;
			MemoryStream outputStream = null;
			WebSocketReceiveResult receiveResult = null;
			var buffer = new byte[ReceiveBufferSize];
			var bufferSegment = new ArraySegment<byte>(buffer, 0, buffer.Length);
			try
			{
				while (!loopToken.IsCancellationRequested)
				{
					outputStream = new MemoryStream(ReceiveBufferSize);
					do
					{
						receiveResult = await _webSocket.ReceiveAsync(bufferSegment, _cancellationTokenSource.Token);
						if (receiveResult.MessageType != WebSocketMessageType.Close)
							outputStream.Write(buffer, 0, receiveResult.Count);
					}
					while (!receiveResult.EndOfMessage);
					if (receiveResult.MessageType == WebSocketMessageType.Close) break;
					outputStream.Flush();
					outputStream.Position = 0;
					ResponseReceived(outputStream);
				}
			}
			catch (TaskCanceledException) { }
			finally
			{
				outputStream?.Dispose();
			}
		}

		private void ResponseReceived(MemoryStream outputStream)
		{
			var message = new StreamReader(outputStream).ReadToEnd();
			foreach (var listener in _listeners)
			{
				listener($"{message}");
			}
		}
	}
}