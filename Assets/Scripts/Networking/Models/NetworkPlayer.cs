using SouthPointe.Serialization.MessagePack;
using System;
using Unity.Collections;
using Unity.Netcode;

namespace AgarIOSiphome.Networking.Models
{
    [Serializable]
    public struct NetworkPlayer : INetworkSerializable, IEquatable<NetworkPlayer>
    {
        public ulong ClientId;
        public FixedString32Bytes NickName;
        public int Color;
        public uint Score;
        public Guid Guid;

        private static readonly MessagePackFormatter _formatter = new();

        public bool IsEmpty => false;

        public NetworkPlayer(ulong clientId, FixedString32Bytes nickName)
        {
            ClientId = clientId;
            NickName = nickName;
            Color = (int)clientId % 10;
            Score = 0;
            Guid = Guid.NewGuid();
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            if (serializer.IsReader)
            {
                byte[] bytes = default;
                serializer.SerializeValue(ref bytes);

                if (bytes != null && bytes.Length > 0)
                {
                    NetworkPlayer deserialized = _formatter.Deserialize<NetworkPlayer>(bytes);
                    this = deserialized;
                }
            }
            else
            {
                byte[] bytes = _formatter.Serialize(this);
                serializer.SerializeValue(ref bytes);
            }
        }

        public bool Equals(NetworkPlayer other)
        {
            return Guid == other.Guid;
        }

        public override string ToString()
        {
            return $"Player: {NickName} (ID: {ClientId}), Score: {Score}, Color: {Color}";
        }
    }
}