
﻿using AgarIOSiphome.Networking.Configs;
using System;
using UniRx;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using Zenject;

namespace AGarIOSiphome.Networking.Handlers {

    public class NetworkHandler : MonoBehaviour, INetworkHandler
    {
        [Inject] private NetworkHandlerConfig _config;

        private NetworkManager _networkManager;
        private Subject<Unit> _onConnected = new();
        private Subject<Unit> _onDisconnected = new();
        private Subject<string> _onConnectionError = new();
        private string _currentPlayerName;
        private NetworkSpawnHandler _spawnHandler;
        public static string SetedNickName { get; private set; }

        public bool IsConnected => _networkManager != null && _networkManager.IsListening;
        public bool IsHost => _networkManager != null && _networkManager.IsHost;
        public bool IsClient => _networkManager != null && _networkManager.IsClient;
        public string CurrentPlayerName => _currentPlayerName;

        public IObservable<Unit> OnConnected => _onConnected;
        public IObservable<Unit> OnDisconnected => _onDisconnected;
        public IObservable<string> OnConnectionError => _onConnectionError;

        public INetworkObjectSpawnHandler SpawnHandler
        {
            get
            {
                if (_spawnHandler is null)
                {
                    _spawnHandler = FindAnyObjectByType<NetworkSpawnHandler>();
                }
                return _spawnHandler;
            }
        }

        private void Start()
        {
            if (Application.isBatchMode)
            {
                StartHost("127.0.0.1");
            }

            else if (!Application.isBatchMode && !Application.isEditor)
            {
                ConnectToHost("127.0.0.1", string.Empty);
            }

            else if (Application.isEditor && !Application.isBatchMode)
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
                    _onConnectionError.OnNext("IP address cannot be null or empty");
                    return false;
                }

                _currentPlayerName = string.IsNullOrEmpty(playerName) ? _config.DefaultPlayerName : playerName;
                SetedNickName = _currentPlayerName;

                bool isConnected = _networkManager.StartClient();

                if (isConnected)
                {
                    _onConnected.OnNext(Unit.Default);
                }

                return isConnected;
            
        }

        public bool StartHost(string ipAddress)
        {
            try
            {
                SetupNetworkManager(ipAddress);
                if (_networkManager.IsListening)
                {
                    Debug.LogWarning("Network manager is already running");
                    return false;
                }

                bool isStarted = _networkManager.StartHost();
                if (isStarted)
                {
                    SpawnNetworkSpawnHandler();
                    SpawnHandlers();
                    _onConnected.OnNext(Unit.Default);
                }

                return isStarted;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Host start error: {ex.Message}");
                _onConnectionError.OnNext(ex.Message);
                return false;
            }
        }

        public GameObject SpawnNetworkObject(GameObject gameObject, Vector3 position = default, Quaternion rotation = default)
        {
            if (!IsConnected)
            {
                Debug.LogError("Network is not running");
                _onConnectionError.OnNext("Network is not running");
                return null;
            }

            try
            {
                return SpawnHandler.SpawnAsLocalOwner(gameObject, position, rotation);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to spawn network object: {ex.Message}");
                _onConnectionError.OnNext($"Failed to spawn network object: {ex.Message}");
                return null;
            }
        }

        public void DespawnNetworkObject(GameObject gameObject)
        {
            try
            {
                SpawnHandler.DestroyNetworkObject(gameObject);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to despawn network object: {ex.Message}");
                _onConnectionError.OnNext($"Failed to despawn network object: {ex.Message}");
            }
        }

        private void SpawnNetworkSpawnHandler()
        {
            var spawnHandlerObj = Instantiate(_config.NetworkSpawnHandlerPrefab);
            NetworkObject networkObject = spawnHandlerObj.GetComponent<NetworkObject>();
            networkObject.Spawn();

            _spawnHandler = spawnHandlerObj.GetComponent<NetworkSpawnHandler>();

            Debug.Log("NetworkSpawnHandler spawned successfully");
        }

        private void SpawnHandlers()
        {
            if (IsHost)
            {
                foreach (var item in _config.NetworkHandlersPrefabs)
                {
                    SpawnNetworkObject(item.gameObject);
                }
            }
        }

        public void Disconnect()
        {
            try
            {
                if (_networkManager != null && _networkManager.IsListening)
                {
                    Debug.Log("Disconnecting from network...");

                    UnsubscribeFromNetworkEvents();
                    Destroy(_networkManager.gameObject);
                    _networkManager = null;

                    _onDisconnected.OnNext(Unit.Default);
                    Debug.Log("Disconnected successfully");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error during disconnect: {ex.Message}");
                _onConnectionError.OnNext($"Error during disconnect: {ex.Message}");
            }
        }

        private void SubscribeToNetworkEvents()
        {
            if (_networkManager != null)
            {
                _networkManager.OnClientConnectedCallback += OnClientConnected;
                _networkManager.OnClientDisconnectCallback += OnClientDisconnected;
                _networkManager.OnServerStarted += OnServerStarted;
                _networkManager.OnTransportFailure += OnTransportFailure;
            }
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
            _networkManager.OnTransportFailure -= OnTransportFailure;
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

            if (clientId == _networkManager.LocalClientId)
            {
                Debug.Log("Local client disconnected");
                _onDisconnected.OnNext(Unit.Default);
                _onConnectionError.OnNext("Connection lost");
            }
        }

        private void OnServerStarted()
        {
            Debug.Log("Server started successfully");
        }

        private void OnTransportFailure()
        {
            Debug.LogError("Network transport failure detected");
            _onConnectionError.OnNext("Network transport failure");
            _onDisconnected.OnNext(Unit.Default);

            if (_networkManager != null)
            {
                UnsubscribeFromNetworkEvents();
                Destroy(_networkManager.gameObject);
                _networkManager = null;
            }
        }
    }
}