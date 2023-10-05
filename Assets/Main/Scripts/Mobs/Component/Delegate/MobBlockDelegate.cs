namespace Main.Scripts.Mobs.Component.Delegate
{
    public interface MobBlockDelegate
    {
        public void Do(ref MobBlockContext context, out MobBlockResult blockResult);
        public void Reset();
    }
}