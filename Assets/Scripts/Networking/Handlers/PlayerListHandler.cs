using AgarIOSiphome.Networking.Models;
using AGarIOSiphome.Networking;
using AGarIOSiphome.Networking.Handlers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace AgarIOSiphome.Networking.Handlers
{
    public class PlayerListHandler : NetworkBehaviour, IPlayerListHandler
    {
        private readonly NetworkList<NetworkPlayer> _players = new();
        private readonly Subject<NetworkPlayer> _onPlayerAdded = new();
        private readonly Subject<NetworkPlayer> _onPlayerRemoved = new();

        public IObservable<NetworkPlayer> OnPlayerAdded => _onPlayerAdded;
        public IObservable<NetworkPlayer> OnPlayerRemoved => _onPlayerRemoved;

        public NetworkPlayer LocalPlayer => Players.FirstOrDefault(x => x.ClientId == NetworkManager.LocalClientId);

        private IEnumerable<NetworkPlayer> Players
        {
            get
            {
                List<NetworkPlayer> list = new List<NetworkPlayer>();

                foreach (NetworkPlayer player in _players)
                {
                    list.Add(player);
                }

                return list;
            }
        }

        private void Awake()
        {
            _players.OnListChanged += HandlePlayersListChanged;
            NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnected;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            _players.OnListChanged -= HandlePlayersListChanged;

            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnected;
            }
        }

        private void HandleClientConnected(ulong clientId)
        {
            var nickName = string.IsNullOrEmpty(NetworkHandler.SetedNickName)
                ? $"Player_{clientId}"
                : NetworkHandler.SetedNickName;

            var player = new NetworkPlayer(clientId, new FixedString32Bytes(nickName));
            if (clientId != NetworkManager.ServerClientId)
            {
                AddPlayerServerRpc(player);
            }

        }

        private void HandleClientDisconnected(ulong clientId)
        {
            if (IsServer)
            {
                RemovePlayerDirect(clientId);
            }
        }

        public override void OnNetworkSpawn()
        {
           HandleClientConnected(NetworkManager.LocalClientId);
        }

        private void HandlePlayersListChanged(NetworkListEvent<NetworkPlayer> changeEvent)
        {
            switch (changeEvent.Type)
            {
                case NetworkListEvent<NetworkPlayer>.EventType.Add:
                    _onPlayerAdded.OnNext(changeEvent.Value);
                    break;

                case NetworkListEvent<NetworkPlayer>.EventType.Remove:
                    _onPlayerRemoved.OnNext(changeEvent.Value);
                    break;
            }

        }

        [ServerRpc(RequireOwnership = false)]
        public void AddPlayerServerRpc(NetworkPlayer player)
        {
            if (IsServer) _players.Add(player);
        }

        private void RemovePlayerDirect(ulong clientId)
        {
            for (int i = _players.Count - 1; i >= 0; i--)
            {
                if (_players[i].ClientId == clientId)
                {
                    _players.RemoveAt(i);
                    break;
                }
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void UpdatePlayerScoreServerRpc(ulong clientId, uint score)
        {
            for (int i = 0; i < _players.Count; i++)
            {
                if (_players[i].ClientId == clientId)
                {
                    NetworkPlayer player = _players[i];
                    player.Score = score;
                    _players[i] = player;
                    break;
                }
            }
        }

        public void UpdatePlayerScore(ulong clientId, uint score)
        {
            UpdatePlayerScoreServerRpc(clientId, score);
        }

        [ServerRpc(RequireOwnership = false)]
        public void UpdatePlayerColorServerRpc(ulong clientId, int color)
        {
            for (int i = 0; i < _players.Count; i++)
            {
                if (_players[i].ClientId == clientId)
                {
                    NetworkPlayer player = _players[i];
                    player.Color = color;
                    _players[i] = player;
                    break;
                }
            }
        }

        public void UpdatePlayerColor(ulong clientId, int color)
        {
            UpdatePlayerColorServerRpc(clientId, color);
        }

        public NetworkPlayer GetPlayerById(ulong id)
        {
            return Players.FirstOrDefault(player => player.ClientId == id);
        }

        public bool PlayerExists(ulong id)
        {
            return Players.Any(player => player.ClientId == id);
        }

        public IEnumerator<NetworkPlayer> GetEnumerator() => _players.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}