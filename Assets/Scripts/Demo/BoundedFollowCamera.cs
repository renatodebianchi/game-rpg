using UnityEngine;

namespace GameRpg.Demo
{
    /// <summary>
    /// Centers a target on the camera, except near the edges of the current
    /// map/arena, where the camera stops following and keeps the world's edge
    /// on screen instead of revealing empty space beyond it (FR-015; see
    /// specs/004-2d-real-time-combat/contracts/camera-bounds-contract.md).
    /// Shared by the Exploration and battle-arena scenes — plain script, not
    /// Cinemachine (research.md, "Decision: Câmera com clamp nas bordas").
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class BoundedFollowCamera : MonoBehaviour
    {
        private Camera _camera;
        private Transform _target;
        private float _minX;
        private float _maxX;
        private float _minY;
        private float _maxY;

        private void Awake()
        {
            _camera = GetComponent<Camera>();
        }

        public void SetTarget(Transform target)
        {
            _target = target;
        }

        public void SetWorldBounds(float minX, float maxX, float minY, float maxY)
        {
            _minX = minX;
            _maxX = maxX;
            _minY = minY;
            _maxY = maxY;
        }

        private void LateUpdate()
        {
            if (_target == null)
            {
                return;
            }

            var halfHeight = _camera.orthographicSize;
            var halfWidth = halfHeight * _camera.aspect;

            var clampedX = ClampAxis(_target.position.x, _minX, _maxX, halfWidth);
            var clampedY = ClampAxis(_target.position.y, _minY, _maxY, halfHeight);

            transform.position = new Vector3(clampedX, clampedY, transform.position.z);
        }

        /// <summary>Contract rule 3: when the world is smaller than the camera's
        /// view on this axis, stay centered instead of chasing the target.</summary>
        private static float ClampAxis(float desired, float min, float max, float halfExtent)
        {
            if (max - min <= halfExtent * 2f)
            {
                return (min + max) / 2f;
            }

            return Mathf.Clamp(desired, min + halfExtent, max - halfExtent);
        }
    }
}
