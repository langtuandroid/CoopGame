using Main.Scripts.Utils;
using Photon.Bolt;
using Photon.Bolt.Utils;
using UdpKit;
using UnityEngine;

namespace Main.Scripts.Callbacks
{
	[BoltGlobalBehaviour(BoltNetworkModes.Server, "Level1")]
	public class ServerCallbacks : GlobalEventListener
	{
		public static bool ListenServer = true;
		private float lastEnemiesSpawnTime;
		private float enemiesSpawnTimeDelay = 0.1f;

		void Awake()
		{
			if (ListenServer)
			{
				Player.PlayerInfo.CreateServerPlayer();
				Player.PlayerInfo.ServerPlayerInfo.name = "SERVER";
			}
		}

		void FixedUpdate()
		{
			foreach (Player.PlayerInfo p in Player.PlayerInfo.allPlayers)
			{
				// if we have an entity, it's dead but our spawn frame has passed
				if (p.entity && p.state.Dead && p.state.respawnFrame <= BoltNetwork.ServerFrame)
				{
					p.Spawn();
				}
			}
			if (Input.GetKey(KeyCode.T) && (Time.time -  lastEnemiesSpawnTime > enemiesSpawnTimeDelay))
			{
				lastEnemiesSpawnTime = Time.time;
				// for (var i = 0; i < 5; i++)
				// {
				BoltNetwork.Instantiate(BoltPrefabs.EnemyManGreatSword, RandomSpawn(), Quaternion.identity);
				// }
			}
		}
		
		static Vector3 RandomSpawn()
		{
			float x = Random.Range(-32f, +32f);
			float z = Random.Range(-32f, +32f);
			return new Vector3(x, 2f, z);
		}

		public override void ConnectRequest(UdpEndPoint endpoint, IProtocolToken token)
		{
			BoltLog.Warn("ConnectRequest");

			if (token != null)
			{
				BoltLog.Info("Token Received");
			}

			BoltNetwork.Accept(endpoint);
		}

		public override void ConnectAttempt(UdpEndPoint endpoint, IProtocolToken token)
		{
			BoltLog.Warn("ConnectAttempt");
			base.ConnectAttempt(endpoint, token);
		}

		public override void Disconnected(BoltConnection connection)
		{
			BoltLog.Warn("Disconnected");
			base.Disconnected(connection);
		}

		public override void ConnectRefused(UdpEndPoint endpoint, IProtocolToken token)
		{
			BoltLog.Warn("ConnectRefused");
			base.ConnectRefused(endpoint, token);
		}

		public override void ConnectFailed(UdpEndPoint endpoint, IProtocolToken token)
		{
			BoltLog.Warn("ConnectFailed");
			base.ConnectFailed(endpoint, token);
		}

		public override void Connected(BoltConnection connection)
		{
			BoltLog.Warn("Connected");

			connection.UserData = new Player.PlayerInfo();
			connection.GetPlayer().connection = connection;
			connection.GetPlayer().name = "CLIENT:" + connection.RemoteEndPoint.Port;

			connection.SetStreamBandwidth(1024 * 1024);
		}

		public override void SceneLoadRemoteDone(BoltConnection connection, IProtocolToken token)
		{
			connection.GetPlayer().InstantiateEntity();
		}

		public override void SceneLoadLocalDone(string scene, IProtocolToken token)
		{
			if (Player.PlayerInfo.serverIsPlaying)
			{
				Player.PlayerInfo.ServerPlayerInfo.InstantiateEntity();
			}
		}

		public override void SceneLoadLocalBegin(string scene, IProtocolToken token)
		{
			foreach (Player.PlayerInfo p in Player.PlayerInfo.allPlayers)
			{
				p.entity = null;
			}
		}
	}
}
