using System;
using UnityEngine;

namespace AGarIOSiphome.Networking.Handlers
{
    public interface INetworkObjectSpawnHandler
    {
        IObservable<(ulong requestId, GameObject spawnedObject)> OnObjectSpawned { get; }
        IObservable<GameObject> OnObjectDestroyed { get; }

        void DestroyNetworkObject(GameObject gameObject, Action callback = null);
        GameObject SpawnAsLocalOwner(GameObject gameObject, Vector3 position = default, Quaternion rotation = default);
    }
}