using AgarIOSiphome.Core.Player;
using AgarIOSiphome.System.Configs;
using UnityEngine;
namespace AgarIOSiphome.Core.Configs
{
    [CreateAssetMenu(menuName = "Core/Configs/Player Spawn Handler Config")]
    public class PlayerSpawnHandlerConfig : ScriptableConfig
    {
        [SerializeField] private PlayerInstance _playerPrefab;

        public PlayerInstance PlayerPrefab => _playerPrefab;
    }
}
