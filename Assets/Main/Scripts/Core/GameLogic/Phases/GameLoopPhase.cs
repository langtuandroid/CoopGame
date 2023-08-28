namespace Main.Scripts.Core.GameLogic.Phases
{
    public enum GameLoopPhase
    {
        SyncTransformBeforeAllPhase,
        EffectsRemoveFinishedPhase,
        PlayerInputPhase,
        StrategyPhase,
        SkillActivationPhase,
        SkillSpawnPhase,
        SkillUpdatePhase,
        EffectsApplyPhase,
        EffectsUpdatePhase,
        ApplyActionsPhase,
        DespawnPhase,
        MovementStrategyPhase,
        PhysicsUpdatePhase,
        PhysicsSkillMovementPhase,
        PhysicsCheckCollisionsPhase,
        PhysicsUnitsLookPhase,
        PhysicsSkillLookPhase,
        AOIUpdatePhase,
        ObjectsSpawnPhase,
        VisualStateUpdatePhase,
        SyncTransformAfterAllPhase
    }
}