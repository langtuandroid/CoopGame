using UnityEngine;
using System.Collections;
using System.Text;
using Photon.Bolt;
using Photon.Bolt.Utils;

namespace Bolt.AdvancedTutorial
{
	[BoltGlobalBehaviour("Level1")]
	public class PlayerCallbacks : GlobalEventListener
	{
		public override void SceneLoadLocalBegin(string scene, IProtocolToken token)
		{
			BoltLog.Info("SceneLoadLocalBegin-Token: {0}", token);
			// ui
			GameUI.Instantiate();

			// camera
			PlayerCamera.Instantiate();
		}

		public override void SceneLoadLocalDone(string scene, IProtocolToken token)
		{
			BoltLog.Info("SceneLoadLocalDone-Token: {0}", token);
		}

		public override void SceneLoadRemoteDone(BoltConnection connection, IProtocolToken token)
		{
			BoltLog.Info("SceneLoadRemoteDone-Token: {0}", token);
		}

		public override void ControlOfEntityGained(BoltEntity entity)
		{
			if (entity.gameObject.GetComponent<BobController>() == null)
			{
				//ignore not player entities
				return;
			}
			
			// add audio listener to our character
			entity.gameObject.AddComponent<AudioListener>();

			PlayerCamera.instance.getHealth = () => entity.GetState<IPlayerState>().health;

			// set camera target
			PlayerCamera.instance.SetTarget(entity);
		}
	}
}
