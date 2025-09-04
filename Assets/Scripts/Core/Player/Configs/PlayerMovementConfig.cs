using AgarIOSiphome.System.Configs;
using UnityEngine;

namespace AgarIOSiphome.Core.Player.Configs
{
    [CreateAssetMenu(menuName = "System/Configs/Player/Player Movement Config")]
    public class PlayerMovementConfig : ScriptableConfig
    {
        [Header("Movement Settings")]
        [SerializeField] private float _moveSpeed = 5f;
        [SerializeField] private float _keyboardMoveSpeed = 7f;

        public float MoveSpeed => _moveSpeed;
        public float KeyboardMoveSpeed => _keyboardMoveSpeed;
    }
}
