namespace Main.Scripts.Core.Architecture
{
    public interface InterfacesHolder
    {
        public T? GetInterface<T>();

        public bool TryGetInterface<T>(out T typed);
    }
}