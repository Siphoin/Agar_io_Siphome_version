using AGarIOSiphome.Networking;
using System;
using UniRx;
using UnityEngine;
using Zenject;

namespace AgarIOSiphome.Core.Player.Factories
{
    public class PlayerFactory : IPlayerFactory
    {
        [Inject] private INetworkHandler _networkHandler;
        private Subject<IPlayerInstance> _onSpawn = new();

        public IObservable<IPlayerInstance> OnSpawn => _onSpawn;

        public IPlayerInstance Create(PlayerInstance prefab, Vector3 position, Quaternion rotation, Transform parent = null)
        {
            var newObject = _networkHandler.SpawnNetworkObject(prefab.gameObject, position, rotation);

            if (newObject != null)
            {
                _onSpawn.OnNext(newObject.GetComponent<PlayerInstance>());
            }

            return newObject.GetComponent<PlayerInstance>();
        }
    }
}
