using System;
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
        private PlayerMainDataComponents _player;
        
        private float _maxDistance;

        [Inject]
        private void Construct(
            IPlayerControls playerControls)
        {
            _playerControls = playerControls;
            _player = GetComponent<PlayerMainDataComponents>();

            _maxDistance = _data.InteractionDistance;
        }

        public override void OnStartClient()
        {
            if (isOwned)
                _playerControls.Interacted += OnInteract;
        }

        private void OnDestroy()
        {
            if (isOwned)
                _playerControls.Interacted -= OnInteract;
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
                    interactable.ServerInteract(_player);
                }
                else
                {
                    if (!interactable.ClientInteractPrediction(_player))
                        return;
                    
                    CmdInteractServer(new PlayerInteractionCommand(
                        netId,
                        id,
                        _camera.forward,
                        PlayerInteractionCommandType.ToServer));
                }
            });
        }

        [Command(requiresAuthority = false)]
        private void CmdInteractServer(PlayerInteractionCommand command)
        {
            command.PlayerId = netId;
            var item = NetworkObjectResolver.Resolve<NetworkIdentity>(command.ItemId);

            var cancelCommand = command;
            cancelCommand.Type = PlayerInteractionCommandType.Cancellation;
            
            if (item == null)
            {
                Debug.LogError($"Cannot interact. Cannot find item with id {command.ItemId}");
                CancelInteractionTargetRpc(cancelCommand);
            }
            
            bool interacted = TryInteractLocal(command.LookDirection, (id, interactable) =>
            {
                if (command.ItemId != id)
                {
                    CancelInteractionTargetRpc(cancelCommand);
                    return;
                }

                if (!interactable.ServerInteract(_player))
                {
                    CancelInteractionTargetRpc(cancelCommand);
                }
            });

            if (!interacted)
                CancelInteractionTargetRpc(cancelCommand);
        }

        [TargetRpc]
        private void CancelInteractionTargetRpc(PlayerInteractionCommand command)
        {
            var networkIdentity = NetworkObjectResolver.ResolveOrException<NetworkIdentity>(command.ItemId);
            var interactable = networkIdentity.GetComponent<IInteractable>();
            
            interactable.CancelInteractPrediction(_player);
            networkIdentity.transform.SetPositionAndRotation(command.Position, command.Rotation);
        }
    }
}