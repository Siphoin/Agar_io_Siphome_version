using AgarIOSiphome.Networking.Configs;
using System;
using UniRx;
using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace AGarIOSiphome.Networking.Handlers
{
    public class NetworkSpawnHandler : NetworkBehaviour, INetworkObjectSpawnHandler
    {

        private NetworkManager _networkManager;
        private readonly Subject<(ulong, GameObject)> _onObjectSpawned = new();
        private readonly Subject<GameObject> _onObjectDestroyed = new();

        public IObservable<(ulong requestId, GameObject spawnedObject)> OnObjectSpawned => _onObjectSpawned;
        public IObservable<GameObject> OnObjectDestroyed => _onObjectDestroyed;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            _networkManager = NetworkManager.Singleton;
        }

        public GameObject SpawnNetworkObject(
            GameObject prefab,
            Action<GameObject> callback = null,
            bool spawnWithOwnership = true,
            Vector3 position = default,
            Quaternion rotation = default)
        {
            if (!IsSpawned || prefab == null)
            {
                callback?.Invoke(null);
                return null;
            }

            ulong requestId = GenerateRequestId();
            ulong ownerClientId = spawnWithOwnership ? NetworkManager.LocalClientId : 0;

            if (IsServer)
            {
                var spawnedObject = SpawnObjectDirect(prefab, position, rotation, spawnWithOwnership, ownerClientId);
                callback?.Invoke(spawnedObject);
                NotifyClientSpawnedClientRpc(requestId, spawnedObject.GetComponent<NetworkObject>().NetworkObjectId);
                return spawnedObject;
            }
            else
            {
                RegisterCallback(requestId, callback);
                RequestSpawnServerRpc(
                    requestId,
                    GetPrefabName(prefab),
                    position,
                    rotation,
                    spawnWithOwnership,
                    ownerClientId
                );
                return null;
            }
        }

        public void DestroyNetworkObject(GameObject gameObject, Action callback = null)
        {
            if (!IsSpawned || gameObject == null)
            {
                callback?.Invoke();
                return;
            }

            var networkObject = gameObject.GetComponent<NetworkObject>();
            if (networkObject == null)
            {
                Debug.LogError("GameObject doesn't have NetworkObject component!");
                callback?.Invoke();
                return;
            }

            if (IsServer)
            {
                DestroyObjectDirect(gameObject);
                callback?.Invoke();
                NotifyClientDestroyedClientRpc(networkObject.NetworkObjectId);
            }
            else
            {
                RegisterDestroyCallback(networkObject.NetworkObjectId, callback);
                RequestDestroyServerRpc(networkObject.NetworkObjectId);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void RequestSpawnServerRpc(
            ulong requestId,
            string prefabName,
            Vector3 position,
            Quaternion rotation,
            bool spawnWithOwnership,
            ulong ownerClientId,
            ServerRpcParams rpcParams = default)
        {
            var prefab = FindPrefabByName(prefabName);
            if (prefab == null)
            {
                Debug.LogError($"Prefab with name '{prefabName}' not found!");
                return;
            }

            var spawnedObject = SpawnObjectDirect(prefab, position, rotation, spawnWithOwnership, ownerClientId);
            NotifyClientSpawnedClientRpc(requestId, spawnedObject.GetComponent<NetworkObject>().NetworkObjectId);
        }

        [ClientRpc]
        private void NotifyClientSpawnedClientRpc(ulong requestId, ulong spawnedObjectId)
        {
            if (_networkManager.SpawnManager.SpawnedObjects.TryGetValue(spawnedObjectId, out var netObj))
            {
                _onObjectSpawned.OnNext((requestId, netObj.gameObject));
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void RequestDestroyServerRpc(ulong networkObjectId, ServerRpcParams rpcParams = default)
        {
            if (_networkManager.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out var netObj))
            {
                DestroyObjectDirect(netObj.gameObject);
                NotifyClientDestroyedClientRpc(networkObjectId);
            }
        }

        [ClientRpc]
        private void NotifyClientDestroyedClientRpc(ulong destroyedObjectId)
        {
            if (_networkManager.SpawnManager.SpawnedObjects.TryGetValue(destroyedObjectId, out var netObj))
            {
                _onObjectDestroyed.OnNext(netObj.gameObject);
            }
        }

        private GameObject SpawnObjectDirect(
            GameObject prefab,
            Vector3 position,
            Quaternion rotation,
            bool spawnWithOwnership,
            ulong ownerClientId)
        {
            GameObject spawnedObject = Instantiate(prefab, position, rotation);
            NetworkObject networkObject = spawnedObject.GetComponent<NetworkObject>();

            if (spawnWithOwnership)
                networkObject.SpawnWithOwnership(ownerClientId);
            else
                networkObject.Spawn();

            return spawnedObject;
        }

        private void DestroyObjectDirect(GameObject gameObject)
        {
            NetworkObject networkObject = gameObject.GetComponent<NetworkObject>();
            if (networkObject != null)
            {
                networkObject.Despawn();
            }
            Destroy(gameObject);
        }

        private ulong GenerateRequestId()
        {
            return (ulong)DateTime.Now.Ticks;
        }

        private string GetPrefabName(GameObject prefab)
        {
            return prefab.name;
        }

        private GameObject FindPrefabByName(string prefabName)
        {
            foreach (var networkPrefab in _networkManager.NetworkConfig.Prefabs.Prefabs)
            {
                if (networkPrefab.Prefab.name == prefabName)
                    return networkPrefab.Prefab;
            }

            return null;
        }

        private void RegisterCallback(ulong requestId, Action<GameObject> callback)
        {
            if (callback == null) return;

            _onObjectSpawned
                .Where(x => x.Item1 == requestId)
                .Take(1)
                .Subscribe(x => callback(x.Item2));
        }

        private void RegisterDestroyCallback(ulong networkObjectId, Action callback)
        {
            if (callback == null) return;

            _onObjectDestroyed
                .Where(x => x.GetComponent<NetworkObject>().NetworkObjectId == networkObjectId)
                .Take(1)
                .Subscribe(_ => callback());
        }

        // Упрощенный метод для спавна с владельцем-локалкой
        public GameObject SpawnAsLocalOwner(GameObject prefab, Vector3 position = default, Quaternion rotation = default)
        {
            return SpawnNetworkObject(prefab, null, true, position, rotation);
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            _onObjectSpawned?.Dispose();
            _onObjectDestroyed?.Dispose();
        }
    }
}