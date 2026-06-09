namespace Game.Scripts.Player.Interactions
{
    public interface IInteractable
    {
        bool CanInteract { get; }
        
        bool ServerInteract(PlayerMainDataComponents player);
        bool ClientInteractPrediction(PlayerMainDataComponents player);
        void CancelInteractPrediction(PlayerMainDataComponents player);
    }
}