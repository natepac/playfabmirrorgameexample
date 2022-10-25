namespace PlayFab.Networking
{
	using System;
	using System.Collections.Generic;
	using UnityEngine;
	using Mirror;
	using UnityEngine.Events;

	public class UnityNetworkServer : NetworkBehaviour
	{
		public Configuration configuration;

		public PlayerEvent OnPlayerAdded = new PlayerEvent();
		public PlayerEvent OnPlayerRemoved = new PlayerEvent();

		public int MaxConnections = 100;
		public int Port = 7777;

		public NetworkManager _networkManager;

		public List<UnityNetworkConnection> Connections
		{
			get { return _connections; }
			private set { _connections = value; }
		}
		private List<UnityNetworkConnection> _connections = new List<UnityNetworkConnection>();

		public class PlayerEvent : UnityEvent<string> { }

		void Awake()
		{
			if (configuration.buildType == BuildType.REMOTE_SERVER)
			{
				AddRemoteServerListeners();
			}
		}

		private void AddRemoteServerListeners()
		{
			Debug.Log("[UnityNetworkServer].AddRemoteServerListeners");
			//NetworkServer.OnConnectedEvent = OnServerConnect;
			//NetworkServer.OnDisconnectedEvent = OnServerDisconnect;
			//NetworkServer.OnErrorEvent = OnServerError;
			NetworkServer.RegisterHandler<ReceiveAuthenticateMessage>(OnReceiveAuthenticate);
		}

		public void StartServer()
		{
			NetworkServer.Listen(MaxConnections);
		}

		private void OnApplicationQuit()
		{
			NetworkServer.Shutdown();
		}

		private void OnReceiveAuthenticate( NetworkConnection nconn, ReceiveAuthenticateMessage message )
		{
			var conn = _connections.Find(c => c.ConnectionId == nconn.connectionId);
			if (conn != null)
			{
				conn.PlayFabId = message.PlayFabId;
				conn.IsAuthenticated = true;
				OnPlayerAdded.Invoke(message.PlayFabId);
			}
		}

		private void OnServerConnect( NetworkConnection connection )
		{
			Debug.LogWarning("Client Connected");
			var conn = _connections.Find(c => c.ConnectionId == connection.connectionId);
			if (conn == null)
			{
				_connections.Add(new UnityNetworkConnection()
				{
					Connection = connection,
					ConnectionId = connection.connectionId,
					LobbyId = PlayFabMultiplayerAgentAPI.SessionConfig.SessionId
				});
			}
		}

		private void OnServerError( NetworkConnection conn, TransportError error, string reason )
		{
			Debug.LogFormat( "Unity Network Connection Status: error - {0}, reason: {1}", error.ToString(), reason );
		}

		private void OnServerDisconnect( NetworkConnection connection )
		{
			var conn = _connections.Find(c => c.ConnectionId == connection.connectionId);
			if (conn != null)
			{
				if (!string.IsNullOrEmpty( conn.PlayFabId))
				{
					OnPlayerRemoved.Invoke( conn.PlayFabId);
				}
				_connections.Remove(conn);
			}
		}

	}

	[Serializable]
	public class UnityNetworkConnection
	{
		public bool IsAuthenticated;
		public string PlayFabId;
		public string LobbyId;
		public int ConnectionId;
		public NetworkConnection Connection;
	}

	public class CustomGameServerMessageTypes
	{
		public const short ReceiveAuthenticate = 900;
		public const short ShutdownMessage = 901;
		public const short MaintenanceMessage = 902;
	}

	public struct ReceiveAuthenticateMessage : NetworkMessage
	{
		public string PlayFabId;
	}

	public struct ShutdownMessage : NetworkMessage { }

	[Serializable]
	public struct MaintenanceMessage : NetworkMessage
	{
		public DateTime ScheduledMaintenanceUTC;
	}

	public static class MaintenanceMessageFunctions
	{
		public static MaintenanceMessage Deserialize( this NetworkReader reader )
		{
			var json = PlayFab.PluginManager.GetPlugin<ISerializerPlugin>( PluginContract.PlayFab_Serializer );
			DateTime ScheduledMaintenanceUTC = json.DeserializeObject<DateTime>( reader.ReadString() );
			MaintenanceMessage value = new MaintenanceMessage
			{
				ScheduledMaintenanceUTC = ScheduledMaintenanceUTC
			};

			return value;
		}

		public static void Serialize( this NetworkWriter writer, MaintenanceMessage value )
		{
			var json = PlayFab.PluginManager.GetPlugin<ISerializerPlugin>( PluginContract.PlayFab_Serializer );
			var str = json.SerializeObject( value.ScheduledMaintenanceUTC );
			writer.Write( str );
		}
	}
}