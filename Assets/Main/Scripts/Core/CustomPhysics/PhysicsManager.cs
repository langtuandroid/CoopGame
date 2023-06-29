using UnityEngine;
using Assert = UnityEngine.Assertions.Assert;

namespace Main.Scripts.Core.CustomPhysics
{
    public class PhysicsManager : MonoBehaviour
    {
        public static PhysicsManager? Instance { get; private set; }

        public static float DeltaTime;

        [SerializeField]
        private int stepsByTick = 1;

        public int StepsByTick => stepsByTick;

        private void Awake()
        {
            Assert.IsNull(Instance);

            Instance = this;

            Physics.simulationMode = SimulationMode.Script;
        }

        private void OnDestroy()
        {
            Instance = null;
        }

        public void Init(int tickRate)
        {
            DeltaTime = 1f / (tickRate * stepsByTick);
        }

        public void Simulate()
        {
            Physics.Simulate(DeltaTime);
        }
    }
}