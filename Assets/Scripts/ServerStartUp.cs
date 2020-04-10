using System.Collections;
using UnityEngine;
using PlayFab;
using System;
using PlayFab.Networking;
using System.Collections.Generic;
using PlayFab.MultiplayerAgent.Model;
using Mirror;

public class ServerStartUp : MonoBehaviour
{

	public Configuration configuration;

	private List<ConnectedPlayer> _connectedPlayers;
	public UnityNetworkServer UNetServer;

	public NetworkManager networkManager;

	void Start()
	{
		if (configuration.buildType == BuildType.REMOTE_SERVER)
		{
			StartRemoteServer();
		}
	}

	public void OnStartLocalServerButtonClick()
	{
		if (configuration.buildType == BuildType.LOCAL_SERVER)
		{
			networkManager.StartServer();
		}
	}

	private void StartRemoteServer()
	{
		Debug.Log("[ServerStartUp].StartRemoteServer");
		_connectedPlayers = new List<ConnectedPlayer>();
		PlayFabMultiplayerAgentAPI.Start();
		PlayFabMultiplayerAgentAPI.IsDebugging = configuration.playFabDebugging;
		PlayFabMultiplayerAgentAPI.OnMaintenanceCallback += OnMaintenance;
		PlayFabMultiplayerAgentAPI.OnShutDownCallback += OnShutdown;
		PlayFabMultiplayerAgentAPI.OnServerActiveCallback += OnServerActive;
		PlayFabMultiplayerAgentAPI.OnAgentErrorCallback += OnAgentError;

		UNetServer.OnPlayerAdded.AddListener(OnPlayerAdded);
		UNetServer.OnPlayerRemoved.AddListener(OnPlayerRemoved);

		StartCoroutine(ReadyForPlayers());
		StartCoroutine(ShutdownServerInXTime());
	}

	IEnumerator ShutdownServerInXTime()
	{
		yield return new WaitForSeconds(300f);
		StartShutdownProcess();
	}

	IEnumerator ReadyForPlayers()
	{
		yield return new WaitForSeconds(.5f);
		PlayFabMultiplayerAgentAPI.ReadyForPlayers();
	}

	private void OnServerActive()
	{
		UNetServer.StartServer();
		Debug.Log("Server Started From Agent Activation");
	}

	private void OnPlayerRemoved(string playfabId)
	{
		ConnectedPlayer player = _connectedPlayers.Find(x => x.PlayerId.Equals(playfabId, StringComparison.OrdinalIgnoreCase));
		_connectedPlayers.Remove(player);
		PlayFabMultiplayerAgentAPI.UpdateConnectedPlayers(_connectedPlayers);
		CheckPlayerCountToShutdown();
	}

	private void CheckPlayerCountToShutdown()
	{
		if (_connectedPlayers.Count <= 0)
		{
			StartShutdownProcess();
		}
	}

	private void OnPlayerAdded(string playfabId)
	{
		_connectedPlayers.Add(new ConnectedPlayer(playfabId));
		PlayFabMultiplayerAgentAPI.UpdateConnectedPlayers(_connectedPlayers);
	}

	private void OnAgentError(string error)
	{
		Debug.Log(error);
	}

	private void OnShutdown()
	{
		StartShutdownProcess();
	}

	private void StartShutdownProcess()
	{
		Debug.Log("Server is shutting down");
		foreach (var conn in UNetServer.Connections)
		{
			conn.Connection.Send(CustomGameServerMessageTypes.ShutdownMessage, new ShutdownMessage());
		}
		StartCoroutine(ShutdownServer());
	}

	IEnumerator ShutdownServer()
	{
		yield return new WaitForSeconds(5f);
		Application.Quit();
	}

	private void OnMaintenance(DateTime? NextScheduledMaintenanceUtc)
	{
		Debug.LogFormat("Maintenance scheduled for: {0}", NextScheduledMaintenanceUtc.Value.ToLongDateString());
		foreach (var conn in UNetServer.Connections)
		{
			conn.Connection.Send(CustomGameServerMessageTypes.ShutdownMessage, new MaintenanceMessage()
			{
				ScheduledMaintenanceUTC = (DateTime)NextScheduledMaintenanceUtc
			});
		}
	}
}
