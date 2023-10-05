namespace Main.Scripts.Mobs.Component.Delegate.Condition
{
    public interface ConditionDelegate
    {
        public bool Check(ref MobBlockContext context);
        public void Reset();
    }
}