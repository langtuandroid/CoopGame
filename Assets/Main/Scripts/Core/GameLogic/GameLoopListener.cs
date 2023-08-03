namespace Main.Scripts.Core.GameLogic
{
    public interface GameLoopListener
    {
        void OnSyncTransformBeforeAll() { }
        void OnInputPhase() { }
        void OnBeforePhysics() { }
        void OnBeforePhysicsStep() { }
        void OnAfterPhysicsStep() { }
        void OnAfterPhysicsSteps() { }
        void OnSyncTransformAfterAll() { }
    }
}