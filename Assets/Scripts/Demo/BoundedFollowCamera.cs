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

        // [SerializeField] so values assigned by an Editor-time bootstrap script (via
        // SetTarget/SetWorldBounds, before the scene is saved) actually persist into the
        // saved scene — plain private fields are dropped by Unity's serializer, which
        // silently left this camera with a null target and zeroed bounds at runtime
        // (the camera never moved at all, having nothing to follow or clamp against).
        [SerializeField] private Transform target;
        [SerializeField] private float minX;
        [SerializeField] private float maxX;
        [SerializeField] private float minY;
        [SerializeField] private float maxY;

        private void Awake()
        {
            _camera = GetComponent<Camera>();
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }

        public void SetWorldBounds(float newMinX, float newMaxX, float newMinY, float newMaxY)
        {
            minX = newMinX;
            maxX = newMaxX;
            minY = newMinY;
            maxY = newMaxY;
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            var halfHeight = _camera.orthographicSize;
            var halfWidth = halfHeight * _camera.aspect;

            var clampedX = ClampAxis(target.position.x, minX, maxX, halfWidth);
            var clampedY = ClampAxis(target.position.y, minY, maxY, halfHeight);

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
