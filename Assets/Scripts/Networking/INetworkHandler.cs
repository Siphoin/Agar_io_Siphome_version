using Unity.Netcode;
using UnityEngine;

namespace AGarIOSiphome.Networking
{
    public interface INetworkHandler
    {
        bool ConnectToHost(string ipAddress, string playerName);
        bool StartHost(string ipAddress);
        void SpawnNetworkObject(GameObject gameObject, Vector3 position = default, Quaternion rotation = default);
        void DespawnNetworkObject(GameObject gameObject);
        void Disconnect();

        public bool IsConnected { get; }
        public bool IsHost { get; }
        public bool IsClient { get; }
        public string CurrentPlayerName { get; }
    }
}