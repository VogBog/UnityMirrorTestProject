namespace Game.Scripts.GameSystems.GameStarting
{
    public interface IGameStarter
    {
        void AddToInitializationQueue(IInitializationWaiter waiter);
    }
}