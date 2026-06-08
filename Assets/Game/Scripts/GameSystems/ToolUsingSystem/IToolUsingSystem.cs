using Game.Scripts.GameSystems.ToolUsingSystem.Data;

namespace Game.Scripts.GameSystems.ToolUsingSystem
{
    public interface IToolUsingSystem
    {
        bool IsPlayerUsingTool(uint playerId);
        
        bool TryStartUsingTool(StartUsingToolCommand command);
        bool StopUsingTool(uint playerId, UsingToolCommandType type = UsingToolCommandType.ToServer);
        void StopUsingToolCauseOfCancel(StartUsingToolCommand cause);
    }
}