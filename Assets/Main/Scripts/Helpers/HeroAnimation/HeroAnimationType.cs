namespace Main.Scripts.Helpers.HeroAnimation
{
public enum HeroAnimationType
{
    None, //None when animation was canceled (for example: stun on casting)
    PrimaryCast,
    PrimaryExecute,
    SecondaryCast,
    SecondaryExecute,
    FirstCast,
    FirstExecute,
    SecondCast,
    SecondExecute,
    ThirdCast,
    ThirdExecute,
}
}