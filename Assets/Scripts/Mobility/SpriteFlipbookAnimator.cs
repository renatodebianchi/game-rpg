using System.Collections.Generic;
using UnityEngine;

namespace GameRpg.Mobility
{
    /// <summary>The movement states FR-011 requires visual feedback for.</summary>
    public enum MovementStateKind
    {
        Idle,
        Walking,
        Running,
        Jumping,
        DoubleJumping,
        Falling,
        Crouching,
        WallSliding
    }

    /// <summary>
    /// Cycles SpriteRenderer.sprite through the frames configured for the
    /// current MovementStateKind, at a fixed interval — replaces the need for
    /// an authored Animator/AnimatorController (research.md, "Animação por
    /// troca de sprite via código"). Presentation-only, exempt from Princípio
    /// III's test requirement.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteFlipbookAnimator : MonoBehaviour
    {
        [SerializeField] private float frameIntervalSeconds = 0.1f;

        private SpriteRenderer _spriteRenderer;
        private readonly Dictionary<MovementStateKind, Sprite[]> _framesByState = new Dictionary<MovementStateKind, Sprite[]>();
        private readonly Dictionary<MovementStateKind, float> _frameIntervalByState = new Dictionary<MovementStateKind, float>();
        private MovementStateKind _currentState = MovementStateKind.Idle;
        private int _frameIndex;
        private float _frameTimer;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        /// <summary>Assigns the frame sequence for a state; call once per state during setup.
        /// <paramref name="frameInterval"/> lets the same frame set play back at a different
        /// speed per state (e.g. Walking vs. Running reusing the same run-cycle frames) —
        /// defaults to the shared <see cref="frameIntervalSeconds"/> when omitted.</summary>
        public void SetFrames(MovementStateKind state, Sprite[] frames, float? frameInterval = null)
        {
            _framesByState[state] = frames;
            _frameIntervalByState[state] = frameInterval ?? frameIntervalSeconds;
        }

        /// <summary>Switches the active state, restarting its cycle from the first frame.</summary>
        public void SetState(MovementStateKind state)
        {
            if (state == _currentState)
            {
                return;
            }

            _currentState = state;
            _frameIndex = 0;
            _frameTimer = 0f;
            ApplyCurrentFrame();
        }

        private void Update()
        {
            if (!_framesByState.TryGetValue(_currentState, out var frames) || frames.Length == 0)
            {
                return;
            }

            var interval = _frameIntervalByState.TryGetValue(_currentState, out var stateInterval) ? stateInterval : frameIntervalSeconds;

            _frameTimer += Time.deltaTime;
            if (_frameTimer < interval)
            {
                return;
            }

            _frameTimer -= interval;
            _frameIndex = (_frameIndex + 1) % frames.Length;
            ApplyCurrentFrame();
        }

        private void ApplyCurrentFrame()
        {
            if (_framesByState.TryGetValue(_currentState, out var frames) && frames.Length > 0)
            {
                _spriteRenderer.sprite = frames[_frameIndex % frames.Length];
            }
        }
    }
}
