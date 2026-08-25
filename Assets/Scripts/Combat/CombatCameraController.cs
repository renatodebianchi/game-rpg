using Unity.Cinemachine;
using UnityEngine;

namespace GameRpg.Combat
{
    /// <summary>
    /// Blends to a dedicated isometric combat camera when an encounter starts,
    /// and back to the exploration camera when it ends (fluid transitions per
    /// plan.md's "jogabilidade fluída e dinâmica" goal). Priority-based, so
    /// Cinemachine handles the actual blend.
    /// </summary>
    [RequireComponent(typeof(CinemachineCamera))]
    public class CombatCameraController : MonoBehaviour
    {
        private const int ActivePriority = 20;
        private const int InactivePriority = 0;

        [SerializeField] private CinemachineCamera combatCamera;
        [SerializeField] private Vector3 isometricEulerAngles = new Vector3(35.264f, 45f, 0f);

        private void Awake()
        {
            if (combatCamera == null)
            {
                combatCamera = GetComponent<CinemachineCamera>();
            }

            combatCamera.transform.rotation = Quaternion.Euler(isometricEulerAngles);
            SetActive(false);
        }

        public void FocusOn(Vector3 encounterCenter)
        {
            combatCamera.transform.position = encounterCenter - combatCamera.transform.forward * 10f;
        }

        public void SetActive(bool active)
        {
            combatCamera.Priority = active ? ActivePriority : InactivePriority;
        }
    }
}
