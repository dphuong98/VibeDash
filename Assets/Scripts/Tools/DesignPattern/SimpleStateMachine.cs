namespace _Main.Game.Interfaces
{
    public interface SimpleStateMachine<T>
    {
        T CurrentState { get; }
    }
}