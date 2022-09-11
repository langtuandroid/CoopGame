using System;
using System.Collections.Generic;
using System.Linq;
using Main.Scripts.Tokens;
using Photon.Bolt;
using UnityEngine;
using UE = UnityEngine;

namespace Main.Scripts.Player
{

	public partial class PlayerInfo : IDisposable
	{
		public const byte TEAM_RED = 1;
		public const byte TEAM_BLUE = 2;

		public string name;
		public BoltEntity entity;
		public BoltConnection connection;

		public IBobState state
		{
			get { return entity.GetState<IBobState>(); }
		}

		public bool isServer
		{
			get { return connection == null; }
		}

		public PlayerInfo()
		{
			players.Add(this);
		}

		public void Kill()
		{
			if (entity)
			{
				state.Dead = true;
				// state.respawnFrame = BoltNetwork.ServerFrame + (15 * BoltNetwork.FramesPerSecond);
			}
		}

		internal void Spawn()
		{
			if (entity)
			{
				state.Dead = false;
				state.health = 100;

				// teleport
				entity.transform.position = RandomSpawn();
			}
		}

		public void Dispose()
		{
			players.Remove(this);

			// destroy
			if (entity)
			{
				BoltNetwork.Destroy(entity.gameObject);
			}

			// while we have a team difference of more then 1 player
			while (Mathf.Abs(redPlayers.Count() - bluePlayers.Count()) > 1)
			{
				if (redPlayers.Count() < bluePlayers.Count())
				{
					var player = bluePlayers.First();
					player.Kill();
					player.state.team = TEAM_RED;
				}
				else
				{
					var player = redPlayers.First();
					player.Kill();
					player.state.team = TEAM_BLUE;
				}
			}
		}

		public void InstantiateEntity()
		{
			entity = BoltNetwork.Instantiate(BoltPrefabs.ManGreatSword, new TestToken(), RandomSpawn(), Quaternion.identity);

			state.name = name;
			state.team = redPlayers.Count() >= bluePlayers.Count() ? TEAM_BLUE : TEAM_RED;

			if (isServer)
			{
				entity.TakeControl(new TestToken());
			}
			else
			{
				entity.AssignControl(connection, new TestToken());
			}

			Spawn();
		}
	}

	partial class PlayerInfo
	{
		static List<PlayerInfo> players = new List<PlayerInfo>();

		public static IEnumerable<PlayerInfo> redPlayers
		{
			get { return players.Where(x => x.entity && x.state.team == TEAM_RED); }
		}

		public static IEnumerable<PlayerInfo> bluePlayers
		{
			get { return players.Where(x => x.entity && x.state.team == TEAM_BLUE); }
		}

		public static IEnumerable<PlayerInfo> allPlayers
		{
			get { return players; }
		}

		public static bool serverIsPlaying
		{
			get { return ServerPlayerInfo != null; }
		}

		public static PlayerInfo ServerPlayerInfo
		{
			get;
			private set;
		}

		public static void CreateServerPlayer()
		{
			ServerPlayerInfo = new PlayerInfo();
		}

		static Vector3 RandomSpawn()
		{
			float x = UE.Random.Range(-5f, +5f);
			float z = UE.Random.Range(-5f, +5f);
			return new Vector3(x, 3f, z);
		}

	}

}
