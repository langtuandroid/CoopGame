namespace Main.Scripts.Core.GameLogic
{
    public interface GameLoopListener
    {
        void OnBeforePhysicsSteps();
        void OnBeforePhysicsStep();
        void OnAfterPhysicsStep();
        void OnAfterPhysicsSteps();
    }
}