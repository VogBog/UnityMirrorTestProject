namespace Game.Scripts.GameSystems.GameStarting
{
    public interface IGameStartPipelineStep
    {
        bool Completed { get; }
        void StartStep();
    }
}