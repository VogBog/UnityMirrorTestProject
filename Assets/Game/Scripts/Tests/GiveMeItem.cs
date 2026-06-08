using System.Collections;
using Game.Scripts.GameSystems.Factories;
using Game.Scripts.GameSystems.Factories.Data;
using Game.Scripts.GameSystems.Repositories;
using Game.Scripts.Player.Inventory.Items;
using Mirror;
using UnityEngine;
using Zenject;

namespace Game.Scripts.Tests
{
    public class GiveMeItem : NetworkBehaviour
    {
        [SerializeField] private BaseItem _prefab;

        private INetworkObjectFactory _factory;
        private IPlayerRepository _playerRepository;

        [Inject]
        private void Construct(INetworkObjectFactory factory, IPlayerRepository playerRepository)
        {
            _factory = factory;
            _playerRepository = playerRepository;
        }

        public override void OnStartServer()
        {
            StartCoroutine(GiveItemRoutine());
        }

        private IEnumerator GiveItemRoutine()
        {
            yield return null;
            _factory.InstantiateAndSpawnServer(new InstantiateAndSpawnCommand(
                _prefab.gameObject,
                new Vector3(0f, 5f, 0f),
                Quaternion.identity));
        }
    }
}