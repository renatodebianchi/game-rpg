using GameRpg.Characters;
using GameRpg.Core;
using UnityEngine;

namespace GameRpg.Demo
{
    /// <summary>
    /// Makes the player's created Character visible and movable in the Exploration
    /// scene (FR-001, FR-002, FR-005). Reads PendingPlayerCharacter set by
    /// CharacterCreationUI's "Finalizar" button, or creates a default Character when
    /// the scene is opened directly (FR-004), per
    /// contracts/scene-transition-contract.md.
    /// </summary>
    public class ExplorationCharacterController : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 4f;
        [SerializeField] private float walkBobAmplitude = 0.12f;
        [SerializeField] private float walkBobFrequency = 9f;
        [SerializeField] private float walkAnimationBlendSpeed = 12f;

        private Character _character;
        private Transform _rootTransform;
        private Transform _visualTransform;
        private SpriteRenderer _spriteRenderer;
        private float _walkCycleTime;
        private float _walkBlend;
        private bool _facingRight = true;

        private void Start()
        {
            _character = PendingPlayerCharacter.Consume() ?? CreateDefaultCharacter();
            SpawnSprite();
        }

        public Character Character => _character;

        private static Character CreateDefaultCharacter()
        {
            return new Character("player", maxHitPoints: 20, maxMovementPoints: 3, new CharacterAttributes(8, 8, 8, 8));
        }

        private void SpawnSprite()
        {
            var appearance = CharacterSpriteMapping.Resolve(_character.Visuals);
            var sprite = Resources.Load<Sprite>(appearance.SpriteResourceName);

            var rootGameObject = new GameObject("PlayerCharacter");
            _rootTransform = rootGameObject.transform;
            _rootTransform.position = Vector3.zero;

            // Billboard the flat sprite to face the scene's isometric camera
            // (Assets/Editor/ProjectBootstrap.cs, CreateExplorationScene) so it reads
            // as an upright character instead of an edge-on plane.
            if (Camera.main != null)
            {
                _rootTransform.rotation = Camera.main.transform.rotation;
            }

            // The visual is a child so the walk animation (bob/squash below) can
            // offset it in local space without disturbing the root's world
            // position, which is what movement and any future gameplay logic
            // (e.g. proximity checks) should read.
            var visualGameObject = new GameObject("Visual");
            _visualTransform = visualGameObject.transform;
            _visualTransform.SetParent(_rootTransform, worldPositionStays: false);

            _spriteRenderer = visualGameObject.AddComponent<SpriteRenderer>();
            _spriteRenderer.sprite = sprite;
            _spriteRenderer.color = appearance.TintColor;
        }

        private void Update()
        {
            if (_rootTransform == null)
            {
                return;
            }

            var horizontal = 0f;
            var vertical = 0f;

            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) horizontal -= 1f;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) horizontal += 1f;
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) vertical -= 1f;
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) vertical += 1f;

            var isMoving = horizontal != 0f || vertical != 0f;
            if (isMoving)
            {
                var direction = new Vector3(horizontal, 0f, vertical).normalized;
                _rootTransform.position += direction * (moveSpeed * Time.deltaTime);

                if (horizontal != 0f)
                {
                    _facingRight = horizontal > 0f;
                }
            }

            AnimateWalkCycle(isMoving);
        }

        /// <summary>
        /// Procedural walk cycle (bob + subtle squash-and-stretch + facing flip)
        /// so the sprite reads as walking rather than sliding, without needing
        /// hand-picked animation frames from the Kenney spritesheet (FR-002).
        /// </summary>
        private void AnimateWalkCycle(bool isMoving)
        {
            _walkBlend = Mathf.MoveTowards(_walkBlend, isMoving ? 1f : 0f, walkAnimationBlendSpeed * Time.deltaTime);

            if (isMoving)
            {
                _walkCycleTime += Time.deltaTime * walkBobFrequency;
            }

            var bob = Mathf.Abs(Mathf.Sin(_walkCycleTime)) * walkBobAmplitude * _walkBlend;
            _visualTransform.localPosition = new Vector3(0f, bob, 0f);

            var squash = bob / Mathf.Max(walkBobAmplitude, 0.0001f);
            var scaleX = (_facingRight ? 1f : -1f) * (1f + squash * 0.12f);
            var scaleY = 1f - squash * 0.08f;
            _visualTransform.localScale = new Vector3(scaleX, scaleY, 1f);
        }
    }
}
