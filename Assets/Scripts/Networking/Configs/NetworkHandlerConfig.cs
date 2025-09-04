using AgarIOSiphome.System.Configs;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace AgarIOSiphome.Networking.Configs
{
    [CreateAssetMenu(menuName = "System/Configs/Network/Network Handler Config")]
    public class NetworkHandlerConfig : ScriptableConfig
    {
        [Header("Network Settings")]
        [SerializeField] private string _defaultPlayerName = "Player";
        [SerializeField] private ushort _port = 7777;
        [SerializeField] private NetworkManager _prefabNetworkManager;

        [Header("Network handlers")]
        [SerializeField] private NetworkBehaviour[] _networkHandlersPrefabs;

        public string DefaultPlayerName => _defaultPlayerName;
        public ushort Port => _port;

        public NetworkManager PrefabNetworkManager => _prefabNetworkManager;

        public IEnumerable<NetworkBehaviour> NetworkHandlersPrefabs => _networkHandlersPrefabs;
    }
}
