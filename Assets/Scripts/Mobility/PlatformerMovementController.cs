using GameRpg.Demo;
using GameRpg.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GameRpg.Mobility
{
    /// <summary>
    /// Feeds physics queries (ground/wall contact) into a PlatformerMovementState,
    /// reads player input, and applies the resulting velocity to the Rigidbody2D
    /// — it never decides a gameplay rule on its own (Princípio III; see
    /// contracts/movement-state-contract.md and contracts/charge-jump-contract.md).
    /// Spawns its own visual child (sprite + SpriteFlipbookAnimator), same
    /// runtime-constructed pattern as the other demo controllers in this project.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
    public class PlatformerMovementController : MonoBehaviour
    {
        [SerializeField] private float walkSpeed = 3f;
        [SerializeField] private float runSpeed = 6f;
        [SerializeField] private float jumpVelocity = 9f;
        [SerializeField] private float wallJumpHorizontalVelocity = 6f;
        [SerializeField] private float wallJumpVerticalVelocity = 9f;
        [SerializeField] private float maxChargeLeapVelocity = 14f;
        [SerializeField] private float maxFreeFallSpeed = 14f;
        [SerializeField] private float groundCheckDistance = 0.08f;
        [SerializeField] private float wallCheckDistance = 0.1f;

        private Rigidbody2D _rigidbody;
        private BoxCollider2D _collider;
        private Transform _visualTransform;
        private SpriteRenderer _spriteRenderer;
        private SpriteFlipbookAnimator _animator;
        private bool _facingRight = true;

        /// <summary>Exposed for tests/tools; the controller never mutates decisions itself,
        /// only feeds data in and applies the results.</summary>
        public PlatformerMovementState State { get; private set; }

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            _collider = GetComponent<BoxCollider2D>();
            _rigidbody.freezeRotation = true;
            State = new PlatformerMovementState();

            SpawnVisual();
            LoadAnimationFrames();
        }

        private void Start()
        {
            // Belt-and-suspenders alongside ProjectBootstrap's edit-time SetWorldBounds call
            // (Demo.BoundedFollowCamera, feature 004): wiring the target here too, at runtime,
            // matches the pattern already used by ExplorationCharacterController and means the
            // camera still follows even if the scene wasn't (re)generated via ProjectBootstrap.
            var boundedCamera = Camera.main != null ? Camera.main.GetComponent<BoundedFollowCamera>() : null;
            boundedCamera?.SetTarget(transform);

            BuildControlsHelpCard();
        }

        /// <summary>Static help card, top-right — lists every movement command this scene
        /// responds to, same convention as CombatDemoController/BattleArenaDemoController.</summary>
        private void BuildControlsHelpCard()
        {
            var canvasGameObject = new GameObject("MobilityControlsCanvas");
            var canvas = canvasGameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGameObject.AddComponent<CanvasScaler>();
            canvasGameObject.AddComponent<GraphicRaycaster>();

            if (FindAnyObjectByType<EventSystem>() == null)
            {
                var eventSystemGameObject = new GameObject("EventSystem");
                eventSystemGameObject.AddComponent<EventSystem>();
                eventSystemGameObject.AddComponent<StandaloneInputModule>();
            }

            var panelImage = DemoUiKit.CreatePanel(canvasGameObject.transform, new Vector2(0.72f, 0.62f), new Vector2(0.99f, 0.99f));
            panelImage.gameObject.name = "ControlsHelpCard";

            var titleText = DemoUiKit.CreateText(panelImage.transform, new Vector2(0.05f, 0.85f), new Vector2(0.95f, 0.97f));
            titleText.text = "Comandos";
            titleText.fontSize = 18;
            titleText.fontStyle = FontStyle.Bold;
            titleText.color = Color.black;

            var bodyText = DemoUiKit.CreateText(panelImage.transform, new Vector2(0.05f, 0.02f), new Vector2(0.95f, 0.83f));
            bodyText.fontSize = 13;
            bodyText.alignment = TextAnchor.UpperLeft;
            bodyText.color = Color.black;
            bodyText.text =
                "A / D ou setas\n  -> andar\n\n" +
                "Segurar Shift\n  -> correr\n\n" +
                "Espaço / W / seta-cima\n  -> pular (chão, até 2 no ar,\n     ou a partir da parede)\n\n" +
                "S / seta-baixo (segurar)\n  -> abaixar e carregar energia\n\n" +
                "Soltar S / seta-baixo\n  -> saltar pela energia acumulada";
        }

        private void SpawnVisual()
        {
            var visualGameObject = new GameObject("Visual");
            visualGameObject.transform.SetParent(transform, worldPositionStays: false);
            _visualTransform = visualGameObject.transform;
            _spriteRenderer = visualGameObject.AddComponent<SpriteRenderer>();
            _animator = visualGameObject.AddComponent<SpriteFlipbookAnimator>();
        }

        /// <summary>Best-effort pose mapping (research.md): the pack has no dedicated walk cycle
        /// or crouch/wall-slide pose, so Walking/Running both reuse the run cycle (differentiated
        /// by playback speed, not different frames) and Crouching reuses the low, knees-bent
        /// wind-up frames from the jump-start sequence (a much better stand-in than the idle
        /// pose) — the jump itself still uses the full 4-frame wind-up-to-launch sequence.</summary>
        private void LoadAnimationFrames()
        {
            var idle = Resources.LoadAll<Sprite>("Character/Idle");
            var run = Resources.LoadAll<Sprite>("Character/Run");
            var jumpStart = Resources.LoadAll<Sprite>("Character/JumpStart");
            var jumpAll = Resources.LoadAll<Sprite>("Character/JumpAll");

            // jumpstart_02/03: the deepest part of the wind-up crouch, reused as a standalone
            // "held crouch" loop (see the frames in Assets/Art/Platformer/Resources/Character/JumpStart).
            var crouchFrames = jumpStart.Length >= 4 ? jumpStart[2..4] : jumpStart;

            _animator.SetFrames(MovementStateKind.Idle, idle);
            _animator.SetFrames(MovementStateKind.Walking, run, frameInterval: 0.16f);
            _animator.SetFrames(MovementStateKind.Running, run, frameInterval: 0.07f);
            _animator.SetFrames(MovementStateKind.Jumping, jumpStart);
            _animator.SetFrames(MovementStateKind.DoubleJumping, jumpAll);
            _animator.SetFrames(MovementStateKind.Falling, jumpAll);
            _animator.SetFrames(MovementStateKind.Crouching, crouchFrames, frameInterval: 0.2f);
            _animator.SetFrames(MovementStateKind.WallSliding, jumpAll);
        }

        private void Update()
        {
            UpdatePhysicsQueries();

            State.IsCrouching = Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow);

            HandleHorizontalMovement();
            HandleJumpInput();
            HandleChargeJump(Time.deltaTime);
            ApplyFallSpeedCap();

            UpdateAnimationState();
        }

        /// <summary>Feeds IsGrounded/WallContactDirection from foot/side sensors positioned just
        /// outside the character's own collider (avoids self-hits).</summary>
        private void UpdatePhysicsQueries()
        {
            var bounds = _collider.bounds;
            var wasGrounded = State.IsGrounded;

            // The sensor box must sit strictly *outside* the character's own collider — if its
            // top edge overlaps bounds.min.y even slightly, Physics2D.BoxCast self-hits the
            // character's own collider at distance 0, which used to latch IsGrounded true while
            // still airborne (falling) and silently reset/bypass the aerial-jump limit.
            const float footSensorHalfHeight = 0.025f;
            const float footSensorGap = 0.01f;
            var footBoxSize = new Vector2(bounds.size.x * 0.9f, footSensorHalfHeight * 2f);
            var footOrigin = new Vector2(bounds.center.x, bounds.min.y - footSensorHalfHeight - footSensorGap);
            var groundHit = Physics2D.BoxCast(footOrigin, footBoxSize, 0f, Vector2.down, groundCheckDistance);

            // Only latch "grounded" while not actively moving upward, so a jump's initial
            // upward velocity isn't immediately re-grounded by the same-frame foot sensor.
            State.IsGrounded = groundHit.collider != null && _rigidbody.linearVelocity.y <= 0.01f;

            if (State.IsGrounded && !wasGrounded)
            {
                State.NotifyGrounded();
            }

            if (State.IsGrounded)
            {
                State.WallContactDirection = 0;
            }
            else
            {
                var rightOrigin = new Vector2(bounds.max.x + 0.02f, bounds.center.y);
                var leftOrigin = new Vector2(bounds.min.x - 0.02f, bounds.center.y);
                var wallRight = Physics2D.Raycast(rightOrigin, Vector2.right, wallCheckDistance);
                var wallLeft = Physics2D.Raycast(leftOrigin, Vector2.left, wallCheckDistance);

                if (wallRight.collider != null) State.WallContactDirection = 1;
                else if (wallLeft.collider != null) State.WallContactDirection = -1;
                else State.WallContactDirection = 0;
            }
        }

        private void HandleHorizontalMovement()
        {
            var horizontal = 0f;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) horizontal -= 1f;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) horizontal += 1f;

            if (horizontal != 0f)
            {
                _facingRight = horizontal > 0f;
            }

            // Edge case (FR-008): crouching holds the character in place.
            if (State.IsCrouching && State.IsGrounded)
            {
                horizontal = 0f;
            }

            var isRunning = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            var speed = isRunning && State.IsGrounded ? runSpeed : walkSpeed;

            _rigidbody.linearVelocity = new Vector2(horizontal * speed, _rigidbody.linearVelocity.y);
        }

        private void HandleJumpInput()
        {
            var jumpPressed = Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow);
            if (!jumpPressed)
            {
                return;
            }

            if (!State.IsGrounded && State.WallContactDirection != 0 && State.TryWallJump())
            {
                var pushDirection = -State.WallContactDirection;
                _rigidbody.linearVelocity = new Vector2(pushDirection * wallJumpHorizontalVelocity, wallJumpVerticalVelocity);
                return;
            }

            if (State.IsGrounded && State.TryGroundJump())
            {
                _rigidbody.linearVelocity = new Vector2(_rigidbody.linearVelocity.x, jumpVelocity);
                return;
            }

            if (State.TryAerialJump())
            {
                _rigidbody.linearVelocity = new Vector2(_rigidbody.linearVelocity.x, jumpVelocity);
            }
        }

        private void HandleChargeJump(float deltaSeconds)
        {
            // No-op unless PlatformerMovementState's internal guard (grounded + crouching) passes.
            State.AdvanceCharge(deltaSeconds);

            var chargeFraction = Mathf.Clamp01(State.CurrentChargeSeconds / State.MaxChargeSeconds);
            _spriteRenderer.color = Color.Lerp(Color.white, Color.yellow, chargeFraction);

            var crouchReleased = Input.GetKeyUp(KeyCode.S) || Input.GetKeyUp(KeyCode.DownArrow);
            if (!crouchReleased)
            {
                return;
            }

            var leapFraction = State.ReleaseCharge();
            if (leapFraction > 0f)
            {
                _rigidbody.linearVelocity = new Vector2(_rigidbody.linearVelocity.x, leapFraction * maxChargeLeapVelocity);
            }
        }

        /// <summary>Applies FR-006's decision (State.GetFallSpeedMultiplier(), not a value chosen here).</summary>
        private void ApplyFallSpeedCap()
        {
            var minVerticalVelocity = -maxFreeFallSpeed * State.GetFallSpeedMultiplier();
            if (_rigidbody.linearVelocity.y < minVerticalVelocity)
            {
                _rigidbody.linearVelocity = new Vector2(_rigidbody.linearVelocity.x, minVerticalVelocity);
            }
        }

        private void UpdateAnimationState()
        {
            var facingSign = _facingRight ? 1f : -1f;
            var verticalScale = State.IsCrouching && State.IsGrounded ? 0.75f : 1f;
            _visualTransform.localScale = new Vector3(facingSign, verticalScale, 1f);

            MovementStateKind kind;
            if (State.IsCrouching && State.IsGrounded)
            {
                kind = MovementStateKind.Crouching;
            }
            else if (State.IsWallSliding)
            {
                kind = MovementStateKind.WallSliding;
            }
            else if (!State.IsGrounded)
            {
                kind = State.JumpsUsed > 0
                    ? MovementStateKind.DoubleJumping
                    : (_rigidbody.linearVelocity.y > 0.05f ? MovementStateKind.Jumping : MovementStateKind.Falling);
            }
            else
            {
                var isRunning = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
                var horizontalSpeed = Mathf.Abs(_rigidbody.linearVelocity.x);
                kind = horizontalSpeed < 0.05f
                    ? MovementStateKind.Idle
                    : (isRunning ? MovementStateKind.Running : MovementStateKind.Walking);
            }

            _animator.SetState(kind);
        }
    }
}
