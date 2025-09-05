using AgarIOSiphome.Core.Configs;
using AgarIOSiphome.Core.Player.Factories;
using AGarIOSiphome.Networking;
using AGarIOSiphome.Networking.Handlers;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;
using Zenject;

namespace AgarIOSiphome.Core.Handlers
{
    public class PlayerSpawnHandler : MonoBehaviour
    {
        [Inject] private IPlayerFactory _playerFactory;
        [Inject] private INetworkHandler _networkHandler;
        [Inject] private PlayerSpawnHandlerConfig _playerSpawnHandlerConfig;
        private void Awake()
        {
            _networkHandler.OnConnected.Subscribe(async _ =>
            {
                await UniTask.WaitUntil(() => _networkHandler.SpawnHandler != null, cancellationToken: this.GetCancellationTokenOnDestroy());
                _playerFactory.Create(_playerSpawnHandlerConfig.PlayerPrefab, Vector3.zero, Quaternion.identity);

            }).AddTo(this);
        }
    }
}