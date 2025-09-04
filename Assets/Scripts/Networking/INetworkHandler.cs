using Unity.Netcode;
using UnityEngine;

namespace AGarIOSiphome.Networking
{
    public interface INetworkHandler
    {
        bool ConnectToHost(string ipAddress, string playerName);
        bool StartHost();
        void SpawnNetworkObject(NetworkObject networkObject, Vector3 position, Quaternion rotation);
        void DespawnNetworkObject(NetworkObject networkObject);
        void Disconnect();

        public bool IsConnected { get; }
        public bool IsHost { get; }
        public bool IsClient { get; }
        public string CurrentPlayerName { get; }
    }
}