using AgarIOSiphome.Core.Player.Components;
using UnityEngine;

namespace AgarIOSiphome.Core.Player
{
    [RequireComponent(typeof(PlayerMovement))]
    public class PlayerInstance : MonoBehaviour, IPlayerInstance
    {
    }
}