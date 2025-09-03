using UnityEngine;

namespace AgarIOSiphome.Shaders.Handlers
{
    public class InfiniteGridHandler : MonoBehaviour
    {
        [Header("Grid Settings")]
        [SerializeField] private float _gridScale = 1.0f;
        [SerializeField] private float _gridThickness = 0.01f;
        [SerializeField] private Color _gridColor = new(0.3f, 0.3f, 0.3f, 1.0f);
        [SerializeField] private Color _backgroundColor = new(0.1f, 0.1f, 0.1f, 1.0f);

        private Material _gridMaterial;
        private Camera _mainCamera;
        private SpriteRenderer _spriteRenderer;

        private void Start()
        {
            _mainCamera = Camera.main;
            SetupFullScreenGrid();
        }

        private void SetupFullScreenGrid()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>() ?? gameObject.AddComponent<SpriteRenderer>();

            Shader gridShader = Shader.Find("Custom/InfiniteGridFullScreen");
            if (gridShader != null)
            {
                _gridMaterial = new Material(gridShader);
                _spriteRenderer.material = _gridMaterial;
            }

            _spriteRenderer.sortingOrder = -1000;
            _spriteRenderer.drawMode = SpriteDrawMode.Sliced;

            UpdateGridSize();
            ApplyMaterialProperties();
        }

        private void Update()
        {
            if (_mainCamera == null)
                _mainCamera = Camera.main;

            if (_mainCamera == null)
                return;

            UpdateGridPosition();
            UpdateGridSize();
            ApplyMaterialProperties();
        }

        private void UpdateGridPosition()
        {
            Vector3 cameraPosition = _mainCamera.transform.position;
            transform.position = new Vector3(cameraPosition.x, cameraPosition.y, 10f);
        }

        private void UpdateGridSize()
        {
            if (_mainCamera == null || _spriteRenderer == null)
                return;

            float cameraHeight = _mainCamera.orthographicSize * 2f;
            float cameraWidth = cameraHeight * _mainCamera.aspect;

            _spriteRenderer.size = new Vector2(cameraWidth, cameraHeight);
            transform.localScale = Vector3.one;
        }

        private void ApplyMaterialProperties()
        {
            if (_gridMaterial == null)
                return;

            _gridMaterial.SetFloat("_GridScale", _gridScale);
            _gridMaterial.SetFloat("_GridThickness", _gridThickness);
            _gridMaterial.SetColor("_GridColor", _gridColor);
            _gridMaterial.SetColor("_BackgroundColor", _backgroundColor);
        }

        private void OnDestroy()
        {
            if (_gridMaterial != null)
            {
                DestroyImmediate(_gridMaterial);
            }
        }

        private void OnValidate()
        {
            ApplyMaterialProperties();

            if (_mainCamera != null)
                UpdateGridSize();
        }

        private void OnRectTransformDimensionsChange()
        {
            UpdateGridSize();
        }
    }
}