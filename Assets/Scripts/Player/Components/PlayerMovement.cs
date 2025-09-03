using UnityEngine;
using UnityEngine.InputSystem;

namespace AgarIOSiphome.Player.Components
{
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float keyboardMoveSpeed = 7f;

        private Vector2 _mousePosition;
        private Camera _mainCamera;
        private Vector2 _keyboardInput;

        private void Start()
        {
            _mainCamera = Camera.main;
        }

        private void Update()
        {
            HandleInput();
            HandleMovement();
        }

        private void HandleInput()
        {
            _mousePosition = Mouse.current.position.ReadValue();

            _keyboardInput = Vector2.zero;

            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
                _keyboardInput.y += 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
                _keyboardInput.y -= 1f;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
                _keyboardInput.x -= 1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
                _keyboardInput.x += 1f;

            if (_keyboardInput.magnitude > 1f)
                _keyboardInput.Normalize();
        }

        private void HandleMovement()
        {
            if (Mouse.current.leftButton.isPressed)
            {
                MoveWithMouse();
            }
            else if (_keyboardInput != Vector2.zero)
            {
                MoveWithKeyboard();
            }
        }

        private void MoveWithMouse()
        {
            Vector3 targetPosition = _mainCamera.ScreenToWorldPoint(_mousePosition);
            targetPosition.z = 0;

            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPosition,
                moveSpeed * Time.deltaTime
            );
        }

        private void MoveWithKeyboard()
        {
            Vector3 movement = new Vector3(_keyboardInput.x, _keyboardInput.y, 0f) *
                              keyboardMoveSpeed * Time.deltaTime;

            transform.Translate(movement);
        }
    }
}