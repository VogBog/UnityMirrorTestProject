using System;
using Game.Scripts.Network.EventBus;
using Game.Scripts.Player.Control.Data;
using Game.Scripts.Player.Interactions;
using Game.Scripts.Player.Network;
using Mirror;
using UnityEngine;
using Zenject;

namespace Game.Scripts.Player.Control
{
    [RequireComponent(typeof(PlayerMainDataComponents))]
    public class PlayerInteractor : NetworkBehaviour
    {
        [SerializeField] private PlayerScriptableData _data;
        [SerializeField] private Transform _camera;
        [SerializeField] private LayerMask _mask;
        
        private IPlayerControls _playerControls;
        private INetworkServerPublishing _serverPublishing;
        private IPlayerActionCommandHandler _playerActions;
        private PlayerMainDataComponents _player;
        
        private float _maxDistance;

        [Inject]
        private void Construct(
            IPlayerActionCommandHandler playerActions,
            INetworkServerPublishing serverPublishing,
            IPlayerControls playerControls)
        {
            _playerActions = playerActions;
            _serverPublishing = serverPublishing;
            _playerControls = playerControls;
            _player = GetComponent<PlayerMainDataComponents>();

            _maxDistance = _data.InteractionDistance;
        }

        public override void OnStartClient()
        {
            if (isOwned)
                _playerControls.Interacted += OnInteract;
        }

        private void Awake()
        {
            _playerActions.RegisterHandlerForPlayer(netIdentity, OnReceiveCommand, () => new PlayerInteractionCommand());
        }

        private void OnDestroy()
        {
            if (isOwned)
                _playerControls.Interacted -= OnInteract;
            
            _playerActions.UnregisterHandlerForPlayer<PlayerInteractionCommand>(netIdentity.netId);
        }

        public bool TryInteractLocal(
            Vector3 lookDirection,
            Action<uint, IInteractable> onInteract)
        {
            var ray = new Ray(_camera.position, lookDirection);
            if (Physics.Raycast(ray, out var hit, _maxDistance, _mask) &&
                hit.collider.TryGetComponent(out IInteractable interactable) &&
                interactable.CanInteract)
            {
                uint interactableId = 0;
                if (!hit.collider.TryGetComponent(out NetworkIdentity networkIdentity))
                {
                    Debug.LogWarning(
                        $"Cannot sync interaction because interactable object {hit.collider.gameObject} has no NetworkIdentity",
                        hit.collider.gameObject);
                }
                else
                {
                    interactableId = networkIdentity.netId;
                }
                
                onInteract.Invoke(interactableId, interactable);
                return true;
            }

            return false;
        }
        
        private void OnInteract()
        {
            if (!enabled || !isOwned)
                return;

            TryInteractLocal(_camera.forward, (id, interactable) =>
            {
                if (isServer)
                {
                    if (!interactable.ServerInteract(_player))
                        return;
                    
                    _serverPublishing.SendToPlayersExcludeServer(PlayerActionCommand.CreateForPlayer(
                        netId,
                        new PlayerInteractionCommand(
                            netId,
                            id,
                            _camera.forward,
                            PlayerInteractionCommandType.Confirm)));
                }
                else
                {
                    if (!interactable.ClientInteractPrediction(_player))
                        return;
                    
                    NetworkClient.Send(PlayerActionCommand.CreateForPlayer(
                        netId,
                        new PlayerInteractionCommand(
                            netId,
                            id,
                            _camera.forward,
                            PlayerInteractionCommandType.ToServer)));
                }
            });
        }

        private void OnReceiveCommand(PlayerInteractionCommand command)
        {
            if (!enabled || command.PlayerId != netId)
                return;
            
            switch (command.Type)
            {
                case PlayerInteractionCommandType.ToServer:
                    InteractServerCommand(command);
                    break;
                
                case PlayerInteractionCommandType.Cancellation:
                    InteractCancellationCommand(command);
                    break;
                
                case PlayerInteractionCommandType.Confirm:
                    ConfirmInteractCommand(command);
                    break;
            }
        }

        private void InteractServerCommand(PlayerInteractionCommand command)
        {
            var item = NetworkObjectResolver.Resolve<NetworkIdentity>(command.ItemId);
            
            var cancelCommand = PlayerActionCommand.CreateForPlayer(
                netId,
                new PlayerInteractionCommand(
                    netId,
                    command.ItemId,
                    command.LookDirection,
                    item?.transform.position ?? Vector3.zero,
                    item?.transform.rotation ?? Quaternion.identity));
            
            if (item == null)
            {
                Debug.LogError($"Cannot interact. Cannot find item with id {command.ItemId}");
                _serverPublishing.SendToTargetPlayer(cancelCommand, command.PlayerId);
            }
            
            bool interacted = TryInteractLocal(command.LookDirection, (id, interactable) =>
            {
                if (command.ItemId != id)
                {
                    _serverPublishing.SendToTargetPlayer(cancelCommand, command.PlayerId);
                    return;
                }

                if (!interactable.ServerInteract(_player))
                {
                    _serverPublishing.SendToTargetPlayer(cancelCommand, command.PlayerId);
                    return;
                }

                _serverPublishing.SendToPlayersExcludeServer(PlayerActionCommand.CreateForPlayer(
                    netId,
                    new PlayerInteractionCommand(
                        netId,
                        id,
                        command.LookDirection,
                        PlayerInteractionCommandType.Confirm)));
            });

            if (!interacted)
                _serverPublishing.SendToTargetPlayer(cancelCommand, command.PlayerId);
        }

        private void InteractCancellationCommand(PlayerInteractionCommand command)
        {
            var networkIdentity = NetworkObjectResolver.ResolveOrException<NetworkIdentity>(command.ItemId);
            var interactable = networkIdentity.GetComponent<IInteractable>();
            
            interactable.CancelInteractPrediction(_player);
            networkIdentity.transform.SetPositionAndRotation(command.Position, command.Rotation);
        }

        private void ConfirmInteractCommand(PlayerInteractionCommand command)
        {
            if (isServer)
                return;
            
            var interactable = NetworkObjectResolver.ResolveOrException<IInteractable>(command.ItemId);
            interactable.ClientConfirmInteraction(_player, isOwned);
        }
    }
}