namespace Main.Scripts.Core.Mvp
{
    public interface MvpContract
    {
        interface Presenter { }

        interface View<T> where T : Presenter { }
    }
}