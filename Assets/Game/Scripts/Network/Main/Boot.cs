using Game.Scripts.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Scripts.Network.Main
{
    public class Boot : MonoBehaviour
    {
        private void Start()
        {
            SceneManager.LoadScene(nameof(Scenes.MenuOffline));
        }
    }
}