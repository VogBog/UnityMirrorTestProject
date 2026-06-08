using System.Collections;
using Mirror;
using UnityEngine;

namespace Game.Scripts.Player.Network
{
    public class PlayerNetworkResolver : NetworkBehaviour
    {
        [SerializeField] private PlayerEyes _eyes;
        [SerializeField] private Component[] _ownerComponents;

        private IEnumerator Start()
        {
            _eyes.gameObject.SetActive(false);
            
            yield return null;

            if (isOwned)
            {
                _eyes.gameObject.SetActive(true);
                yield break;
            }

            foreach (var component in _ownerComponents)
            {
                Destroy(component);
            }

            yield return null;
            
            _eyes.Resolve();
            _eyes.gameObject.SetActive(true);
            
            Destroy(this);
        }
    }
}