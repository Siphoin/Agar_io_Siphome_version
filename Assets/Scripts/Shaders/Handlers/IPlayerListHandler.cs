using AgarIOSiphome.Networking.Models;
using System.Collections.Generic;

namespace AgarIOSiphome.Networking.Handlers
{
    public interface IPlayerListHandler : IEnumerable<NetworkPlayer>
    {
    }
}