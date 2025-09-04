using Unity.Netcode;
using UnityEngine;

namespace AGarIOSiphome.Networking.Helpers
{
    public static class NetworkObjectHelper
    {
        public static void SafeSpawn(INetworkHandler networkManager, NetworkObject prefab,
            Vector3 position, Quaternion rotation)
        {
            if (networkManager is null || prefab is null) return;

            networkManager.SpawnNetworkObject(prefab, position, rotation);
        }

        public static void SafeDespawn(INetworkHandler networkManager, NetworkObject networkObject)
        {
            if (networkManager is null || networkObject is null) return;

            networkManager.DespawnNetworkObject(networkObject);
        }
    }
}