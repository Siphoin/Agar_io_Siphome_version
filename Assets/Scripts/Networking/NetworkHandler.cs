using AgarIOSiphome.Networking.Configs;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using Zenject;

namespace AGarIOSiphome.Networking
{

    public class NetworkHandler : MonoBehaviour, INetworkHandler
    {
        [Inject] private NetworkHandlerConfig _config;

        private NetworkManager _networkManager;
        private string _currentPlayerName;

        public bool IsConnected => _networkManager.IsListening;
        public bool IsHost => _networkManager.IsHost;
        public bool IsClient => _networkManager.IsClient;
        public string CurrentPlayerName => _currentPlayerName;

        private void Start()
        {
            if (Application.isBatchMode)
            {
                StartHost("127.0.0.1");
            }

            else
            {

                ConnectToHost("127.0.0.1", string.Empty);
            }
        }

        private void SetupNetworkManager(string ipAddress)
        {
            CreateNetwoorkManager(ipAddress);
            SubscribeToNetworkEvents();
        }

        private void CreateNetwoorkManager(string ipAddress)
        {
            if (_networkManager != null)
            {
                Destroy(_networkManager.gameObject);
            }
            _networkManager = Instantiate(_config.PrefabNetworkManager);
            _networkManager.GetComponent<UnityTransport>().SetConnectionData(ipAddress, _config.Port);
        }

        private void OnDestroy()
        {
            UnsubscribeFromNetworkEvents();
        }

        public bool ConnectToHost(string ipAddress, string playerName)
        {
            SetupNetworkManager(ipAddress);
            if (_networkManager.IsListening)
            {
                Debug.LogWarning("Network manager is already running");
                return false;
            }

            if (string.IsNullOrEmpty(ipAddress))
            {
                Debug.LogError("IP address cannot be null or empty");
                return false;
            }

            _currentPlayerName = string.IsNullOrEmpty(playerName) ? _config.DefaultPlayerName : playerName;

            return _networkManager.StartClient();
        }

        public bool StartHost(string ipAddress)
        {
            SetupNetworkManager(ipAddress);
            if (_networkManager.IsListening)
            {
                Debug.LogWarning("Network manager is already running");
                return false;
            }

            return _networkManager.StartHost();
        }
        public void SpawnNetworkObject(NetworkObject networkObject, Vector3 position, Quaternion rotation)
        {
            if (networkObject is null)
            {
                Debug.LogError("NetworkObject cannot be null");
                return;
            }

            if (!_networkManager.IsListening)
            {
                Debug.LogError("Network is not running");
                return;
            }

            var instance = Instantiate(networkObject, position, rotation);
            instance.Spawn();
        }

        public void DespawnNetworkObject(NetworkObject networkObject)
        {
            if (networkObject is null)
            {
                Debug.LogError("NetworkObject cannot be null");
                return;
            }

            if (!networkObject.IsSpawned)
            {
                Debug.LogWarning("NetworkObject is not spawned");
                return;
            }

            networkObject.Despawn();
        }

        public void Disconnect()
        {
           
            if (_networkManager != null && _networkManager.IsListening)
            {
                UnsubscribeFromNetworkEvents();
                Destroy(_networkManager.gameObject);
            }
        }

        private void SubscribeToNetworkEvents()
        {
            _networkManager.OnClientConnectedCallback += OnClientConnected;
            _networkManager.OnClientDisconnectCallback += OnClientDisconnected;
            _networkManager.OnServerStarted += OnServerStarted;
        }

        private void UnsubscribeFromNetworkEvents()
        {
            if (_networkManager is null)
            {
                return;
            }

            _networkManager.OnClientConnectedCallback -= OnClientConnected;
            _networkManager.OnClientDisconnectCallback -= OnClientDisconnected;
            _networkManager.OnServerStarted -= OnServerStarted;
        }

        private void OnClientConnected(ulong clientId)
        {
            Debug.Log($"Client connected: {clientId}");

            if (_networkManager.IsServer)
            {
                Debug.Log($"New client connected to server: {clientId}");
            }
        }

        private void OnClientDisconnected(ulong clientId)
        {
            Debug.Log($"Client disconnected: {clientId}");
        }

        private void OnServerStarted()
        {
            Debug.Log("Server started successfully");
        }
    }
}