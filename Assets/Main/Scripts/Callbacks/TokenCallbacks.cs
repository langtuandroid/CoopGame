using Main.Scripts.Tokens;
using Photon.Bolt;

namespace Main.Scripts.Callbacks {

	[BoltGlobalBehaviour]
	public class TokenCallbacks : GlobalEventListener {
	  public override void BoltStartBegin() {
	    BoltNetwork.RegisterTokenClass<TestToken>();
	  }
	}
}
