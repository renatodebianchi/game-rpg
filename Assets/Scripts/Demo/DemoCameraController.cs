using UnityEngine;

namespace GameRpg.Demo
{
    /// <summary>
    /// Standard RTS/strategy-style camera controls for the manual-test demo:
    /// WASD/arrow keys to pan, scroll wheel (or Q/E) to zoom, middle-mouse-drag
    /// to pan, and right-mouse-drag to orbit around the ground point the camera
    /// is looking at. Not part of the shipped game — a manual-test convenience,
    /// same spirit as CombatDemoController.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class DemoCameraController : MonoBehaviour
    {
        [SerializeField] private float keyboardPanSpeed = 10f;
        [SerializeField] private float mousePanSpeed = 0.03f;
        [SerializeField] private float zoomSpeed = 5f;
        [SerializeField] private float scrollZoomSpeed = 2.5f;
        [SerializeField] private float orbitSpeed = 0.25f;
        [SerializeField] private float minOrthographicSize = 2f;
        [SerializeField] private float maxOrthographicSize = 20f;

        private Camera _camera;
        private Vector3 _lastMousePosition;

        private void Awake()
        {
            _camera = GetComponent<Camera>();
        }

        private void Update()
        {
            HandleKeyboardPan();
            HandleKeyboardZoom();
            HandleScrollZoom();
            HandleMiddleMouseDragPan();
            HandleRightMouseDragOrbit();
        }

        private void HandleKeyboardPan()
        {
            var horizontal = 0f;
            var vertical = 0f;

            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) horizontal -= 1f;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) horizontal += 1f;
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) vertical -= 1f;
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) vertical += 1f;

            if (horizontal == 0f && vertical == 0f)
            {
                return;
            }

            Pan(GroundRight() * horizontal + GroundForward() * vertical, keyboardPanSpeed * Time.deltaTime);
        }

        private void HandleKeyboardZoom()
        {
            if (Input.GetKey(KeyCode.E)) Zoom(-zoomSpeed * Time.deltaTime);
            if (Input.GetKey(KeyCode.Q)) Zoom(zoomSpeed * Time.deltaTime);
        }

        private void HandleScrollZoom()
        {
            var scroll = Input.mouseScrollDelta.y;
            if (Mathf.Abs(scroll) > 0.01f)
            {
                Zoom(-scroll * scrollZoomSpeed);
            }
        }

        private void HandleMiddleMouseDragPan()
        {
            if (Input.GetMouseButtonDown(2))
            {
                _lastMousePosition = Input.mousePosition;
            }

            if (Input.GetMouseButton(2))
            {
                var delta = Input.mousePosition - _lastMousePosition;
                var zoomScale = _camera.orthographicSize / 6f;
                Pan(GroundRight() * -delta.x + GroundForward() * -delta.y, mousePanSpeed * zoomScale);
                _lastMousePosition = Input.mousePosition;
            }
        }

        private void HandleRightMouseDragOrbit()
        {
            if (Input.GetMouseButtonDown(1))
            {
                _lastMousePosition = Input.mousePosition;
            }

            if (Input.GetMouseButton(1))
            {
                var delta = Input.mousePosition - _lastMousePosition;
                transform.RotateAround(GetGroundPivot(), Vector3.up, delta.x * orbitSpeed);
                _lastMousePosition = Input.mousePosition;
            }
        }

        private void Pan(Vector3 direction, float speed)
        {
            transform.position += direction * speed;
        }

        private void Zoom(float delta)
        {
            _camera.orthographicSize = Mathf.Clamp(_camera.orthographicSize + delta, minOrthographicSize, maxOrthographicSize);
        }

        private Vector3 GroundRight() => Vector3.ProjectOnPlane(transform.right, Vector3.up).normalized;

        private Vector3 GroundForward() => Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;

        private Vector3 GetGroundPivot()
        {
            var plane = new Plane(Vector3.up, Vector3.zero);
            var ray = new Ray(transform.position, transform.forward);
            return plane.Raycast(ray, out var distance) ? ray.GetPoint(distance) : transform.position;
        }
    }
}
