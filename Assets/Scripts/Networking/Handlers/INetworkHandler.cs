using System;
using UniRx;
using Unity.Netcode;
using UnityEngine;

namespace AGarIOSiphome.Networking.Handlers
{
    public interface INetworkHandler
    {
        bool ConnectToHost(string ipAddress, string playerName);
        bool StartHost(string ipAddress);
        GameObject SpawnNetworkObject(GameObject gameObject, Vector3 position = default, Quaternion rotation = default);
        void DespawnNetworkObject(GameObject gameObject);
        void Disconnect();

        public bool IsConnected { get; }
        public bool IsHost { get; }
        public bool IsClient { get; }
        public string CurrentPlayerName { get; }

        IObservable<Unit> OnConnected { get; }
        IObservable<Unit> OnDisconnected { get; }
        IObservable<string> OnConnectionError { get; }
        INetworkObjectSpawnHandler SpawnHandler { get; }
    }
}