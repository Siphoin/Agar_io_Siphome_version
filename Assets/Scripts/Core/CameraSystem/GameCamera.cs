using AgarIOSiphome.Core.Player;
using Sirenix.OdinInspector;
using Unity.Cinemachine;
using UnityEngine;

namespace AgarIOSiphome.Core.CameraSystem
{
    [RequireComponent(typeof(CinemachineCamera))]
    public class GameCamera : MonoBehaviour
    {
        [SerializeField, ReadOnly] private PlayerInstance _currentPlayer;
        [SerializeField, ReadOnly] private CinemachineCamera _camera;
        private void LateUpdate()
        {
            if (_currentPlayer is null)
            {
                _currentPlayer = FindAnyObjectByType<PlayerInstance>();
                if (_currentPlayer != null)
                {
                    _camera.Follow = _currentPlayer.transform;
                }
            }
        }

        private void OnValidate()
        {
            if (_camera is null)
            {
                _camera = GetComponent<CinemachineCamera>();
            }
        }
    }
}