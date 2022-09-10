using Photon.Bolt;

namespace Main.Scripts.Utils
{
	public static class ExtensionMethods
	{
		public static Player.PlayerInfo GetPlayer (this BoltConnection connection)
		{
			if (connection == null) {
				return Player.PlayerInfo.ServerPlayerInfo;
			}

			return (Player.PlayerInfo)connection.UserData;
		}
	}
}
