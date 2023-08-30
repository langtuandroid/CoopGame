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
        PhysicsUpdatePhase,
        PhysicsSkillMovementPhase,
        PhysicsCheckCollisionsPhase,
        PhysicsUnitsLookPhase,
        PhysicsSkillLookPhase,
        NavigationPhase,
        AOIUpdatePhase,
        ObjectsSpawnPhase,
        VisualStateUpdatePhase,
        SyncTransformAfterAllPhase,
        LevelStrategyPhase,
    }
}